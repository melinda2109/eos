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
        StartCoroutine(FlashEffect(Color.red));
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        // Trigger heal animation
        if (playerAnimator != null)
            playerAnimator.SetTrigger("Heal");
        
        // Flash green effect
        StartCoroutine(FlashEffect(Color.green));
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }
    
    public void Attack()
    {
        if (!GameManager.Instance.IsActivePlayer(this)) return;
        
        Player target = GetOpponent();
        
        if (playerAnimator != null)
            playerAnimator.SetTrigger("Attack");
        
        GameManager.Instance.PlayerAttack(this, target);
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
        
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
}
