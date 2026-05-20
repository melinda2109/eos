using UnityEngine;
using System.Collections;

public class ParticleEffects : MonoBehaviour
{
    [Header("Effect Prefabs")]
    public GameObject attackEffectPrefab;
    public GameObject healEffectPrefab;
    public GameObject damageEffectPrefab;
    public GameObject victoryEffectPrefab;
    public GameObject abilityEffectPrefab;
    
    [Header("Colors")]
    public Color eosColor = new Color(1f, 0.8f, 0.2f); // Golden yellow
    public Color nightEosColor = new Color(0.4f, 0.4f, 0.8f); // Blue purple
    
    public static ParticleEffects Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlayAttackEffect(Vector3 position)
    {
        if (attackEffectPrefab != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, position, Quaternion.identity);
            
            // Set color based on active player
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = GameManager.Instance.eosIsActivePlayer ? eosColor : nightEosColor;
            }
            
            Destroy(effect, 2f);
        }
    }
    
    public void PlayHealEffect(Vector3 position)
    {
        if (healEffectPrefab != null)
        {
            GameObject effect = Instantiate(healEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        else
        {
            // Fallback if prefab not assigned
            StartCoroutine(SimpleHealEffect(position));
        }
    }
    
    public void PlayDamageEffect(Vector3 position)
    {
        if (damageEffectPrefab != null)
        {
            GameObject effect = Instantiate(damageEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        else
        {
            // Fallback if prefab not assigned
            StartCoroutine(SimpleDamageEffect(position));
        }
    }
    
    public void PlayVictoryEffect(Vector3 position)
    {
        if (victoryEffectPrefab != null)
        {
            GameObject effect = Instantiate(victoryEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        else
        {
            // Fallback if prefab not assigned
            StartCoroutine(SimpleVictoryEffect(position));
        }
    }
    
    public void PlayAbilityEffect(Vector3 position, bool isEos)
    {
        if (abilityEffectPrefab != null)
        {
            GameObject effect = Instantiate(abilityEffectPrefab, position, Quaternion.identity);
            
            // Set color based on player
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = isEos ? eosColor : nightEosColor;
            }
            
            Destroy(effect, 2f);
        }
        else
        {
            // Fallback if prefab not assigned
            StartCoroutine(SimpleAbilityEffect(position, isEos));
        }
    }
    
    // Simple fallback effects using primitive objects if prefabs are not assigned
    
    IEnumerator SimpleHealEffect(Vector3 position)
    {
        // Create a simple green particle effect
        for (int i = 0; i < 10; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.localScale = Vector3.one * 0.2f;
            particle.transform.position = position + Random.insideUnitSphere * 0.5f;
            
            // Set green material
            Renderer renderer = particle.GetComponent<Renderer>();
            renderer.material.color = Color.green;
            
            // Animate and destroy
            StartCoroutine(AnimateAndDestroy(particle, 1f));
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    IEnumerator SimpleDamageEffect(Vector3 position)
    {
        // Create a simple red particle effect
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.transform.localScale = Vector3.one * 0.15f;
            particle.transform.position = position + Random.insideUnitSphere * 0.5f;
            
            // Set red material
            Renderer renderer = particle.GetComponent<Renderer>();
            renderer.material.color = Color.red;
            
            // Animate and destroy
            StartCoroutine(AnimateAndDestroy(particle, 0.7f));
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    IEnumerator SimpleVictoryEffect(Vector3 position)
    {
        // Create a simple gold/yellow particle effect
        for (int i = 0; i < 20; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.localScale = Vector3.one * 0.1f;
            particle.transform.position = position + Random.insideUnitSphere * 1f;
            
            // Set gold material
            Renderer renderer = particle.GetComponent<Renderer>();
            renderer.material.color = new Color(1f, 0.8f, 0.2f); // Gold
            
            // Animate and destroy
            StartCoroutine(AnimateAndDestroy(particle, 2f));
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    IEnumerator SimpleAbilityEffect(Vector3 position, bool isEos)
    {
        // Create a simple colored particle effect
        Color particleColor = isEos ? eosColor : nightEosColor;
        
        for (int i = 0; i < 15; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.localScale = Vector3.one * 0.15f;
            particle.transform.position = position + Random.insideUnitSphere * 0.7f;
            
            // Set color material
            Renderer renderer = particle.GetComponent<Renderer>();
            renderer.material.color = particleColor;
            
            // Animate and destroy
            StartCoroutine(AnimateAndDestroy(particle, 1.5f));
            yield return new WaitForSeconds(0.07f);
        }
    }
    
    IEnumerator AnimateAndDestroy(GameObject obj, float duration)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 endPos = startPos + Vector3.up * 1f + Random.insideUnitSphere * 0.5f;
        Vector3 startScale = obj.transform.localScale;
        Vector3 endScale = Vector3.zero;
        
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            obj.transform.position = Vector3.Lerp(startPos, endPos, t);
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(obj);
    }
}
