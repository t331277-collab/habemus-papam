using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private VolumeSet masterVolume;
    [SerializeField] private VolumeSet bgmVolume;
    [SerializeField] private VolumeSet sfxVolume;

    //private UIManager.UIState prevState;

    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        RegisterEvents();
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

    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel이 연결되지 않았습니다.");
            return;
        }

        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void OpenSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel이 연결되지 않았습니다.");
            return;
        }

        settingsPanel.SetActive(true);
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
    }

    private void OnChangeMasterVolume(float value)
    {
        masterVolume.RefreshText();
        SettingsManager.Instance.SetMasterVolume(value);
    }

    private void OnChangeMasterMute(bool isMuted)
    {
        SettingsManager.Instance.SetMasterMute(isMuted);
    }

    private void OnChangeBgmVolume(float value)
    {
        bgmVolume.RefreshText();
        SettingsManager.Instance.SetBgmVolume(value);
    }

    private void OnChangeBgmMute(bool isMuted)
    {
        SettingsManager.Instance.SetBgmMute(isMuted);
    }

    private void OnChangeSfxVolume(float value)
    {
        sfxVolume.RefreshText();
        SettingsManager.Instance.SetSfxVolume(value);
    }

    private void OnChangeSfxMute(bool isMuted)
    {
        SettingsManager.Instance.SetSfxMute(isMuted);
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