using System.Collections.Generic;
using UnityEngine;

public abstract class Event : ScriptableObject
{
    [SerializeField] public string eventID;
    [SerializeField] public string eventName;
    [TextArea] public string eventDescription;
    [SerializeField] public Sprite itemImage;
    [SerializeField] public int maxAppear;

    [SerializeField] public float eventWeightBase;
    [SerializeField] public float eventWeightMultiplier;

    [SerializeField] public List<Event> preEvents;
    [SerializeField] public List<Event> conflictEvents;

    [SerializeField] public string option1;
    [SerializeField] public float option1Chance;
    [SerializeField] public string option1Requirement = "";
    [TextArea] public string option1SuccessDescription;
    [TextArea] public string option1SuccessResult;
    [TextArea] public string option1FailDescription;
    [TextArea] public string option1FailResult;

    [SerializeField] public string option2;
    [SerializeField] public float option2Chance;
    [TextArea] public string option2SuccessDescription;
    [TextArea] public string option2SuccessResult;
    [TextArea] public string option2FailDescription;
    [TextArea] public string option2FailResult;

    public float GetEventWeight()
    {
        float progressWeight = eventWeightMultiplier * InGameManager.Instance.GetProgress();
        
        return eventWeightBase + progressWeight;
    }

    public virtual bool CanChoiceOption1(Cardinal performer)
    {
        return true;
    }
    public virtual bool CanChoiceOption2(Cardinal performer)
    {
        return true;
    }

    public abstract bool OnChoiceOption1(Cardinal performer);
    public abstract bool OnChoiceOption2(Cardinal performer);
}
