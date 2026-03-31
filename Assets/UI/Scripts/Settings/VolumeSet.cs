using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class VolumeSet : MonoBehaviour
{
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
}
