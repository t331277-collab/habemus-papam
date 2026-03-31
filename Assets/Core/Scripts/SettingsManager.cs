using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private int masterVolume = 100;
    private bool isMasterMuted = false;
    private int bgmVolume = 100;
    private bool isBgmMuted = false;
    private int sfxVolume = 100;
    private bool isSfxMuted = false;

    public int MasterVolume => masterVolume;
    public bool IsMasterMuted => isMasterMuted;
    public int BgmVolume => bgmVolume;
    public bool IsBgmMuted => isBgmMuted;
    public int SfxVolume => sfxVolume;
    public bool IsSfxMuted => isSfxMuted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp(Mathf.RoundToInt(value), 0, 100);
        ApplySettings();
    }

    public void SetMasterMute(bool isMuted)
    {
        isMasterMuted = isMuted;
        ApplySettings();
    }

    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp(Mathf.RoundToInt(value), 0, 100);
        ApplySettings();
    }

    public void SetBgmMute(bool isMuted)
    {
        isBgmMuted = isMuted;
        ApplySettings();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp(Mathf.RoundToInt(value), 0, 100);
        ApplySettings();
    }

    public void SetSfxMute(bool isMuted)
    {
        isSfxMuted = isMuted;
        ApplySettings();
    }

    private void ApplySettings()
    {
        float finalMasterVolume = isMasterMuted ? 0f : masterVolume / 100f;
        AudioListener.volume = finalMasterVolume;

        // BGM / SFX 분리는 나중에 AudioMixer나 AudioSource 분리 후 적용
    }
}