using UnityEngine;

[CreateAssetMenu(fileName = "E60000", menuName = "Events/거룩하도다")]
public class E60000 : Event
{
    [SerializeField] private Item sunBracelet;
    [System.NonSerialized] private bool sunBraceletAcquired;

    void Reset()
    {
        eventID = "E60000";
        eventName = "거룩하도다";
        eventDescription = "(랜덤 후보)가 당신의 열렬한 기도를 보고 감명받습니다.";
        maxAppear = 1;
        eventWeightBase = 1f;
        eventWeightMultiplier = 0f;
        option1 = "감사를 표한다";
        option1Chance = 1f;
        option1Requirement = "";
        option1SuccessDescription = "당신이 감사를 표하자, 그는 당신에게 태양 팔찌를 쥐어 주며 지지를 약속하였다";
        option1SuccessResult = "정치력 +2\n아이템 '태양 팔찌' 획득";
        option2 = "";
        option2Chance = 0f;
    }

    public override bool CanChoiceOption1(Cardinal performer) =>
        performer != null && sunBracelet != null && InventoryManager.Instance != null;
    public override bool CanChoiceOption2(Cardinal performer) => false;

    public override bool OnChoiceOption1(Cardinal performer)
    {
        if (!CanChoiceOption1(performer)) return false;
        performer.ChangeInfluence(2f);
        sunBraceletAcquired = InventoryManager.Instance.AddItem(sunBracelet);
        return FinishChoice(1, true);
    }

    public override bool OnChoiceOption2(Cardinal performer) => false;

    public override string GetResultText(int optionIndex, bool succeeded)
    {
        if (optionIndex == 1 && succeeded && !sunBraceletAcquired)
            return "정치력 +2\n인벤토리가 가득 차 태양 팔찌를 획득하지 못했습니다.";
        return base.GetResultText(optionIndex, succeeded);
    }
}
