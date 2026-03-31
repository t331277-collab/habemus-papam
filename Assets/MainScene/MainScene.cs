using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    [SerializeField] private GameObject startGameWarningPopup;
    [SerializeField] private GameObject loadWarningPopup;
    [SerializeField] private GameObject loadPopup;
    [SerializeField] private Component loadPlayerHpText;
    [SerializeField] private Component loadPlayerInfluenceText;
    [SerializeField] private Component loadPlayerPietyText;
    [SerializeField] private Component loadDayText;
    [SerializeField] private Component loadConclaveText;

    private void Awake()
    {
        SetStartGameWarningPopup(false);
        SetLoadWarningPopup(false);
        SetLoadPopup(false);
    }

    public void OnClickStartGame()
    {
        if (SaveManager.Instance == null)
        {
            return;
        }

        if (SaveManager.Instance.HasSave())
        {
            SetStartGameWarningPopup(true);
            return;
        }

        SaveManager.Instance.StartNewGame();
    }

    public void OnClickConfirmStartGame()
    {
        SetStartGameWarningPopup(false);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.StartNewGame();
        }
    }

    public void OnClickCancelStartGame()
    {
        SetStartGameWarningPopup(false);
    }

    public void OnClickLoad()
    {
        if (SaveManager.Instance == null)
        {
            return;
        }

        SetLoadWarningPopup(false);
        SetLoadPopup(false);

        if (!SaveManager.Instance.HasSave())
        {
            SetLoadWarningPopup(true);
            return;
        }

        if (SaveManager.Instance.TryGetSavePreview(out SavePreviewData preview))
        {
            ApplyLoadPreview(preview);
            SetLoadPopup(true);
            return;
        }

        SetLoadWarningPopup(true);
    }

    public void OnClickPlayLoadGame()
    {
        SetLoadPopup(false);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
    }

    public void OnClickCloseLoadPopup()
    {
        SetLoadPopup(false);
    }

    public void OnClickCloseLoadWarningPopup()
    {
        SetLoadWarningPopup(false);
    }

    private void SetStartGameWarningPopup(bool isActive)
    {
        if (startGameWarningPopup != null)
        {
            startGameWarningPopup.SetActive(isActive);
        }
    }

    private void SetLoadWarningPopup(bool isActive)
    {
        if (loadWarningPopup != null)
        {
            loadWarningPopup.SetActive(isActive);
        }
    }

    private void SetLoadPopup(bool isActive)
    {
        if (loadPopup != null)
        {
            loadPopup.SetActive(isActive);
        }
    }

    private void ApplyLoadPreview(SavePreviewData preview)
    {
        if (preview == null)
        {
            return;
        }

        SetText(loadPlayerHpText, FormatStatValue(preview.playerHp));
        SetText(loadPlayerInfluenceText, FormatStatValue(preview.playerInfluence));
        SetText(loadPlayerPietyText, FormatStatValue(preview.playerPiety));
        SetText(loadDayText, preview.day.ToString());
        SetText(loadConclaveText, preview.conclaveName);
    }

    private string FormatStatValue(float value)
    {
        return value.ToString("0.#", CultureInfo.InvariantCulture);
    }

    private void SetText(Component textComponent, string value)
    {
        Component resolvedComponent = ResolveTextComponent(textComponent);
        if (resolvedComponent == null)
        {
            return;
        }

        if (resolvedComponent is TMP_Text tmpText)
        {
            tmpText.text = value;
            return;
        }

        if (resolvedComponent is Text legacyText)
        {
            legacyText.text = value;
        }
    }

    private Component ResolveTextComponent(Component sourceComponent)
    {
        if (sourceComponent == null)
        {
            return null;
        }

        if (sourceComponent is TMP_Text || sourceComponent is Text)
        {
            return sourceComponent;
        }

        TMP_Text tmpText = sourceComponent.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            return tmpText;
        }

        Text legacyText = sourceComponent.GetComponent<Text>();
        if (legacyText != null)
        {
            return legacyText;
        }

        return sourceComponent.GetComponentInChildren<TMP_Text>(true) as Component
            ?? sourceComponent.GetComponentInChildren<Text>(true);
    }
}
