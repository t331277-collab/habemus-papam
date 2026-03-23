using UnityEngine;
using UnityEngine.UI;
using TMPro;

class EventResult : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI ButtonText;
    private Event currentEvent;
    
    public void ShowEvent(Event evt, int result)
    {
        ButtonText.text = "확인";
        currentEvent = evt;
        title.text = evt.eventName;

        if(result == 1)
        {
            text.text = evt.OnChoiceOption1(UIManager.Instance.Ingame.Stats.LinkedCardinals[0])?
            evt.option1SuccessDescription : evt.option1FailDescription;
        }
        else if(result == 2)
        {
            text.text = evt.OnChoiceOption2(UIManager.Instance.Ingame.Stats.LinkedCardinals[0])?
            evt.option2SuccessDescription : evt.option2FailDescription;
        }
    }
}