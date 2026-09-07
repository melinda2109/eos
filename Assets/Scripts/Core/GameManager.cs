using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GameMode { None, VsAi, VsFriend }

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

    [Header("Optional Services")]
    public AudioManager audioManager;
    public ParticleEffects particleEffects;

    private const int EosBaseMaxHealth = 60;
    private const int EosBaseAttack = 25;
    private const int EosBaseDefense = 6;
    private const int EnemyBaseMaxHealth = 60;
    private const int EnemyBaseAttack = 20;
    private const int EnemyBaseDefense = 6;
    private const int EnemyHealthPerNight = 10;
    private const int EnemyAttackPerNight = 2;

    public static GameManager Instance { get; private set; }
    public BattleSystem Battle { get; private set; }
    public LocalDuelSystem Duel { get; private set; }
    private GameMode currentMode = GameMode.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (uiManager == null) uiManager = FindFirstObjectByType<UIManager>();
        if (eosPlayer == null || nightEosPlayer == null) return;
        ResetToBaseStats();
    }

    private void Start()
    {
        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.RemoveAllListeners();
            nextRoundButton.onClick.AddListener(RestartBattle);
            nextRoundButton.gameObject.SetActive(false);
        }
        if (backgroundRenderer != null && dawnSprite != null) backgroundRenderer.sprite = dawnSprite;
        uiManager?.ShowModeSelect(StartAiMode, StartDuelMode);
    }

    public void StartAiMode()
    {
        currentMode = GameMode.VsAi;
        Battle = GetComponent<BattleSystem>();
        if (Battle == null) Battle = gameObject.AddComponent<BattleSystem>();
        Battle.Initialize(this);
    }

    public void StartDuelMode()
    {
        currentMode = GameMode.VsFriend;
        Duel = GetComponent<LocalDuelSystem>();
        if (Duel == null) Duel = gameObject.AddComponent<LocalDuelSystem>();
        Duel.Initialize(this);
    }

    public bool IsActivePlayer(Player player)
    {
        return Battle != null && Battle.State == BattleState.PlayerTurn && player == eosPlayer;
    }

    public void SetRoundLabel(string text)
    {
        if (roundText != null) roundText.text = text;
    }

    public void SetNightText(int night) => SetRoundLabel($"NIGHT // {night:00}");

    public void ResetToBaseStats()
    {
        eosPlayer.Initialize("Eos", EosBaseMaxHealth, EosBaseAttack, EosBaseDefense);
        nightEosPlayer.Initialize("Night Eos", EnemyBaseMaxHealth, EnemyBaseAttack, EnemyBaseDefense);
    }

    public void ScaleEnemyForNight(int night)
    {
        int extra = Mathf.Max(0, night - 1);
        int maxHealth = EnemyBaseMaxHealth + extra * EnemyHealthPerNight;
        int attack = EnemyBaseAttack + extra * EnemyAttackPerNight;
        int defense = EnemyBaseDefense + extra / 2;
        nightEosPlayer.Initialize("Night Eos", maxHealth, attack, defense);
    }

    public void ShowRestartButton()
    {
        if (nextRoundButton == null) return;
        nextRoundButton.gameObject.SetActive(true);
        TextMeshProUGUI label = nextRoundButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = "RESTART";
    }

    public void RestartBattle()
    {
        if (nextRoundButton != null) nextRoundButton.gameObject.SetActive(false);
        if (currentMode == GameMode.VsFriend) Duel?.Restart();
        else Battle?.Restart();
    }
}
