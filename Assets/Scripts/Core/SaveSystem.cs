using UnityEngine;

/// <summary>
/// Thin wrapper over PlayerPrefs. Everything the game needs to remember between
/// sessions lives here so no other script has to know about key names.
/// </summary>
public static class SaveSystem
{
    private const string BestNightKey = "eos.bestNight";
    private const string RunsPlayedKey = "eos.runsPlayed";
    private const string MusicVolumeKey = "eos.musicVolume";
    private const string SfxVolumeKey = "eos.sfxVolume";

    public static int BestNight
    {
        get => PlayerPrefs.GetInt(BestNightKey, 0);
        private set { PlayerPrefs.SetInt(BestNightKey, value); PlayerPrefs.Save(); }
    }

    public static int RunsPlayed
    {
        get => PlayerPrefs.GetInt(RunsPlayedKey, 0);
        private set { PlayerPrefs.SetInt(RunsPlayedKey, value); PlayerPrefs.Save(); }
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f);
        set { PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        set { PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
    }

    /// <summary>Records the depth a finished run reached. Returns true if it beat the record.</summary>
    public static bool RecordRun(int nightReached)
    {
        RunsPlayed = RunsPlayed + 1;
        if (nightReached <= BestNight) return false;
        BestNight = nightReached;
        return true;
    }

    public static void ClearProgress()
    {
        PlayerPrefs.DeleteKey(BestNightKey);
        PlayerPrefs.DeleteKey(RunsPlayedKey);
        PlayerPrefs.Save();
    }
}
