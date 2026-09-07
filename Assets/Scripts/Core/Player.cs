using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    public string playerName;
    public int maxHealth = 100;
    public int currentHealth;
    public int attackPower;
    public int defensePower;

    [Header("Abilities")]
    public int lightAttackDamage = 15;
    public int heavyAttackDamage = 25;
    public int healAmount = 20;

    [Header("Visual")]
    public Animator playerAnimator;
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite attackSprite;
    public Sprite damageSprite;
    public Sprite healSprite;

    private Coroutine spriteCoroutine;
    private Coroutine flashCoroutine;
    private Color baseSpriteColor = Color.white;

    private void Awake()
    {
        if (spriteRenderer != null)
            baseSpriteColor = spriteRenderer.color;
    }

    public void Initialize(string name, int health, int attack, int defense)
    {
        playerName = name;
        maxHealth = health;
        currentHealth = health;
        attackPower = attack;
        defensePower = defense;
    }

    public void PlayAttack()
    {
        TriggerAnimation("Attack", "eos_attack");
        SwapSprite(attackSprite);
    }

    public void PlayHeal()
    {
        TriggerAnimation("Heal", "eos_heal");
        SwapSprite(healSprite);
        Flash(Color.green);
    }

    public void PlayDefend()
    {
        TriggerAnimation("Defend", "eos_defend");
        Flash(new Color(0.25f, 0.8f, 1f));
    }

    public void TakeDamage(int damage)
    {
        TriggerAnimation("TakeDamage", "eos_damage");
        SwapSprite(damageSprite);
        Flash(Color.red);
        currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(0, damage));
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
        PlayHeal();
    }

    public void HealSilent(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
    }

    public void Attack()
    {
        if (GameManager.Instance != null && GameManager.Instance.Battle != null)
            GameManager.Instance.Battle.OnAttackButton();
    }

    private void TriggerAnimation(string primary, string fallback)
    {
        if (playerAnimator == null) return;
        foreach (var parameter in playerAnimator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == primary)
            {
                playerAnimator.SetTrigger(primary);
                return;
            }
        }
        foreach (var parameter in playerAnimator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == fallback)
            {
                playerAnimator.SetTrigger(fallback);
                return;
            }
        }
    }

    private void SwapSprite(Sprite tempSprite)
    {
        if (spriteRenderer == null || tempSprite == null || idleSprite == null) return;
        if (spriteCoroutine != null) StopCoroutine(spriteCoroutine);
        spriteCoroutine = StartCoroutine(ShowSpriteTemporarily(tempSprite));
    }

    private IEnumerator ShowSpriteTemporarily(Sprite tempSprite)
    {
        spriteRenderer.sprite = tempSprite;
        yield return new WaitForSeconds(0.45f);
        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }

    private void Flash(Color flashColor)
    {
        if (spriteRenderer == null) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashEffect(flashColor));
    }

    private IEnumerator FlashEffect(Color flashColor)
    {
        Color tinted = Color.Lerp(baseSpriteColor, flashColor, 0.35f);
        spriteRenderer.color = tinted;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = baseSpriteColor;
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.color = tinted;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = baseSpriteColor;
    }
}
