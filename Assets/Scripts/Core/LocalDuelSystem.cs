using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum DuelAction { None, Attack, Heal, Defend }

public class LocalDuelSystem : MonoBehaviour
{
    private const int StartMana = 60;
    private const int MaxManaValue = 100;
    private const int HealManaCost = 25;
    private const int HealAmount = 15;
    private const int DefendBonus = 8;

    private GameManager game;
    private Player p1;
    private Player p2;
    private UIManager ui;
    private AudioManager audio;
    private ParticleEffects fx;

    private int p1Mana;
    private int p2Mana;
    private int p1BaseDefense;
    private int p2BaseDefense;
    private bool p1Defending;
    private bool p2Defending;
    private bool isP1Turn;
    private bool busy;
    private bool matchOver;

    public void Initialize(GameManager owner)
    {
        game = owner;
        p1 = owner.eosPlayer;
        p2 = owner.nightEosPlayer;
        ui = owner.uiManager;
        audio = owner.audioManager;
        fx = owner.particleEffects;

        game?.ResetToBaseStats();
        p1BaseDefense = p1.defensePower;
        p2BaseDefense = p2.defensePower;
        p1Mana = StartMana;
        p2Mana = StartMana;
        p1Defending = false;
        p2Defending = false;
        isP1Turn = true;
        busy = false;
        matchOver = false;

        ui?.SetActionButtonsVisible(false);
        game?.SetRoundLabel("LOCAL DUEL");
        ui?.ClearLog();
        ui?.RefreshDuel(p1, p2, p1Mana, p2Mana, MaxManaValue);
        AnnounceTurn();
        ui?.AppendLog("A friendly duel begins. Eos moves first.");
        LogControlScheme();
    }

    private void LogControlScheme()
    {
        int pads = Gamepad.all.Count;
        if (pads >= 2)
        {
            ui?.AppendLog("Both seats are on gamepads.");
            return;
        }
        if (pads == 1)
            ui?.AppendLog("Only one gamepad found — Player 2 plays on the keyboard: <color=#69c8ff>J / K / L</color>.");
        else
            ui?.AppendLog("No gamepad found — Player 1: <color=#69c8ff>A / S / D</color>, Player 2: <color=#69c8ff>J / K / L</color>.");
    }

    private void AnnounceTurn()
    {
        string hint = ControlHint(isP1Turn);
        if (isP1Turn)
            ui?.SetBattleStatus("PLAYER 1's TURN", $"Eos — {hint}");
        else
            ui?.SetBattleStatus("PLAYER 2's TURN", $"Night Eos — {hint}");
    }

    /// <summary>Which device this seat is actually driving, so the prompt matches reality.</summary>
    private static bool HasPadFor(bool playerOne) => Gamepad.all.Count > (playerOne ? 0 : 1);

    private static string ControlHint(bool playerOne)
    {
        if (HasPadFor(playerOne)) return "(A) Attack  (X) Heal  (B) Defend";
        return playerOne ? "(A) Attack  (S) Heal  (D) Defend" : "(J) Attack  (K) Heal  (L) Defend";
    }

    /// <summary>
    /// Reads the active seat's input. Uses that seat's gamepad when one is plugged in and
    /// otherwise falls back to a keyboard half, so a duel is playable with one pad or none.
    /// </summary>
    private static DuelAction ReadAction(bool playerOne)
    {
        int padIndex = playerOne ? 0 : 1;
        if (Gamepad.all.Count > padIndex)
        {
            Gamepad pad = Gamepad.all[padIndex];
            if (pad.buttonSouth.wasPressedThisFrame) return DuelAction.Attack;
            if (pad.buttonWest.wasPressedThisFrame) return DuelAction.Heal;
            if (pad.buttonEast.wasPressedThisFrame) return DuelAction.Defend;
            return DuelAction.None;
        }

        Keyboard kb = Keyboard.current;
        if (kb == null) return DuelAction.None;
        if (playerOne)
        {
            if (kb.aKey.wasPressedThisFrame) return DuelAction.Attack;
            if (kb.sKey.wasPressedThisFrame) return DuelAction.Heal;
            if (kb.dKey.wasPressedThisFrame) return DuelAction.Defend;
        }
        else
        {
            if (kb.jKey.wasPressedThisFrame) return DuelAction.Attack;
            if (kb.kKey.wasPressedThisFrame) return DuelAction.Heal;
            if (kb.lKey.wasPressedThisFrame) return DuelAction.Defend;
        }
        return DuelAction.None;
    }

    private void Update()
    {
        if (matchOver || busy) return;

        DuelAction action = ReadAction(isP1Turn);
        if (action == DuelAction.None) return;

        Player actor = isP1Turn ? p1 : p2;
        int actorMana = isP1Turn ? p1Mana : p2Mana;
        string actorName = isP1Turn ? "Eos" : "Night Eos";

        switch (action)
        {
            case DuelAction.Attack:
                StartCoroutine(AttackRoutine());
                break;

            case DuelAction.Heal:
                // Tell the player why nothing happened instead of swallowing the press.
                if (actorMana < HealManaCost)
                    ui?.AppendLog($"{actorName} lacks Mana to heal ({actorMana}/{HealManaCost}).");
                else if (actor.currentHealth >= actor.maxHealth)
                    ui?.AppendLog($"{actorName} is already at full Health.");
                else
                    StartCoroutine(HealRoutine());
                break;

            case DuelAction.Defend:
                StartCoroutine(DefendRoutine());
                break;
        }
    }

