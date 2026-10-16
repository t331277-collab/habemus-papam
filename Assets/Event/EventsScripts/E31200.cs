using UnityEngine;

[CreateAssetMenu(fileName = "E31200", menuName = "Events/이단과 소문")]
public class E31200 : Event
{
    [Header("회차별 경건함 조건")]
    [SerializeField] private int basePietyRequirement = 5;

    private int PietyRequirement => InGameManager.Instance != null && InGameManager.Instance.EventManager != null
        ? InGameManager.Instance.EventManager.GetBranchPietyRequirement(eventID, basePietyRequirement)
        : basePietyRequirement;

    public override string GetOption1Requirement() => $"경건함 {PietyRequirement} 이상";

    void Reset()
    {
        eventID = "E31200";
        eventName = "이단과 소문";
        maxAppear = 2;

        eventWeightBase = 40f;
        eventWeightMultiplier = 0f;

        option1Chance = 1f;
        option2Chance = 1f;
    }

    public override bool CanChoiceOption1(Cardinal performer)
    {
        return performer != null && performer.Piety >= PietyRequirement;
    }

    public override bool CanChoiceOption2(Cardinal performer)
    {
        return true;
    }

    public override bool OnChoiceOption1(Cardinal performer)
    {
        if(!CanChoiceOption1(performer)) return false;

        if(Random.value <= option1Chance)
        {  // 성공했을 때 로직
            GetCandidate(2)?.ReviveNpcForEvent(6f, 6f, 6f);
            return FinishChoice(1, true);
        }
        else
        {  // 실패했을 때 로직
            

            return FinishChoice(1, false);
        }
    }


    public override bool OnChoiceOption2(Cardinal performer)
    {
        if(!CanChoiceOption2(performer)) return false;

        if(Random.value <= option2Chance)
        {  // 성공했을 때 로직
            return FinishChoice(2, true);
        }
        else
        {  // 실패했을 때 로직
            

            return FinishChoice(2, false);
        }
    }
}
