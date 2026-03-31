using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    //private UIManager.UIState prevState;

    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        //prevState = UIManager.Instance.State;
        //UIManager.Instance.SetUIState(UIManager.UIState.SETTINGS);
    }

    void OnDisable()
    {
        //UIManager.Instance.SetUIState(prevState);
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