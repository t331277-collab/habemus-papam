using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    private enum HotKeyTarget
    {
        None,
        Up,
        Down,
        Right,
        Left,
        Pray,
        Speech
    }

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private ScrollRect settingsScrollRect;

    [SerializeField] private VolumeSet masterVolume;
    [SerializeField] private VolumeSet bgmVolume;
    [SerializeField] private VolumeSet sfxVolume;

    [SerializeField] private Button upKey;
    [SerializeField] private Button downKey;
    [SerializeField] private Button rightKey;
    [SerializeField] private Button leftKey;
    [SerializeField] private Button prayHotKey;
    [SerializeField] private Button speechHotKey;

    private HotKeyTarget waitingHotKeyTarget = HotKeyTarget.None;

    //private UIManager.UIState prevState;

    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        RegisterEvents();
        SyncHotKeyButtonsFromManager();
    }

    void OnEnable()
    {
        RefreshUI();

        //prevState = UIManager.Instance.State;
        //UIManager.Instance.SetUIState(UIManager.UIState.SETTINGS);
    }

    void OnDisable()
    {
        //UIManager.Instance.SetUIState(prevState);
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }

    private void Update()
    {
        if (waitingHotKeyTarget != HotKeyTarget.None)
        {
            CaptureHotKeyInput();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsPanel();
        }

        /*
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isCredit) CloseCredits();
            else if (isHelp) CloseHelp();
        }
        */
    }

    // =========================================================
    // 설정창 열기 / 닫기
    // =========================================================

    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel이 연결되지 않았습니다.");
            return;
        }

        settingsPanel.SetActive(!settingsPanel.activeSelf);

        if (settingsPanel.activeSelf)
        {
            ResetScrollToTop();
        }
    }

    public void OpenSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel이 연결되지 않았습니다.");
            return;
        }

        settingsPanel.SetActive(true);
        ResetScrollToTop();
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel이 연결되지 않았습니다.");
            return;
        }

        settingsPanel.SetActive(false);
    }

    private void RegisterEvents()
    {
        CacheSettingsScrollRect();

        if (masterVolume != null)
        {
            masterVolume.VolumeSlider.onValueChanged.AddListener(OnChangeMasterVolume);
            masterVolume.MuteToggle.onValueChanged.AddListener(OnChangeMasterMute);
        }

        if (bgmVolume != null)
        {
            bgmVolume.VolumeSlider.onValueChanged.AddListener(OnChangeBgmVolume);
            bgmVolume.MuteToggle.onValueChanged.AddListener(OnChangeBgmMute);
        }

        if (sfxVolume != null)
        {
            sfxVolume.VolumeSlider.onValueChanged.AddListener(OnChangeSfxVolume);
            sfxVolume.MuteToggle.onValueChanged.AddListener(OnChangeSfxMute);
        }

        if (upKey != null)
        {
            upKey.onClick.AddListener(OnClickUpKey);
        }

        if (downKey != null)
        {
            downKey.onClick.AddListener(OnClickDownKey);
        }

        if (rightKey != null)
        {
            rightKey.onClick.AddListener(OnClickRightKey);
        }

        if (leftKey != null)
        {
            leftKey.onClick.AddListener(OnClickLeftKey);
        }

        if (prayHotKey != null)
        {
            prayHotKey.onClick.AddListener(OnClickPrayHotKey);
        }

        if (speechHotKey != null)
        {
            speechHotKey.onClick.AddListener(OnClickSpeechHotKey);
        }
    }

    private void UnregisterEvents()
    {
        if (masterVolume != null)
        {
            masterVolume.VolumeSlider.onValueChanged.RemoveListener(OnChangeMasterVolume);
            masterVolume.MuteToggle.onValueChanged.RemoveListener(OnChangeMasterMute);
        }

        if (bgmVolume != null)
        {
            bgmVolume.VolumeSlider.onValueChanged.RemoveListener(OnChangeBgmVolume);
            bgmVolume.MuteToggle.onValueChanged.RemoveListener(OnChangeBgmMute);
        }

        if (sfxVolume != null)
        {
            sfxVolume.VolumeSlider.onValueChanged.RemoveListener(OnChangeSfxVolume);
            sfxVolume.MuteToggle.onValueChanged.RemoveListener(OnChangeSfxMute);
        }

        if (upKey != null)
        {
            upKey.onClick.RemoveListener(OnClickUpKey);
        }

        if (downKey != null)
        {
            downKey.onClick.RemoveListener(OnClickDownKey);
        }

        if (rightKey != null)
        {
            rightKey.onClick.RemoveListener(OnClickRightKey);
        }

        if (leftKey != null)
        {
            leftKey.onClick.RemoveListener(OnClickLeftKey);
        }

        if (prayHotKey != null)
        {
            prayHotKey.onClick.RemoveListener(OnClickPrayHotKey);
        }

        if (speechHotKey != null)
        {
            speechHotKey.onClick.RemoveListener(OnClickSpeechHotKey);
        }
    }

    public void RefreshUI()
    {
        SettingsManager sm = SettingsManager.Instance;
        if (sm == null) return;

        masterVolume.SetValue(sm.MasterVolume);
        masterVolume.SetMute(sm.IsMasterMuted);

        bgmVolume.SetValue(sm.BgmVolume);
        bgmVolume.SetMute(sm.IsBgmMuted);

        sfxVolume.SetValue(sm.SfxVolume);
        sfxVolume.SetMute(sm.IsSfxMuted);

        RefreshDependentVolumeVisuals();
        SyncHotKeyButtonsFromManager();
    }

    // =========================================================
    // 스크롤 초기화
    // =========================================================

    private void CacheSettingsScrollRect()
    {
        if (settingsScrollRect == null && settingsPanel != null)
        {
            settingsScrollRect = settingsPanel.GetComponentInChildren<ScrollRect>(true);
        }
    }

    private void ResetScrollToTop()
    {
        if (settingsScrollRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        settingsScrollRect.StopMovement();
        settingsScrollRect.verticalNormalizedPosition = 1f;
        settingsScrollRect.horizontalNormalizedPosition = 0f;
    }

    // =========================================================
    // 볼륨 설정
    // =========================================================

    private void OnChangeMasterVolume(float value)
    {
        masterVolume.RefreshText();
        SettingsManager.Instance.SetMasterVolume(value);
    }

    private void OnChangeMasterMute(bool isMuted)
    {
        masterVolume.ApplyMutedVisual(isMuted);
        RefreshDependentVolumeVisuals();
        SettingsManager.Instance.SetMasterMute(isMuted);
    }

    private void OnChangeBgmVolume(float value)
    {
        bgmVolume.RefreshText();
        SettingsManager.Instance.SetBgmVolume(value);
    }

    private void OnChangeBgmMute(bool isMuted)
    {
        RefreshDependentVolumeVisuals();
        SettingsManager.Instance.SetBgmMute(isMuted);
    }

    private void OnChangeSfxVolume(float value)
    {
        sfxVolume.RefreshText();
        SettingsManager.Instance.SetSfxVolume(value);
    }

    private void OnChangeSfxMute(bool isMuted)
    {
        RefreshDependentVolumeVisuals();
        SettingsManager.Instance.SetSfxMute(isMuted);
    }

    private void RefreshDependentVolumeVisuals()
    {
        bool isMasterMuted = masterVolume != null && masterVolume.IsMuted();

        if (bgmVolume != null)
        {
            bgmVolume.ApplyMutedVisual(isMasterMuted || bgmVolume.IsMuted());
        }

        if (sfxVolume != null)
        {
            sfxVolume.ApplyMutedVisual(isMasterMuted || sfxVolume.IsMuted());
        }
    }

    // =========================================================
    // 단축키 설정
    // =========================================================

    private void OnClickUpKey()
    {
        waitingHotKeyTarget = HotKeyTarget.Up;
        SetWaitingText(upKey);
    }

    private void OnClickDownKey()
    {
        waitingHotKeyTarget = HotKeyTarget.Down;
        SetWaitingText(downKey);
    }

    private void OnClickRightKey()
    {
        waitingHotKeyTarget = HotKeyTarget.Right;
        SetWaitingText(rightKey);
    }

    private void OnClickLeftKey()
    {
        waitingHotKeyTarget = HotKeyTarget.Left;
        SetWaitingText(leftKey);
    }

    private void OnClickPrayHotKey()
    {
        waitingHotKeyTarget = HotKeyTarget.Pray;
        SetWaitingText(prayHotKey);
    }

    private void OnClickSpeechHotKey()
    {
        waitingHotKeyTarget = HotKeyTarget.Speech;
        SetWaitingText(speechHotKey);
    }

    private void SetWaitingText(Button targetButton)
    {
        TMP_Text targetText = GetButtonText(targetButton);

        if (targetText != null)
        {
            targetText.text = "...";
        }
    }

    private void CaptureHotKeyInput()
    {
        if (!Input.anyKeyDown)
        {
            return;
        }

        if (TryGetPressedKeyCode(out KeyCode pressedKey))
        {
            ApplyHotKeyText(pressedKey);
        }
    }

    private bool TryGetPressedKeyCode(out KeyCode pressedKey)
    {
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                pressedKey = keyCode;
                return true;
            }
        }

        pressedKey = KeyCode.None;
        return false;
    }

    private void ApplyHotKeyText(KeyCode pressedKey)
    {
        if (IsAlphabetKey(pressedKey))
        {
            string newLabel = FormatHotKeyLabel(pressedKey);
            UpdateManagerHotKey(waitingHotKeyTarget, newLabel);
        }

        SyncHotKeyButtonsFromManager();
        waitingHotKeyTarget = HotKeyTarget.None;
    }

    private bool IsAlphabetKey(KeyCode keyCode)
    {
        return keyCode >= KeyCode.A && keyCode <= KeyCode.Z;
    }

    private string FormatHotKeyLabel(KeyCode keyCode)
    {
        string keyName = keyCode.ToString();

        if (keyCode >= KeyCode.A && keyCode <= KeyCode.Z)
        {
            return keyName.ToUpper();
        }

        return keyName;
    }

    private void SyncHotKeyButtonsFromManager()
    {
        SetButtonText(upKey, GetManagerHotKey(HotKeyTarget.Up));
        SetButtonText(downKey, GetManagerHotKey(HotKeyTarget.Down));
        SetButtonText(rightKey, GetManagerHotKey(HotKeyTarget.Right));
        SetButtonText(leftKey, GetManagerHotKey(HotKeyTarget.Left));
        SetButtonText(prayHotKey, GetManagerHotKey(HotKeyTarget.Pray));
        SetButtonText(speechHotKey, GetManagerHotKey(HotKeyTarget.Speech));
    }

    private void UpdateManagerHotKey(HotKeyTarget target, string label)
    {
        if (target == HotKeyTarget.None)
        {
            return;
        }

        ClearDuplicateManagerHotKey(target, label);
        SetManagerHotKey(target, label);
    }

    private void ClearDuplicateManagerHotKey(HotKeyTarget currentTarget, string label)
    {
        for (HotKeyTarget target = HotKeyTarget.Up; target <= HotKeyTarget.Speech; target++)
        {
            if (target == currentTarget)
            {
                continue;
            }

            if (GetManagerHotKey(target) == label)
            {
                SetManagerHotKey(target, string.Empty);
            }
        }
    }

    private string GetManagerHotKey(HotKeyTarget target)
    {
        SettingsManager sm = SettingsManager.Instance;

        if (sm == null)
        {
            return string.Empty;
        }

        switch (target)
        {
            case HotKeyTarget.Up:
                return sm.UpKeyLabel;
            case HotKeyTarget.Down:
                return sm.DownKeyLabel;
            case HotKeyTarget.Right:
                return sm.RightKeyLabel;
            case HotKeyTarget.Left:
                return sm.LeftKeyLabel;
            case HotKeyTarget.Pray:
                return sm.PrayKeyLabel;
            case HotKeyTarget.Speech:
                return sm.SpeechKeyLabel;
            default:
                return string.Empty;
        }
    }

    private void SetManagerHotKey(HotKeyTarget target, string label)
    {
        SettingsManager sm = SettingsManager.Instance;

        if (sm == null)
        {
            return;
        }

        switch (target)
        {
            case HotKeyTarget.Up:
                sm.SetUpKeyLabel(label);
                break;
            case HotKeyTarget.Down:
                sm.SetDownKeyLabel(label);
                break;
            case HotKeyTarget.Right:
                sm.SetRightKeyLabel(label);
                break;
            case HotKeyTarget.Left:
                sm.SetLeftKeyLabel(label);
                break;
            case HotKeyTarget.Pray:
                sm.SetPrayKeyLabel(label);
                break;
            case HotKeyTarget.Speech:
                sm.SetSpeechKeyLabel(label);
                break;
        }
    }

    private static TMP_Text GetButtonText(Button button)
    {
        if (button == null)
        {
            return null;
        }

        return button.GetComponentInChildren<TMP_Text>(true);
    }

    private static void SetButtonText(Button button, string label)
    {
        TMP_Text targetText = GetButtonText(button);

        if (targetText != null)
        {
            targetText.text = label;
        }
    }

    bool isCredit = false;
    bool isHelp = false;
    void OpenCredits()
    {
        isCredit = true;
    }
    void CloseCredits()
    {
        isCredit = false;
    }
    void OpenHelp()
    {
        isHelp = true;
    }
    void CloseHelp()
    {
        isHelp = false;
    }

    // 어떤 방식으로 credits와 help를 열지는 나중에 결정
}
