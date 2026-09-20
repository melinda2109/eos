using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Health Bars")]
    public Slider eosHealthBar;
    public Slider nightEosHealthBar;
    public TextMeshProUGUI eosHealthText;
    public TextMeshProUGUI nightEosHealthText;
    public Image eosHealthFill;
    public Image nightEosHealthFill;

    [Header("Mana")]
    public Slider eosManaBar;
    public TextMeshProUGUI eosManaText;
    public Slider nightEosManaBar;
    public TextMeshProUGUI nightEosManaText;
    [Tooltip("Optional: assign ui_healthbar_frame_night_eos.png here for a themed Mana bar frame instead of a flat rectangle.")]
    public Sprite manaBarFrameSprite;
    [Tooltip("Optional: assign ui_healthbar_fill_night_eos.png here for a themed Mana bar fill instead of a flat rectangle.")]
    public Sprite manaBarFillSprite;

    [Header("Action Buttons")]
    public Button[] eosActionButtons;
    public Button[] nightEosActionButtons;

    [Header("Battle Log")]
    public ScrollRect battleLogScrollRect;
    public TextMeshProUGUI battleLogText;
    public TextMeshProUGUI gameStatusText;

    [Header("Players")]
    public Player eosPlayer;
    public Player nightEosPlayer;

    private BattleSystem battle;
    private readonly List<Button> actionButtons = new List<Button>();
    private Coroutine eosHealthAnimation;
    private Coroutine nightHealthAnimation;
    private Coroutine manaAnimation;

    private GameObject upgradePanel;
    private readonly List<Button> upgradeButtons = new List<Button>();
    private readonly List<TextMeshProUGUI> upgradeTitleTexts = new List<TextMeshProUGUI>();
    private readonly List<TextMeshProUGUI> upgradeDescTexts = new List<TextMeshProUGUI>();

    private GameObject modePanel;
    private AudioManager audioManager;
    private Coroutine nightManaAnimation;

    private void Start()
    {
        if (eosHealthBar == null) eosHealthBar = FindSlider("EosHealthBar");
        if (nightEosHealthBar == null) nightEosHealthBar = FindSlider("NightEosHealthBar");
        if (battleLogText == null) battleLogText = FindText("BattleLogText");
        if (gameStatusText == null) gameStatusText = FindText("GameStatusText");
        EnsureActionButtons();
        battle = GameManager.Instance != null ? GameManager.Instance.Battle : FindFirstObjectByType<BattleSystem>();
        if (battle != null) BindBattle(battle);
    }

    public void BindBattle(BattleSystem system)
    {
        battle = system;
        EnsureActionButtons();
        foreach (Button button in actionButtons)
            button.onClick.RemoveAllListeners();
        if (actionButtons.Count >= 3)
        {
            actionButtons[0].onClick.AddListener(battle.OnAttackButton);
            actionButtons[1].onClick.AddListener(battle.OnHealButton);
            actionButtons[2].onClick.AddListener(battle.OnDefendButton);
        }
        SetActionButtonsInteractable(true);
    }

    private void EnsureActionButtons()
    {
        if (actionButtons.Count >= 3 && actionButtons[0] != null)
        {
            HideLegacyNightEosButtons();
            FindManaBar();
            return;
        }
        actionButtons.Clear();
        string[] names = { "EosAttackButton", "EosHealButton", "EosDefendButton" };
        foreach (string objectName in names)
        {
            GameObject obj = GameObject.Find(objectName);
            Button button = obj != null ? obj.GetComponent<Button>() : null;
            if (button != null) actionButtons.Add(button);
        }
        eosActionButtons = actionButtons.ToArray();
        HideLegacyNightEosButtons();
        FindManaBar();
    }

    private void HideLegacyNightEosButtons()
    {
        if (nightEosActionButtons != null)
        {
            foreach (Button button in nightEosActionButtons)
            {
                if (button != null) button.gameObject.SetActive(false);
            }
        }
        GameObject legacy = GameObject.Find("NightEosAttackButton");
        if (legacy != null) legacy.SetActive(false);
    }

    private void FindManaBar()
    {
        if (eosManaBar == null)
        {
            GameObject barObj = GameObject.Find("EosManaBar");
            if (barObj != null) eosManaBar = barObj.GetComponent<Slider>();
        }
        if (eosManaText == null)
        {
            GameObject textObj = GameObject.Find("ManaText");
            if (textObj != null) eosManaText = textObj.GetComponent<TextMeshProUGUI>();
        }
    }

    private void CreateNightManaBarIfNeeded()
    {
        if (nightEosManaBar != null) return;
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        GameObject root = new GameObject("NightEosManaBar", typeof(RectTransform), typeof(Slider));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(-400f, -25f);
        rootRect.sizeDelta = new Vector2(160f, 14f);
        Slider slider = root.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 60f;
        slider.interactable = false;

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(root.transform, false);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one; bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;
        Image backgroundImage = background.GetComponent<Image>();
        if (manaBarFrameSprite != null)
        {
            backgroundImage.sprite = manaBarFrameSprite;
            backgroundImage.color = Color.white;
        }
        else
        {
            backgroundImage.color = new Color(0.2f, 0.03f, 0.1f, 0.95f);
        }

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(root.transform, false);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one; fillRect.offsetMin = new Vector2(3f, 3f); fillRect.offsetMax = new Vector2(-3f, -3f);
        Image fillImage = fill.GetComponent<Image>();
        if (manaBarFillSprite != null)
        {
            fillImage.sprite = manaBarFillSprite;
            fillImage.color = Color.white;
        }
        else
        {
            fillImage.color = new Color(0.86f, 0.32f, 0.62f, 1f);
        }
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        nightEosManaBar = slider;

        GameObject textObject = new GameObject("NightManaText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvas.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f); textRect.anchorMax = new Vector2(0.5f, 0.5f); textRect.anchoredPosition = new Vector2(-400f, -8f); textRect.sizeDelta = new Vector2(160f, 20f);
        nightEosManaText = textObject.GetComponent<TextMeshProUGUI>();
        nightEosManaText.alignment = TextAlignmentOptions.Center;
        nightEosManaText.fontSize = 12f;
        nightEosManaText.fontStyle = FontStyles.Bold;
        nightEosManaText.color = new Color(1f, 0.6f, 0.8f);
    }

    public void RefreshDuel(Player p1, Player p2, int manaP1, int manaP2, int maxMana)
    {
        CreateNightManaBarIfNeeded();
        eosPlayer = p1; nightEosPlayer = p2;
        RefreshHealth(eosHealthBar, eosHealthText, p1, ref eosHealthAnimation);
        RefreshHealth(nightEosHealthBar, nightEosHealthText, p2, ref nightHealthAnimation);
        if (eosManaBar != null)
        {
            eosManaBar.maxValue = maxMana;
            if (manaAnimation != null) StopCoroutine(manaAnimation);
            manaAnimation = StartCoroutine(AnimateSlider(eosManaBar, manaP1));
        }
        if (eosManaText != null) eosManaText.text = $"MANA  {manaP1}/{maxMana}";
        if (nightEosManaBar != null)
        {
            nightEosManaBar.maxValue = maxMana;
            if (nightManaAnimation != null) StopCoroutine(nightManaAnimation);
            nightManaAnimation = StartCoroutine(AnimateSlider(nightEosManaBar, manaP2));
        }
        if (nightEosManaText != null) nightEosManaText.text = $"MANA  {manaP2}/{maxMana}";
    }

    public void ShowModeSelect(Action onAi, Action onFriend)
    {
        EnsureActionButtons();
        BuildModeSelectPanel(onAi, onFriend);
    }

    private void BuildModeSelectPanel(Action onAi, Action onFriend)
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject panel = new GameObject("ModeSelectPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 0.92f);
        panel.transform.SetAsLastSibling();
        modePanel = panel;

        GameObject titleObject = new GameObject("ModeTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 120f);
        titleRect.sizeDelta = new Vector2(700f, 60f);
        TextMeshProUGUI title = titleObject.GetComponent<TextMeshProUGUI>();
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 30f;
        title.fontStyle = FontStyles.Bold;
        title.color = Color.white;
        title.text = "CHOOSE MODE";

        Button templateButton = actionButtons.Count > 0 ? actionButtons[0] : FindFirstObjectByType<Button>();
        if (templateButton == null) return;

        string[] labels = { "SURVIVE THE NIGHT\n<size=70%>(vs AI)</size>", "DUEL A FRIEND\n<size=70%>(2 Players)</size>" };
        float[] x = { -170f, 170f };
        Action[] callbacks = { onAi, onFriend };
        for (int i = 0; i < 2; i++)
        {
            GameObject card = Instantiate(templateButton.gameObject, panel.transform);
            card.name = $"ModeButton{i}";
            Button button = card.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.interactable = true;
            RectTransform cardRect = card.transform as RectTransform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2(x[i], -20f);
            cardRect.sizeDelta = new Vector2(280f, 140f);
            TextMeshProUGUI label = card.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = labels[i];
                label.fontSize = 20f;
                label.fontStyle = FontStyles.Bold;
                label.enableWordWrapping = true;
            }
            AttachNavigationSounds(button);
            Action callback = callbacks[i];
            button.onClick.AddListener(() =>
            {
                Audio?.PlayUISelectSound();
                modePanel.SetActive(false);
                callback?.Invoke();
            });
        }
    }

    public void RefreshAll(Player eos, Player night, int mana, int maxMana)
    {
        eosPlayer = eos; nightEosPlayer = night;
        RefreshHealth(eosHealthBar, eosHealthText, eos, ref eosHealthAnimation);
        RefreshHealth(nightEosHealthBar, nightEosHealthText, night, ref nightHealthAnimation);
        if (eosManaBar != null)
        {
            eosManaBar.maxValue = maxMana;
            if (manaAnimation != null) StopCoroutine(manaAnimation);
            manaAnimation = StartCoroutine(AnimateSlider(eosManaBar, mana));
        }
        if (eosManaText != null) eosManaText.text = $"MANA  {mana}/{maxMana}";
        RefreshButtonStates();
    }

    private void RefreshHealth(Slider slider, TextMeshProUGUI text, Player player, ref Coroutine animation)
    {
        if (player == null) return;
        if (text != null) text.text = $"{player.currentHealth}/{player.maxHealth}";
        if (slider != null)
        {
            slider.maxValue = player.maxHealth;
            if (animation != null) StopCoroutine(animation);
            animation = StartCoroutine(AnimateSlider(slider, player.currentHealth));
        }
    }

    private IEnumerator AnimateSlider(Slider slider, float target)
    {
        if (slider == null) yield break;
        float start = slider.value;
        float elapsed = 0f;
        while (elapsed < 0.35f)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(start, target, elapsed / 0.35f);
            yield return null;
        }
        slider.value = target;
    }

    public void RefreshButtonStates()
    {
        if (battle == null) return;
        bool canAct = battle.CanPlayerAct;
        for (int i = 0; i < actionButtons.Count; i++)
        {
            bool enabled = canAct;
            if (i == 1 && (battle.CurrentMana < battle.HealManaCost || eosPlayer == null || eosPlayer.currentHealth >= eosPlayer.maxHealth)) enabled = false;
            actionButtons[i].interactable = enabled;
        }
    }

    public void SetActionButtonsInteractable(bool enabled)
    {
        foreach (Button button in actionButtons) button.interactable = enabled;
    }

    public void SetActionButtonsVisible(bool visible)
    {
        foreach (Button button in actionButtons)
            if (button != null) button.gameObject.SetActive(visible);
    }

    public void SetBattleStatus(string title, string subtitle)
    {
        if (gameStatusText != null) gameStatusText.text = $"{title}\n<size=70%>{subtitle}</size>";
        RefreshButtonStates();
    }

    public void AppendLog(string line)
    {
        if (battleLogText == null) return;
        if (!string.IsNullOrEmpty(battleLogText.text)) battleLogText.text += "\n";
        battleLogText.text += line;

        // The Content/Text rects have no auto-layout, so they must be grown by hand to fit
        // the accumulated text, or the ScrollRect sees nothing to scroll and clips new lines.
        Canvas.ForceUpdateCanvases();
        battleLogText.ForceMeshUpdate();
        float newHeight = Mathf.Max(300f, battleLogText.preferredHeight);
        RectTransform textRect = battleLogText.rectTransform;
        textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, newHeight);
        RectTransform contentRect = battleLogText.transform.parent as RectTransform;
        if (contentRect != null)
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, newHeight);

        if (battleLogScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            battleLogScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void ClearLog()
    {
        if (battleLogText != null) battleLogText.text = string.Empty;
    }

    public void ShowUpgradeChoices(List<UpgradeOption> options, Action<UpgradeOption> onChosen)
    {
        EnsureUpgradePanel();
        if (upgradePanel == null) return;
        SetActionButtonsInteractable(false);
        upgradePanel.SetActive(true);
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            if (i < options.Count)
            {
                UpgradeOption option = options[i];
                upgradeButtons[i].gameObject.SetActive(true);
                upgradeButtons[i].interactable = true;
                upgradeTitleTexts[i].text = option.Title;
                upgradeDescTexts[i].text = option.Description;
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() =>
                {
                    Audio?.PlayUISelectSound();
                    upgradePanel.SetActive(false);
                    onChosen?.Invoke(option);
                });
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void EnsureUpgradePanel()
    {
        if (upgradePanel != null) return;
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject panel = new GameObject("UpgradePanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 0.88f);
        panel.transform.SetAsLastSibling();
        upgradePanel = panel;

        GameObject titleObject = new GameObject("UpgradeTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 170f);
        titleRect.sizeDelta = new Vector2(700f, 50f);
        TextMeshProUGUI title = titleObject.GetComponent<TextMeshProUGUI>();
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 26f;
        title.fontStyle = FontStyles.Bold;
        title.color = Color.white;
        title.text = "CHOOSE AN UPGRADE";

        Button templateButton = actionButtons.Count > 0 ? actionButtons[0] : FindFirstObjectByType<Button>();
        if (templateButton == null) return;
        float[] cardX = { -260f, 0f, 260f };
        for (int i = 0; i < 3; i++)
        {
            GameObject card = Instantiate(templateButton.gameObject, panel.transform);
            card.name = $"UpgradeCard{i}";
            Button button = card.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.interactable = true;
            RectTransform cardRect = card.transform as RectTransform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2(cardX[i], -20f);
            cardRect.sizeDelta = new Vector2(230f, 220f);

            TextMeshProUGUI titleLabel = card.GetComponentInChildren<TextMeshProUGUI>(true);
            RectTransform titleLabelRect = titleLabel.rectTransform;
            titleLabelRect.anchorMin = new Vector2(0f, 1f);
            titleLabelRect.anchorMax = new Vector2(1f, 1f);
            titleLabelRect.pivot = new Vector2(0.5f, 1f);
            titleLabelRect.anchoredPosition = new Vector2(0f, -14f);
            titleLabelRect.sizeDelta = new Vector2(210f, 36f);
            titleLabel.alignment = TextAlignmentOptions.Center;
            titleLabel.fontSize = 18f;
            titleLabel.fontStyle = FontStyles.Bold;
            titleLabel.enableWordWrapping = true;

            GameObject descObj = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(card.transform, false);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 1f);
            descRect.offsetMin = new Vector2(14f, 14f);
            descRect.offsetMax = new Vector2(-14f, -56f);
            TextMeshProUGUI descLabel = descObj.GetComponent<TextMeshProUGUI>();
            descLabel.alignment = TextAlignmentOptions.Center;
            descLabel.fontSize = 14f;
            descLabel.color = new Color(0.2f, 0.12f, 0.08f);
            descLabel.enableWordWrapping = true;

            AttachNavigationSounds(button);
            upgradeButtons.Add(button);
            upgradeTitleTexts.Add(titleLabel);
            upgradeDescTexts.Add(descLabel);
        }
        upgradePanel.SetActive(false);
    }

    private AudioManager Audio
    {
        get
        {
            if (audioManager == null)
            {
                audioManager = GameManager.Instance != null ? GameManager.Instance.audioManager : null;
                if (audioManager == null) audioManager = FindAnyObjectByType<AudioManager>();
            }
            return audioManager;
        }
    }

    /// <summary>
    /// Adds a blip when the highlight moves onto a card and a confirm chime when it is
    /// taken. Hooks both PointerEnter and Select so mouse and gamepad navigation agree.
    /// </summary>
    private void AttachNavigationSounds(Button button)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        EventTrigger.Entry hover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        hover.callback.AddListener(_ => Audio?.PlayUIMoveSound());
        trigger.triggers.Add(hover);

        EventTrigger.Entry select = new EventTrigger.Entry { eventID = EventTriggerType.Select };
        select.callback.AddListener(_ => Audio?.PlayUIMoveSound());
        trigger.triggers.Add(select);
    }

    private Slider FindSlider(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        return obj != null ? obj.GetComponent<Slider>() : null;
    }

    private TextMeshProUGUI FindText(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        return obj != null ? obj.GetComponent<TextMeshProUGUI>() : null;
    }
}
