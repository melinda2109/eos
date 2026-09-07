using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { PlayerTurn, EnemyTurn, Won, Lost }

public class BattleSystem : MonoBehaviour
{
    public BattleState State { get; private set; } = BattleState.PlayerTurn;
    public int CurrentMana { get; private set; }
    public int MaxMana => 100 + bonusMaxMana;
    public int MaxHealth => player != null ? player.maxHealth : 100;
    public int NightNumber => nightNumber;
    public int HealManaCost => healManaCost;

    private GameManager game;
    private Player player;
    private Player enemy;
    private UIManager ui;
    private AudioManager audio;
    private ParticleEffects fx;
    private int playerBaseDefense;
    private bool playerDefending;
    private bool initialized;
    private Coroutine actionRoutine;

    private int nightNumber = 1;
    private int bonusMaxMana;
    private int healManaCost = 25;
    private float lifestealPercent;
    private int defendManaRestore;

    public void Initialize(GameManager owner)
    {
        game = owner;
        player = owner.eosPlayer;
        enemy = owner.nightEosPlayer;
        ui = owner.uiManager;
        audio = owner.audioManager;
        fx = owner.particleEffects;
        playerBaseDefense = player.defensePower;
        CurrentMana = 60;
        initialized = true;
        State = BattleState.PlayerTurn;
        game?.SetNightText(nightNumber);
        ui?.BindBattle(this);
        ui?.RefreshAll(player, enemy, CurrentMana, MaxMana);
        ui?.SetBattleStatus("YOUR TURN", "Choose an action");
        ui?.AppendLog("The arena falls silent. Eos moves first.");
    }

    public bool CanPlayerAct => initialized && State == BattleState.PlayerTurn && actionRoutine == null;

    public void OnAttackButton()
    {
        if (!CanPlayerAct) return;
        actionRoutine = StartCoroutine(PlayerAttackRoutine());
    }

    public void OnHealButton()
    {
        if (!CanPlayerAct || CurrentMana < healManaCost || player.currentHealth >= player.maxHealth) return;
        actionRoutine = StartCoroutine(PlayerHealRoutine());
    }

    public void OnDefendButton()
    {
        if (!CanPlayerAct) return;
        actionRoutine = StartCoroutine(PlayerDefendRoutine());
    }

    private IEnumerator PlayerAttackRoutine()
    {
        State = BattleState.EnemyTurn;
        ui?.SetBattleStatus("EOS ATTACKS", "A clean strike restores Mana");
        player.PlayAttack();
        audio?.PlayAttackSound();
        fx?.PlayAttackEffect(player.transform.position, true);
        yield return new WaitForSeconds(0.2f);
        int damage = CalculateDamage(player, enemy);
        enemy.TakeDamage(damage);
        audio?.PlayDamageSound();
        fx?.PlayDamageEffect(enemy.transform.position);
        if (lifestealPercent > 0f)
        {
            int healed = Mathf.RoundToInt(damage * lifestealPercent);
            if (healed > 0)
            {
                player.HealSilent(healed);
                ui?.AppendLog($"Eos drains <color=#8dff9b>{healed}</color> Health from the strike.");
            }
        }
        CurrentMana = Mathf.Min(MaxMana, CurrentMana + 10);
        ui?.AppendLog($"Eos hits Night Eos for <color=#ffcf66>{damage}</color> damage and gains <color=#69c8ff>+10 Mana</color>.");
        ui?.RefreshAll(player, enemy, CurrentMana, MaxMana);
        if (!CheckBattleOver())
            yield return EnemyTurnRoutine();
        actionRoutine = null;
        ui?.RefreshButtonStates();
    }

    private IEnumerator PlayerHealRoutine()
    {
        State = BattleState.EnemyTurn;
        CurrentMana -= healManaCost;
        ui?.SetBattleStatus("EOS HEALS", "Mana is converted into vitality");
        player.PlayHeal();
        audio?.PlayHealSound();
        fx?.PlayHealEffect(player.transform.position);
        yield return new WaitForSeconds(0.25f);
        player.Heal(15);
        ui?.AppendLog($"Eos restores <color=#8dff9b>15 Health</color> for {healManaCost} Mana.");
        ui?.RefreshAll(player, enemy, CurrentMana, MaxMana);
        yield return EnemyTurnRoutine();
        actionRoutine = null;
        ui?.RefreshButtonStates();
    }

    private IEnumerator PlayerDefendRoutine()
    {
        State = BattleState.EnemyTurn;
        playerDefending = true;
        player.defensePower = playerBaseDefense + 8;
        player.PlayDefend();
        ui?.SetBattleStatus("EOS DEFENDS", "+8 Defense until your next turn");
        ui?.AppendLog("Eos braces for impact. Defense rises by <color=#69c8ff>8</color>.");
        if (defendManaRestore > 0)
        {
            CurrentMana = Mathf.Min(MaxMana, CurrentMana + defendManaRestore);
            ui?.AppendLog($"The guard stance channels <color=#69c8ff>+{defendManaRestore} Mana</color>.");
        }
        ui?.RefreshAll(player, enemy, CurrentMana, MaxMana);
        yield return new WaitForSeconds(0.3f);
        yield return EnemyTurnRoutine();
        actionRoutine = null;
        ui?.RefreshButtonStates();
    }