    private IEnumerator AttackRoutine()
    {
        busy = true;
        Player attacker = isP1Turn ? p1 : p2;
        Player target = isP1Turn ? p2 : p1;
        attacker.PlayAttack();
        audio?.PlayAttackSound();
        fx?.PlayAttackEffect(attacker.transform.position, isP1Turn);
        yield return new WaitForSeconds(0.2f);
        int damage = CalculateDamage(attacker, target);
        target.TakeDamage(damage);
        audio?.PlayDamageSound();
        fx?.PlayDamageEffect(target.transform.position);
        if (isP1Turn) p1Mana = Mathf.Min(MaxManaValue, p1Mana + 10);
        else p2Mana = Mathf.Min(MaxManaValue, p2Mana + 10);
        string attackerName = isP1Turn ? "Eos" : "Night Eos";
        string targetName = isP1Turn ? "Night Eos" : "Eos";
        ui?.AppendLog($"{attackerName} hits {targetName} for <color=#ffcf66>{damage}</color> damage and gains <color=#69c8ff>+10 Mana</color>.");
        ui?.RefreshDuel(p1, p2, p1Mana, p2Mana, MaxManaValue);
        yield return EndTurn();
    }

    private IEnumerator HealRoutine()
    {
        busy = true;
        Player actor = isP1Turn ? p1 : p2;
        if (isP1Turn) p1Mana -= HealManaCost; else p2Mana -= HealManaCost;
        actor.PlayHeal();
        audio?.PlayHealSound();
        fx?.PlayHealEffect(actor.transform.position);
        yield return new WaitForSeconds(0.25f);
        actor.Heal(HealAmount);
        string name = isP1Turn ? "Eos" : "Night Eos";
        ui?.AppendLog($"{name} restores <color=#8dff9b>{HealAmount} Health</color> for {HealManaCost} Mana.");
        ui?.RefreshDuel(p1, p2, p1Mana, p2Mana, MaxManaValue);
        yield return EndTurn();
    }

    private IEnumerator DefendRoutine()
    {
        busy = true;
        Player actor = isP1Turn ? p1 : p2;
        if (isP1Turn)
        {
            p1Defending = true;
            actor.defensePower = p1BaseDefense + DefendBonus;
        }
        else
        {
            p2Defending = true;
            actor.defensePower = p2BaseDefense + DefendBonus;
        }
        actor.PlayDefend();
        audio?.PlayDefendSound();
        string name = isP1Turn ? "Eos" : "Night Eos";
        ui?.AppendLog($"{name} braces for impact. Defense rises by <color=#69c8ff>{DefendBonus}</color>.");
        ui?.RefreshDuel(p1, p2, p1Mana, p2Mana, MaxManaValue);
        yield return new WaitForSeconds(0.3f);
        yield return EndTurn();
    }

    private IEnumerator EndTurn()
    {
        if (CheckMatchOver())
        {
            busy = false;
            yield break;
        }
        isP1Turn = !isP1Turn;
        ExpireDefenseIfNeeded();
        AnnounceTurn();
        busy = false;
    }

    private void ExpireDefenseIfNeeded()
    {
        if (isP1Turn && p1Defending)
        {
            p1.defensePower = p1BaseDefense;
            p1Defending = false;
            ui?.AppendLog("Eos lowers the guard. Temporary Defense has expired.");
        }
        else if (!isP1Turn && p2Defending)
        {
            p2.defensePower = p2BaseDefense;
            p2Defending = false;
            ui?.AppendLog("Night Eos lowers the guard. Temporary Defense has expired.");
        }
    }

    private int CalculateDamage(Player attacker, Player target)
    {
        return Mathf.Max(1, attacker.attackPower - target.defensePower + Random.Range(-2, 3));
    }

    private bool CheckMatchOver()
    {
        if (p2.currentHealth <= 0)
        {
            matchOver = true;
            ui?.SetBattleStatus("PLAYER 1 WINS", "Eos claims victory");
            ui?.AppendLog("<color=#8dff9b>Eos wins the duel!</color>");
            audio?.PlayVictorySound();
            fx?.PlayVictoryEffect(p1.transform.position);
            game?.ShowRestartButton();
            return true;
        }
        if (p1.currentHealth <= 0)
        {
            matchOver = true;
            ui?.SetBattleStatus("PLAYER 2 WINS", "Night Eos claims victory");
            ui?.AppendLog("<color=#ff8d8d>Night Eos wins the duel!</color>");
            audio?.PlayVictorySound();
            fx?.PlayVictoryEffect(p2.transform.position);
            game?.ShowRestartButton();
            return true;
        }
        return false;
    }

    public void Restart()
    {
        matchOver = false;
        busy = false;
        game?.ResetToBaseStats();
        p1BaseDefense = p1.defensePower;
        p2BaseDefense = p2.defensePower;
        p1Defending = false;
        p2Defending = false;
        p1Mana = StartMana;
        p2Mana = StartMana;
        isP1Turn = true;
        game?.SetRoundLabel("LOCAL DUEL");
        ui?.ClearLog();
        ui?.RefreshDuel(p1, p2, p1Mana, p2Mana, MaxManaValue);
        AnnounceTurn();
        ui?.AppendLog("A new duel begins. Eos moves first.");
        LogControlScheme();
    }
}
