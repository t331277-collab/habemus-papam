using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class VolumeSet : MonoBehaviour
{
    private const float DisabledAlpha = 0.8f;
    private const float EnabledAlpha = 1f;

    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle muteToggle;

    public Slider VolumeSlider => volumeSlider;
    public TMP_Text ValueText => valueText;
    public Toggle MuteToggle => muteToggle;

    private void Reset()
    {
        valueText = transform.Find("Value")?.GetComponent<TMP_Text>();
        volumeSlider = transform.Find("Slider")?.GetComponent<Slider>();
        muteToggle = transform.Find("Mute")?.GetComponent<Toggle>();
    }

    public void SetValue(float value)
    {
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(value);
        }

        if (valueText != null)
        {
            valueText.text = Mathf.RoundToInt(value).ToString();
        }
    }

    public void SetMute(bool isMuted)
    {
        if (muteToggle != null)
        {
            muteToggle.SetIsOnWithoutNotify(isMuted);
        }

        ApplyMutedVisual(isMuted);
    }

    public float GetValue()
    {
        return volumeSlider != null ? volumeSlider.value : 0f;
    }

    public bool IsMuted()
    {
        return muteToggle != null && muteToggle.isOn;
    }

    public void RefreshText()
    {
        if (volumeSlider != null && valueText != null)
        {
            valueText.text = Mathf.RoundToInt(volumeSlider.value).ToString();
        }
    }

    public void ApplyMutedVisual(bool isMuted)
    {
        if (volumeSlider != null)
        {
            volumeSlider.interactable = !isMuted;
            SetCanvasGroupState(volumeSlider.gameObject, isMuted);
        }

        if (valueText != null)
        {
            SetCanvasGroupState(valueText.gameObject, isMuted);
        }
    }

    private void SetCanvasGroupState(GameObject target, bool isMuted)
    {
        CanvasGroup canvasGroup = GetOrAddCanvasGroup(target);
        canvasGroup.alpha = isMuted ? DisabledAlpha : EnabledAlpha;
        canvasGroup.interactable = !isMuted;
        canvasGroup.blocksRaycasts = !isMuted;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }
}
