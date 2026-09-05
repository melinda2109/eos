using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;


public class UIManager : MonoBehaviour
{
    [Header("Health Bars")]
    public Slider eosHealthBar;
    public Slider nightEosHealthBar;
    public TextMeshProUGUI eosHealthText;
    public TextMeshProUGUI nightEosHealthText;
    public Image eosHealthFill;
    public Image nightEosHealthFill;
    
    [Header("Action Buttons")]
    public Button[] eosActionButtons;
    public Button[] nightEosActionButtons;
    
    [Header("Battle Log")]
    public ScrollRect battleLogScrollRect;
    
    [Header("Players")]
    public Player eosPlayer;
    public Player nightEosPlayer;
    
    [Header("Ability Icons")]
    public GameObject eosAbilityIcon;
    public GameObject nightEosAbilityIcon;

    private Coroutine eosPulseCoroutine;
    private Coroutine nightEosPulseCoroutine;

    void Start()
    {
        SetupActionButtons();
        UpdateHealthBars();
    }
    
    void SetupActionButtons()
    {
        // Eos buttons
        if (eosActionButtons.Length >= 1)
        {
            eosActionButtons[0].onClick.AddListener(() => eosPlayer.Attack());
        }
        
        // Night Eos buttons
        if (nightEosActionButtons.Length >= 1)
        {
            nightEosActionButtons[0].onClick.AddListener(() => nightEosPlayer.Attack());
        }
        
        // Add hover animations to all buttons
        foreach (Button btn in eosActionButtons)
        {
            AddButtonHoverEffects(btn);
        }
        
        foreach (Button btn in nightEosActionButtons)
        {
            AddButtonHoverEffects(btn);
        }
    }
    
    void AddButtonHoverEffects(Button button)
    {
        // Add hover animation
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();
        
        // Add pointer enter event
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnButtonHover(button, true); });
        trigger.triggers.Add(enterEntry);
        
        // Add pointer exit event
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnButtonHover(button, false); });
        trigger.triggers.Add(exitEntry);
    }
    
    void OnButtonHover(Button button, bool isHovering)
    {
        // Scale button slightly on hover
        float targetScale = isHovering ? 1.1f : 1.0f;
        StartCoroutine(ScaleButton(button.transform, targetScale));
        
        // Change button color on hover
        ColorBlock colors = button.colors;
        colors.normalColor = isHovering ? new Color(1f, 1f, 0.8f) : Color.white;
        button.colors = colors;
    }
    
    IEnumerator ScaleButton(Transform buttonTransform, float targetScale)
    {
        float duration = 0.1f;
        float elapsed = 0;
        Vector3 startScale = buttonTransform.localScale;
        Vector3 endScale = new Vector3(targetScale, targetScale, targetScale);
        
        while (elapsed < duration)
        {
            buttonTransform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        buttonTransform.localScale = endScale;
    }
    
    public void UpdateHealthBars()
    {
        // Update Eos health bar
        float eosHealthPercent = (float)eosPlayer.currentHealth / eosPlayer.maxHealth;
        if (eosHealthText != null)
            eosHealthText.text = $"{eosPlayer.currentHealth}/{eosPlayer.maxHealth}";

        // Update Night Eos health bar
        float nightEosHealthPercent = (float)nightEosPlayer.currentHealth / nightEosPlayer.maxHealth;
        if (nightEosHealthText != null)
            nightEosHealthText.text = $"{nightEosPlayer.currentHealth}/{nightEosPlayer.maxHealth}";

        // Animate health bars (Slider range is 0-maxHealth, not 0-1)
        StartCoroutine(AnimateHealthBar(eosHealthBar, eosPlayer.currentHealth));
        StartCoroutine(AnimateHealthBar(nightEosHealthBar, nightEosPlayer.currentHealth));

        // Change health bar color based on health percentage
        UpdateHealthBarColor(eosHealthFill, eosHealthPercent, ref eosPulseCoroutine);
        UpdateHealthBarColor(nightEosHealthFill, nightEosHealthPercent, ref nightEosPulseCoroutine);
    }
    
    void UpdateHealthBarColor(Image fillImage, float healthPercent, ref Coroutine pulseCoroutine)
    {
        if (fillImage == null) return;

        if (healthPercent <= 0.2f)
        {
            if (pulseCoroutine == null)
                pulseCoroutine = StartCoroutine(PulseHealthBar(fillImage));
        }
        else
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            fillImage.color = Color.white;
        }
    }
    
    IEnumerator PulseHealthBar(Image fillImage)
    {
        while (true)
        {
            // Pulse between normal color and red
            float duration = 0.5f;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                fillImage.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(t * 2, 1));
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            yield return null;
        }
    }
    
    IEnumerator AnimateHealthBar(Slider healthBar, float targetValue)
    {
        float startValue = healthBar.value;
        float elapsedTime = 0;
        float duration = 0.5f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            healthBar.value = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }
        
        healthBar.value = targetValue;
    }
    
    void Update()
    {
        // Enable/disable buttons based on active player
        bool eosActive = GameManager.Instance.IsActivePlayer(eosPlayer);
        
        foreach (Button btn in eosActionButtons)
        {
            btn.interactable = eosActive;
        }
        
        foreach (Button btn in nightEosActionButtons)
        {
            btn.interactable = !eosActive;
        }
    }
    
    public void ShowAbilityActivation(bool isEos)
    {
        GameObject abilityIcon = isEos ? eosAbilityIcon : nightEosAbilityIcon;
        if (abilityIcon != null)
        {
            StartCoroutine(AnimateAbilityIcon(abilityIcon));
        }
    }
    
    IEnumerator AnimateAbilityIcon(GameObject abilityIcon)
    {
        abilityIcon.SetActive(true);
        
        // Scale up animation
        float duration = 0.3f;
        float elapsed = 0;
        Transform iconTransform = abilityIcon.transform;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        
        while (elapsed < duration)
        {
            iconTransform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Hold for a moment
        yield return new WaitForSeconds(1.0f);
        
        // Scale down animation
        elapsed = 0;
        while (elapsed < duration)
        {
            iconTransform.localScale = Vector3.Lerp(endScale, startScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        abilityIcon.SetActive(false);
    }
}
