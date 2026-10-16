using UnityEngine;

[CreateAssetMenu(fileName = "E31100", menuName = "Events/반란 모의")]
public class E31100 : Event
{
    [Header("회차별 경건함 조건")]
    [SerializeField] private int basePietyRequirement = 6;

    [Header("1회차 선택지 2 전용 결과")]
    [TextArea] public string firstOption2SuccessDescription =
        "당신은 (후보 1)을(를) 지지하기로 했다. 성전 선포에 대한 결정은 다음 회의로 미뤄졌다.";
    [TextArea] public string firstOption2SuccessResult = "지지하는 후보: (후보 1)";
    [TextArea] public string firstOption2FailDescription =
        "당신은 (후보 1)을(를) 지지하기로 했다. 회의는 결론을 내리지 못했고, 논의는 다음 회의로 이어진다.";
    [TextArea] public string firstOption2FailResult = "지지하는 후보: (후보 1)";

    private bool IsFirstOccurrence => InGameManager.Instance == null ||
        InGameManager.Instance.EventManager == null ||
        InGameManager.Instance.EventManager.GetAppearCount(eventID) <= 1;

    private int PietyRequirement => InGameManager.Instance != null && InGameManager.Instance.EventManager != null
        ? InGameManager.Instance.EventManager.GetBranchPietyRequirement(eventID, basePietyRequirement)
        : basePietyRequirement;

    public override string GetOption1Requirement() => $"경건함 {PietyRequirement} 이상";

    public override string GetResultDescription(int optionIndex, bool succeeded) =>
        optionIndex == 2 && IsFirstOccurrence
            ? (succeeded ? firstOption2SuccessDescription : firstOption2FailDescription)
            : base.GetResultDescription(optionIndex, succeeded);

    public override string GetResultText(int optionIndex, bool succeeded) =>
        optionIndex == 2 && IsFirstOccurrence
            ? (succeeded ? firstOption2SuccessResult : firstOption2FailResult)
            : base.GetResultText(optionIndex, succeeded);

    void Reset()
    {
        eventID = "E31100";
        eventName = "반란모의";
        maxAppear = 2;

        eventWeightBase = 20f;
        eventWeightMultiplier = 0f;

        option1Chance = 1f;
        option2Chance = 1f;
        option1Requirement = "경건함 6 이상";
    }

    public override bool CanChoiceOption1(Cardinal performer)
    {
        if(performer == null || performer.Piety < PietyRequirement) return false;
        return true;
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
            performer.ChangePiety(4f);
            performer.ChangeInfluence(2f);

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

        bool succeeded = Random.value <= option2Chance;
        if (IsFirstOccurrence) return FinishChoice(2, succeeded);

        if(succeeded)
        {  // 성공했을 때 로직


            return FinishChoiceWithEnding(2, EndingType.Crusade);
        }
        else
        {  // 실패했을 때 로직
            

            return FinishChoice(2, false);
        }
    }
}
