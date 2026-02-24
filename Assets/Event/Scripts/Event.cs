using UnityEngine;

public abstract class Event : ScriptableObject
{
    [SerializeField] public string eventID;
    [SerializeField] public string eventName;
    [TextArea] public string eventDescription;
    [SerializeField] public Sprite itemImage;

    [SerializeField] public float eventWeightBase;
    [SerializeField] public float eventWeightMultiplier;

    [SerializeField] public string option1;
    [SerializeField] public float option1Chance;
    [TextArea] public string option1SuccessDescription;
    [TextArea] public string option1FailDescription;

    [SerializeField] public string option2;
    [SerializeField] public float option2Chance;
    [TextArea] public string option2SuccessDescription;
    [TextArea] public string option2FailDescription;

    public float GetEventWeight()
    {
        float progressWeight = eventWeightMultiplier * InGameManager.Instance.GetProgress();
        
        return eventWeightBase + progressWeight;
    }

    // 선택지 성공시 true, 실패시 false 반환
    public abstract bool OnChoiceOption1(Cardinal performer);
<<<<<<< Updated upstream
    public abstract bool OnChocieOption2(Cardinal performer);
=======
    public abstract bool OnChoiceOption2(Cardinal performer);
>>>>>>> Stashed changes
}
