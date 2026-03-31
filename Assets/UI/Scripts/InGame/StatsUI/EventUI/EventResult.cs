using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

class EventResult : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] TextMeshProUGUI result;
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
            if(evt.OnChoiceOption1(UIManager.Instance.Ingame.Stats.LinkedCardinals[0]) == true)
            {
                text.text = evt.option1SuccessDescription;
                this.result.text = evt.option1SuccessResult;
            }
            else
            {
                text.text = evt.option1FailDescription;
                this.result.text = evt.option1FailResult;
            }
        }
        else if(result == 2)
        {
            if(evt.OnChoiceOption2(UIManager.Instance.Ingame.Stats.LinkedCardinals[0]) == true)
            {
                text.text = evt.option2SuccessDescription;
                this.result.text = evt.option2SuccessResult;
            }
            else
            {
                text.text = evt.option2FailDescription;
                this.result.text = evt.option2FailResult;
            }
        }
    }
}