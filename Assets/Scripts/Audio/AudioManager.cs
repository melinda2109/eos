using System.Collections;
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
    [Tooltip("Optional. Falls back to a quieter attackSound when left empty.")]
    public AudioClip damageSound;
    public AudioClip defendSound;

    [Header("UI")]
    public AudioClip uiMoveSound;
    public AudioClip uiSelectSound;
    public AudioClip backgroundMusic;

    private float musicVolume = 0.5f;
    private float sfxVolume = 1f;
    private Coroutine duckRoutine;

    private void Awake()
    {
        EnsureSources();
        EnsureClips();
    }

    /// <summary>
    /// Fills any clip the Inspector did not supply from Resources/Audio. A Unity domain
    /// reload keeps the in-memory scene rather than re-reading it from disk, so a newly
    /// added clip field stays null until the scene is reopened; loading by name here means
    /// the audio never depends on that happening.
    /// </summary>
    private void EnsureClips()
    {
        attackSound = Fallback(attackSound, "attack_sound");
        heavyAttackSound = Fallback(heavyAttackSound, "heavy_attack_sound");
        healSound = Fallback(healSound, "heal_sound");
        victorySound = Fallback(victorySound, "victory_sound");
        abilityActivateSound = Fallback(abilityActivateSound, "ability_activate_sound");
        damageSound = Fallback(damageSound, "damage_sound");
        defendSound = Fallback(defendSound, "defend_sound");
        backgroundMusic = Fallback(backgroundMusic, "background_music");
        uiMoveSound = Fallback(uiMoveSound, "ui_move_sound");
        uiSelectSound = Fallback(uiSelectSound, "ui_select_sound");
    }

    private static AudioClip Fallback(AudioClip assigned, string resourceName)
    {
        if (assigned != null) return assigned;
        return Resources.Load<AudioClip>($"Audio/{resourceName}");
    }

    /// <summary>
    /// Creates the two AudioSources when the scene did not supply them. Without this the
    /// whole manager degrades to silence the moment someone forgets to wire the Inspector.
    /// </summary>
    private void EnsureSources()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f; // 2D: audible regardless of listener distance
        }
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
        }
    }

    private void Start()
    {
        EnsureSources();
        EnsureClips();
        musicVolume = SaveSystem.MusicVolume;
        sfxVolume = SaveSystem.SfxVolume;

        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        SaveSystem.MusicVolume = musicVolume;
        // A duck in progress owns the source volume; it restores to the new level itself.
        if (duckRoutine == null && musicSource != null) musicSource.volume = musicVolume;
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        SaveSystem.SfxVolume = sfxVolume;
    }

    private void Play(AudioClip clip, float volumeScale)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume);
    }

    public void PlayAttackSound() => Play(attackSound, 0.7f);

    public void PlayHeavyAttackSound() => Play(heavyAttackSound, 0.8f);

    public void PlayHealSound() => Play(healSound, 0.6f);

    public void PlayAbilityActivateSound() => Play(abilityActivateSound, 0.7f);

    public void PlayDefendSound() => Play(defendSound, 0.65f);

    public void PlayUIMoveSound() => Play(uiMoveSound, 0.5f);

    public void PlayUISelectSound() => Play(uiSelectSound, 0.7f);

    public void PlayDamageSound()
    {
        // No dedicated impact clip yet: reuse the attack swing, quieter, so hits are never silent.
        if (damageSound != null) Play(damageSound, 0.6f);
        else Play(attackSound, 0.35f);
    }

    /// <summary>
    /// Plays the victory sting over the music instead of killing the track, so the
    /// soundtrack is still running when the next night starts.
    /// </summary>
    public void PlayVictorySound()
    {
        Play(victorySound, 1.0f);
        float duration = victorySound != null ? victorySound.length : 1.5f;
        if (musicSource == null) return;
        if (duckRoutine != null) StopCoroutine(duckRoutine);
        duckRoutine = StartCoroutine(DuckMusic(duration));
    }

    private IEnumerator DuckMusic(float holdSeconds)
    {
        const float fade = 0.25f;
        float ducked = musicVolume * 0.25f;

        for (float t = 0f; t < fade; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(musicVolume, ducked, t / fade);
            yield return null;
        }
        musicSource.volume = ducked;

        yield return new WaitForSeconds(Mathf.Max(0f, holdSeconds));

        for (float t = 0f; t < fade; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(ducked, musicVolume, t / fade);
            yield return null;
        }
        musicSource.volume = musicVolume;
        duckRoutine = null;
    }
}
