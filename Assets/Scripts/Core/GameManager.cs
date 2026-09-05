using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Players")]
    public Player eosPlayer;
    public Player nightEosPlayer;
    
    [Header("UI")]
    public UIManager uiManager;
    public Button nextRoundButton;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI gameStatusText;
    public TextMeshProUGUI battleLogText;
    
    [Header("Background")]
    public SpriteRenderer backgroundRenderer;
    public Sprite dawnSprite;
    public Sprite nightSprite;
    
    [Header("Audio")]
    public AudioManager audioManager;
    
    [Header("Effects")]
    public ParticleEffects particleEffects;
    
    private int currentRound = 1;
    private bool gameEnded = false;
    public bool eosIsActivePlayer;
    private string battleLog = "";
    
    // Special abilities
    private enum SpecialAbility { DamageReduction, AttackBoost, LowHealthHeal }
    private SpecialAbility eosAbility;
    private SpecialAbility nightEosAbility;
    private bool eosAbilityActivated = false;
    private bool nightEosAbilityActivated = false;
    
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeGame();
    }

    void InitializeGame()
    {
        // Initialize players with random stats
        int eosDefense = Random.Range(10, 16);
        int eosAttack = Random.Range(15, 21);
        int nightEosDefense = Random.Range(10, 16);
        int nightEosAttack = Random.Range(15, 21);
        
        eosPlayer.Initialize("Eos", 100, eosAttack, eosDefense);
        nightEosPlayer.Initialize("Night Eos", 100, nightEosAttack, nightEosDefense);
        
        // Assign random abilities
        eosAbility = (SpecialAbility)Random.Range(0, 3);
        nightEosAbility = (SpecialAbility)Random.Range(0, 3);

        // TEMP TESTING: force Low Health Heal on both so you can see it trigger. Remove later.
        eosAbility = SpecialAbility.LowHealthHeal;
        nightEosAbility = SpecialAbility.LowHealthHeal;
        
        // Randomly select starting player
        eosIsActivePlayer = (Random.value > 0.5f);
        
        // Update UI
        UpdateUI();
        UpdateBackground();
        uiManager.UpdateHealthBars();
        nextRoundButton.onClick.AddListener(NextRound);
        
        // Display initial game state
        string eosAbilityName = GetAbilityName(eosAbility);
        string nightEosAbilityName = GetAbilityName(nightEosAbility);
        
        battleLog = $"<color=#FFD700>Eos</color>: Attack = {eosAttack}, Defense = {eosDefense}, Ability = {eosAbilityName}\n" +
                   $"<color=#9370DB>Night Eos</color>: Attack = {nightEosAttack}, Defense = {nightEosDefense}, Ability = {nightEosAbilityName}\n\n" +
                   $"Round {currentRound} begins!\n" +
                   $"{(eosIsActivePlayer ? "Eos" : "Night Eos")} goes first.";
        
        UpdateBattleLogDisplay();
        gameStatusText.text = eosIsActivePlayer ? "Eos's Turn" : "Night Eos's Turn";
    }

    void UpdateBattleLogDisplay()
    {
        battleLogText.text = battleLog;

        // Content (and the text box itself) have no auto-layout, so grow them to fit
        // the text or the log gets clipped/pushed out of view and can't be scrolled
        Canvas.ForceUpdateCanvases();
        battleLogText.ForceMeshUpdate();
        float newHeight = Mathf.Max(300f, battleLogText.preferredHeight);

        RectTransform textRect = battleLogText.rectTransform;
        textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, newHeight);

        RectTransform contentRect = battleLogText.transform.parent as RectTransform;
        if (contentRect != null)
        {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, newHeight);
        }

        // Snap to the newest entry
        ScrollRect scrollRect = battleLogText.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    string GetAbilityName(SpecialAbility ability)
    {
        switch (ability)
        {
            case SpecialAbility.DamageReduction:
                return "Half Damage";
            case SpecialAbility.AttackBoost:
                return "50% Attack Boost";
            case SpecialAbility.LowHealthHeal:
                return "Low Health Heal";
            default:
                return "None";
        }
    }
    
    public void PlayerAttack(Player attacker, Player target)
    {
        if (gameEnded) return;
        
        // Calculate base damage
        int damage = attacker.attackPower;
        bool abilityActivated = false;
        
        // Check for attack boost ability
        if ((attacker == eosPlayer && eosAbility == SpecialAbility.AttackBoost) ||
            (attacker == nightEosPlayer && nightEosAbility == SpecialAbility.AttackBoost))
        {
            // 25% chance to activate
            if (Random.value < 0.25f)
            {
                damage = Mathf.RoundToInt(damage * 1.5f);
                abilityActivated = true;

                if (attacker == eosPlayer)
                    eosAbilityActivated = true;
                else
                    nightEosAbilityActivated = true;

                uiManager.ShowAbilityActivation(attacker == eosPlayer);
                particleEffects.PlayAbilityEffect(attacker.transform.position, attacker == eosPlayer);
            }
        }
        
        // Check for damage reduction ability
        bool damageReduced = false;
        if ((target == eosPlayer && eosAbility == SpecialAbility.DamageReduction) ||
            (target == nightEosPlayer && nightEosAbility == SpecialAbility.DamageReduction))
        {
            // 25% chance to activate
            if (Random.value < 0.25f)
            {
                damage = Mathf.RoundToInt(damage * 0.5f);
                damageReduced = true;
                abilityActivated = true;

                if (target == eosPlayer)
                    eosAbilityActivated = true;
                else
                    nightEosAbilityActivated = true;

                uiManager.ShowAbilityActivation(target == eosPlayer);
                particleEffects.PlayAbilityEffect(target.transform.position, target == eosPlayer);
            }
        }

        // Apply defense
        int finalDamage = Mathf.Max(0, damage - target.defensePower);

        // Track health before damage to detect threshold crossing (matches C++ design)
        int targetHealthBefore = target.currentHealth;

        // Apply damage
        target.TakeDamage(finalDamage);
        
        // Play attack sound and effects
        audioManager.PlayAttackSound();
        particleEffects.PlayAttackEffect(attacker.transform.position);
        particleEffects.PlayDamageEffect(target.transform.position);
        
        // Update battle log
        string attackerName = attacker == eosPlayer ? "<color=#FFD700>Eos</color>" : "<color=#9370DB>Night Eos</color>";
        string targetName = target == eosPlayer ? "<color=#FFD700>Eos</color>" : "<color=#9370DB>Night Eos</color>";
        
        battleLog += $"\n\n{attackerName} attacks!";
        
        if (abilityActivated)
        {
            if (damageReduced)
                battleLog += $"\n{targetName} activates <color=#00FF00>Half Damage</color> ability!";
            else if (damage > attacker.attackPower)
                battleLog += $"\n{attackerName} activates <color=#FF0000>Attack Boost</color> ability!";
        }
        else
        {
            battleLog += "\nNo ability activated.";
        }
        
        battleLog += $"\n{targetName} takes {finalDamage} damage (Attack: {damage} - Defense: {target.defensePower})";
        battleLog += $"\n{targetName} has {target.currentHealth} health remaining";
        
        UpdateBattleLogDisplay();

        // Update health bars
        uiManager.UpdateHealthBars();
        
        // Check for low health heal ability
        if ((target == eosPlayer && eosAbility == SpecialAbility.LowHealthHeal) ||
            (target == nightEosPlayer && nightEosAbility == SpecialAbility.LowHealthHeal))
        {
            if (targetHealthBefore >= 30 && target.currentHealth < 30 && Random.value < 1.0f) // TEMP TESTING: was 0.25f
            {
                target.Heal(5);
                battleLog += $"\n{targetName} activates <color=#00FF00>Low Health Heal</color> ability!";
                battleLog += $"\n{targetName} heals 5 health points!";
                battleLog += $"\n{targetName} now has {target.currentHealth} health";
                UpdateBattleLogDisplay();

                audioManager.PlayHealSound();
                particleEffects.PlayHealEffect(target.transform.position);

                if (target == eosPlayer)
                    eosAbilityActivated = true;
                else
                    nightEosAbilityActivated = true;

                uiManager.ShowAbilityActivation(target == eosPlayer);
                uiManager.UpdateHealthBars();
            }
        }
        
        // Check for game over
        if (target.currentHealth <= 0)
        {
            EndGame(attacker);
        }
        else
        {
            SwitchActivePlayer();
        }
    }
    
    void SwitchActivePlayer()
    {
        eosIsActivePlayer = !eosIsActivePlayer;
        gameStatusText.text = eosIsActivePlayer ? "Eos's Turn" : "Night Eos's Turn";
        UpdateBackground();
    }
    
    void UpdateBackground()
    {
        backgroundRenderer.sprite = eosIsActivePlayer ? dawnSprite : nightSprite;
        StartCoroutine(FadeBackground());
    }
    
    IEnumerator FadeBackground()
    {
        SpriteRenderer sr = backgroundRenderer;
        Color startColor = sr.color;
        startColor.a = 0.7f;
        Color endColor = sr.color;
        endColor.a = 1f;
        
        float duration = 0.5f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            sr.color = Color.Lerp(startColor, endColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        sr.color = endColor;
    }
    
    void NextRound()
    {
        if (gameEnded)
        {
            RestartGame();
            return;
        }
        
        currentRound++;
        UpdateUI();
        
        // Reset ability activation flags
        eosAbilityActivated = false;
        nightEosAbilityActivated = false;
        
        // Add round separator to battle log
        battleLog += $"\n\n<color=#FFFFFF>========== Round {currentRound} ==========</color>";
        UpdateBattleLogDisplay();
    }
    
    void UpdateUI()
    {
        roundText.text = $"Round {currentRound}";
    }
    
    void EndGame(Player winner)
    {
        gameEnded = true;
        string winnerName = winner == eosPlayer ? "Eos" : "Night Eos";
        gameStatusText.text = $"{winnerName} Wins!";
        
        battleLog += $"\n\n<color=#00FF00>{winnerName} has won the battle!</color>";
        UpdateBattleLogDisplay();
        
        nextRoundButton.GetComponentInChildren<TextMeshProUGUI>().text = "Restart";
        audioManager.PlayVictorySound();
        particleEffects.PlayVictoryEffect(winner.transform.position);
    }
    
    void RestartGame()
    {
        gameEnded = false;
        currentRound = 1;
        eosIsActivePlayer = (Random.value > 0.5f);
        
        eosPlayer.ResetHealth();
        nightEosPlayer.ResetHealth();
        
        // Reassign random abilities
        eosAbility = (SpecialAbility)Random.Range(0, 3);
        nightEosAbility = (SpecialAbility)Random.Range(0, 3);
        
        // Reset battle log
        battleLog = "";
        
        UpdateUI();
        UpdateBackground();
        
        // Display initial game state
        string eosAbilityName = GetAbilityName(eosAbility);
        string nightEosAbilityName = GetAbilityName(nightEosAbility);
        
        battleLog = $"<color=#FFD700>Eos</color>: Attack = {eosPlayer.attackPower}, Defense = {eosPlayer.defensePower}, Ability = {eosAbilityName}\n" +
                   $"<color=#9370DB>Night Eos</color>: Attack = {nightEosPlayer.attackPower}, Defense = {nightEosPlayer.defensePower}, Ability = {nightEosAbilityName}\n\n" +
                   $"Round {currentRound} begins!\n" +
                   $"{(eosIsActivePlayer ? "Eos" : "Night Eos")} goes first.";
        
        UpdateBattleLogDisplay();
        gameStatusText.text = eosIsActivePlayer ? "Eos's Turn" : "Night Eos's Turn";
        nextRoundButton.GetComponentInChildren<TextMeshProUGUI>().text = "Next Round";
        
        uiManager.UpdateHealthBars();
    }
    
    public bool IsActivePlayer(Player player)
    {
        return (eosIsActivePlayer && player == eosPlayer) || 
               (!eosIsActivePlayer && player == nightEosPlayer);
    }
}
