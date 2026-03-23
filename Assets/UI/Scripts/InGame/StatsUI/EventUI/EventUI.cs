using TMPro;
using UnityEngine;
using UnityEngine.UI;
//이벤트 버튼 창 열기/닫기
//
public class EventUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] EventWindow window;
    [SerializeField] EventResult result;

    public enum EventUIState
    {
        NONE,
        TUTORIAL,
        CHOICE,
        RESULT1,
        RESULT2
    }
    private EventUIState currentState = EventUIState.NONE;
    public void SetState(EventUIState state)
    {
        currentState = state;

        //일단 패널을 껐다가
        window.gameObject.SetActive(false);
        result.gameObject.SetActive(false);

        switch (state)
        {
            case EventUIState.NONE:
                Time.timeScale = 1f;
                break;
            case EventUIState.TUTORIAL:
                Time.timeScale = 0f;
                window.gameObject.SetActive(true);
                window.ShowEvent("11100");
                result.gameObject.SetActive(false);
                break;
            case EventUIState.CHOICE:
                Time.timeScale = 0f;
                window.gameObject.SetActive(true);
                window.ShowEvent(InGameManager.Instance.GetCurrentEvent());
                result.gameObject.SetActive(false);
                break;
            case EventUIState.RESULT1:
                window.Clear();
                Time.timeScale = 0f;
                window.gameObject.SetActive(false);
                result.gameObject.SetActive(true);
                result.ShowEvent(InGameManager.Instance.GetCurrentEvent(), 1);
                break;
            case EventUIState.RESULT2:
                window.Clear();
                Time.timeScale = 0f;
                window.gameObject.SetActive(false);
                result.gameObject.SetActive(true);
                result.ShowEvent(InGameManager.Instance.GetCurrentEvent(), 2);
                break;
        }
        
    }
    
    public void UISetEvent(Event evt)
    {
        window.ShowEvent(evt);
    }
    public void UISetEvent(string eventID)
    {
        window.ShowEvent(eventID);
    }
}