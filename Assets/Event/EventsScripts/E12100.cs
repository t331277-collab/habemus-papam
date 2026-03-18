using UnityEngine;

[CreateAssetMenu(fileName = "E12100", menuName = "Events/태양의 눈")]
public class E12100 : Event
{
    void Reset()
    {
        eventID = "E12100";
        eventName = "태양의 눈";
        maxAppear = 2;

        eventWeightBase = 30f;
        eventWeightMultiplier = 0f;

        option1Chance = 1f;
        option2Chance = 0.5f;

        // 선행 충돌 이벤트는 일단 인스펙터에서 드래그드롭으로처리
        //preEvents.Add(InGameManager.Instance.EventManager.GetEventById("E11100"));
    }

    public override bool CanChoiceOption1(Cardinal performer)
    {
        if(performer.Piety >= 30f) return true;
        else return false;
    }

    public override bool CanChoiceOption2(Cardinal performer)
    {
        return true;
    }

    public override bool OnChoiceOption1(Cardinal performer)
    {
        if(Random.value > option1Chance) return false;
        if(!CanChoiceOption1(performer)) return false;

        performer.ChangeInfluence(5f);

        return true;
    }


    public override bool OnChoiceOption2(Cardinal performer)
    {
        if(!CanChoiceOption2(performer)) return false;

        if(Random.value <= option2Chance)
        {
            performer.ChangeHp(-10f);
            return true;
        }
        else
        {
            performer.ChangeHp(-20f);
            return false;
        }
    }
}
