using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private const string DefaultUpKey = "W";
    private const string DefaultDownKey = "S";
    private const string DefaultRightKey = "D";
    private const string DefaultLeftKey = "A";
    private const string DefaultPrayKey = "F";
    private const string DefaultSpeechKey = "G";

    private int masterVolume = 100;
    private bool isMasterMuted = false;
    private int bgmVolume = 100;
    private bool isBgmMuted = false;
    private int sfxVolume = 100;
    private bool isSfxMuted = false;
    private string upKeyLabel = DefaultUpKey;
    private string downKeyLabel = DefaultDownKey;
    private string rightKeyLabel = DefaultRightKey;
    private string leftKeyLabel = DefaultLeftKey;
    private string prayKeyLabel = DefaultPrayKey;
    private string speechKeyLabel = DefaultSpeechKey;

    public int MasterVolume => masterVolume;
    public bool IsMasterMuted => isMasterMuted;
    public int BgmVolume => bgmVolume;
    public bool IsBgmMuted => isBgmMuted;
    public int SfxVolume => sfxVolume;
    public bool IsSfxMuted => isSfxMuted;
    public string UpKeyLabel => upKeyLabel;
    public string DownKeyLabel => downKeyLabel;
    public string RightKeyLabel => rightKeyLabel;
    public string LeftKeyLabel => leftKeyLabel;
    public string PrayKeyLabel => prayKeyLabel;
    public string SpeechKeyLabel => speechKeyLabel;

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

    public void SetUpKeyLabel(string label)
    {
        upKeyLabel = NormalizeHotKeyLabel(label);
    }

    public void SetDownKeyLabel(string label)
    {
        downKeyLabel = NormalizeHotKeyLabel(label);
    }

    public void SetRightKeyLabel(string label)
    {
        rightKeyLabel = NormalizeHotKeyLabel(label);
    }

    public void SetLeftKeyLabel(string label)
    {
        leftKeyLabel = NormalizeHotKeyLabel(label);
    }

    public void SetPrayKeyLabel(string label)
    {
        prayKeyLabel = NormalizeHotKeyLabel(label);
    }

    public void SetSpeechKeyLabel(string label)
    {
        speechKeyLabel = NormalizeHotKeyLabel(label);
    }

    private void ApplySettings()
    {
        float finalMasterVolume = isMasterMuted ? 0f : masterVolume / 100f;
        AudioListener.volume = finalMasterVolume;

        // BGM / SFX 분리는 나중에 AudioMixer나 AudioSource 분리 후 적용
    }

    private static string NormalizeHotKeyLabel(string label)
    {
        return string.IsNullOrWhiteSpace(label) ? string.Empty : label.ToUpper();
    }
}