    private IEnumerator EnemyTurnRoutine()
    {
        if (CheckBattleOver()) yield break;
        State = BattleState.EnemyTurn;
        ui?.SetBattleStatus("NIGHT EOS THINKS", "The shadows are moving...");
        yield return new WaitForSeconds(0.45f);

        bool enemyHeals = enemy.currentHealth <= 24 && Random.value < 0.35f;
        if (enemyHeals)
        {
            enemy.Heal(8);
            audio?.PlayHealSound();
            fx?.PlayHealEffect(enemy.transform.position);
            ui?.AppendLog("Night Eos recovers <color=#d9a7ff>8 Health</color> in the darkness.");
        }
        else
        {
            enemy.PlayAttack();
            audio?.PlayAttackSound();
            fx?.PlayAttackEffect(enemy.transform.position, false);
            yield return new WaitForSeconds(0.2f);
            int damage = CalculateDamage(enemy, player);
            player.TakeDamage(damage);
            audio?.PlayDamageSound();
            fx?.PlayDamageEffect(player.transform.position);
            ui?.AppendLog($"Night Eos strikes Eos for <color=#ff7f7f>{damage}</color> damage.");
        }

        ui?.RefreshAll(player, enemy, CurrentMana, MaxMana);
        if (!CheckBattleOver())
        {
            State = BattleState.PlayerTurn;
            if (playerDefending)
            {
                player.defensePower = playerBaseDefense;
                playerDefending = false;
                ui?.AppendLog("Eos lowers the guard. Temporary Defense has expired.");
            }
            ui?.SetBattleStatus("YOUR TURN", "Choose Attack, Heal, or Defend");
        }
    }

    private int CalculateDamage(Player attacker, Player target)
    {
        return Mathf.Max(1, attacker.attackPower - target.defensePower + Random.Range(-2, 3));
    }

    private bool CheckBattleOver()
    {
        if (enemy.currentHealth <= 0)
        {
            State = BattleState.Won;
            ui?.SetBattleStatus("VICTORY", $"Night {nightNumber} survived");
            ui?.AppendLog($"<color=#8dff9b>Eos wins the battle. Night {nightNumber} survived.</color>");
            ui?.SetActionButtonsInteractable(false);
            audio?.PlayVictorySound();
            fx?.PlayVictoryEffect(player.transform.position);
            OfferUpgrade();
            return true;
        }
        if (player.currentHealth <= 0)
        {
            State = BattleState.Lost;
            ui?.SetBattleStatus("DEFEAT", $"Eos fell on Night {nightNumber}");
            ui?.AppendLog($"<color=#ff8d8d>Night Eos wins the battle. Eos fell on Night {nightNumber}.</color>");
            ui?.SetActionButtonsInteractable(false);
            audio?.PlayVictorySound();
            fx?.PlayVictoryEffect(enemy.transform.position);
            game?.ShowRestartButton();
            return true;
        }
        return false;
    }

    private void OfferUpgrade()
    {
        List<UpgradeOption> pool = UpgradePool.All();
        for (int i = 0; i < pool.Count; i++)
        {
            int j = Random.Range(i, pool.Count);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        List<UpgradeOption> offered = pool.GetRange(0, Mathf.Min(3, pool.Count));
        ui?.ShowUpgradeChoices(offered, OnUpgradeChosen);
    }

    private void OnUpgradeChosen(UpgradeOption option)
    {
        option.Apply?.Invoke(this);
        ui?.AppendLog($"<color=#ffe58a>Upgrade chosen:</color> {option.Title} — {option.Description}");
        StartNextNight();
    }

    private void StartNextNight()
    {
        nightNumber++;
        int restoreAmount = Mathf.RoundToInt(player.maxHealth * 0.3f);
        player.HealSilent(restoreAmount);
        CurrentMana = Mathf.Min(MaxMana, 60);
        playerDefending = false;
        player.defensePower = playerBaseDefense;
        game?.ScaleEnemyForNight(nightNumber);
        game?.SetNightText(nightNumber);
        State = BattleState.PlayerTurn;
        ui?.SetActionButtonsInteractable(true);
        ui?.RefreshAll(player, enemy, CurrentMana, MaxMana);
        ui?.SetBattleStatus("YOUR TURN", $"Night {nightNumber} begins. Choose an action");
        ui?.AppendLog($"<color=#ffffff>== Night {nightNumber} ==</color> Night Eos grows stronger.");
    }

    public void ApplyVigor(int amount)
    {
        player.maxHealth += amount;
        player.HealSilent(amount);
    }

    public void ApplyAttackBonus(int amount) => player.attackPower += amount;

    public void ApplyDefenseBonus(int amount)
    {
        playerBaseDefense += amount;
        player.defensePower += amount;
    }

    public void ApplyMaxManaBonus(int amount)
    {
        bonusMaxMana += amount;
        CurrentMana = Mathf.Min(MaxMana, CurrentMana + amount);
    }

    public void ReduceHealCost(int amount) => healManaCost = Mathf.Max(10, healManaCost - amount);

    public void AddLifesteal(float percent) => lifestealPercent += percent;

    public void AddDefendManaRestore(int amount) => defendManaRestore += amount;

    public void Restart()
    {
        if (actionRoutine != null) StopCoroutine(actionRoutine);
        nightNumber = 1;
        bonusMaxMana = 0;
        healManaCost = 25;
        lifestealPercent = 0f;
        defendManaRestore = 0;
        game?.ResetToBaseStats();
        playerBaseDefense = player.defensePower;
        playerDefending = false;
        CurrentMana = 60;
        State = BattleState.PlayerTurn;
        actionRoutine = null;
        game?.SetNightText(nightNumber);
        ui?.ClearLog();
        ui?.RefreshAll(player, enemy, CurrentMana, MaxMana);
        ui?.SetActionButtonsInteractable(true);
        ui?.SetBattleStatus("YOUR TURN", "Choose an action");
        ui?.AppendLog("A new run begins. Eos moves first.");
    }
}
