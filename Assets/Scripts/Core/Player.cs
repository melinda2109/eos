using UnityEngine;
using System.Collections;

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

    void Awake()
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
    
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Trigger damage animation
        if (playerAnimator != null)
            playerAnimator.SetTrigger("TakeDamage");
        
        // Flash red effect
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashEffect(Color.red));
        SwapSprite(damageSprite);
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        // Trigger heal animation
        if (playerAnimator != null)
            playerAnimator.SetTrigger("Heal");
        
        // Flash green effect
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashEffect(Color.green));
        SwapSprite(healSprite);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }
    
    public void Attack()
    {
        if (!GameManager.Instance.IsActivePlayer(this)) return;
        
        Player target = GetOpponent();
        
        if (playerAnimator != null)
            playerAnimator.SetTrigger("Attack");

        SwapSprite(attackSprite);

        GameManager.Instance.PlayerAttack(this, target);
    }

    void SwapSprite(Sprite tempSprite)
    {
        if (spriteRenderer == null || tempSprite == null || idleSprite == null) return;
        if (spriteCoroutine != null)
            StopCoroutine(spriteCoroutine);
        spriteCoroutine = StartCoroutine(ShowSpriteTemporarily(tempSprite));
    }

    IEnumerator ShowSpriteTemporarily(Sprite tempSprite)
    {
        spriteRenderer.sprite = tempSprite;
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.sprite = idleSprite;
    }

    Player GetOpponent()
    {
        return this == GameManager.Instance.eosPlayer ? 
               GameManager.Instance.nightEosPlayer : 
               GameManager.Instance.eosPlayer;
    }
    
    IEnumerator FlashEffect(Color flashColor)
    {
        if (spriteRenderer == null) yield break;

        // Blend only partially so the sprite's own colors/details stay visible
        // (a full color replacement wipes out warm sprites to solid red / dark sprites to near-black).
        // Always tint from/restore to baseSpriteColor (not spriteRenderer.color) so an overlapping
        // damage+heal flash can't leave the sprite stuck on a half-applied tint.
        Color tintedColor = Color.Lerp(baseSpriteColor, flashColor, 0.35f);
        spriteRenderer.color = tintedColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = baseSpriteColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = tintedColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = baseSpriteColor;
    }
}
