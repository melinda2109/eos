using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    
    [Header("Sound Effects")]
    public AudioClip attackSound;
    public AudioClip heavyAttackSound;
    public AudioClip healSound;
    public AudioClip victorySound;
    public AudioClip abilityActivateSound;
    public AudioClip damageSound;
    public AudioClip backgroundMusic;
    
    void Start()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = 0.5f;
            musicSource.Play();
        }
    }
    
    public void PlayAttackSound()
    {
        if (attackSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(attackSound, 0.7f);
        }
    }
    
    public void PlayHeavyAttackSound()
    {
        if (heavyAttackSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(heavyAttackSound, 0.8f);
        }
    }
    
    public void PlayHealSound()
    {
        if (healSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(healSound, 0.6f);
        }
    }
    
    public void PlayVictorySound()
    {
        if (victorySound != null && sfxSource != null)
        {
            // Stop background music
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
            
            sfxSource.PlayOneShot(victorySound, 1.0f);
        }
    }
    
    public void PlayAbilityActivateSound()
    {
        if (abilityActivateSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(abilityActivateSound, 0.7f);
        }
    }
    
    public void PlayDamageSound()
    {
        if (damageSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(damageSound, 0.6f);
        }
    }
}
