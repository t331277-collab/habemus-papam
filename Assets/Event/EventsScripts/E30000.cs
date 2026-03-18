using UnityEngine;

[CreateAssetMenu(fileName = "E30000", menuName = "Events/새로운 전례")]
public class E30000 : Event
{
    void Reset()
    {
        eventID = "E30000";
        eventName = "새로운 전례";
        maxAppear = 1;

        eventWeightBase = 40f;
        eventWeightMultiplier = 0.1f;

        option1Chance = 1f;
        option2Chance = 0.5f;
    }

    public override bool CanChoiceOption1(Cardinal performer)
    {
        if(performer.Piety < 30) return false;
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


            return true;
        }
        else
        {  // 실패했을 때 로직
            

            return false;
        }
    }


    public override bool OnChoiceOption2(Cardinal performer)
    {
        if(!CanChoiceOption2(performer)) return false;

        if(Random.value <= option2Chance)
        {  // 성공했을 때 로직
            performer.ChangeHp(30);
            InGameManager.Instance.Context.ChangeRemainingTime(-20f);

            // 콘클라베 즉시종료 로직인데 문제가 발생할가능성있음
            InGameManager.Instance.Context.ChangeRemainingTime(-99999f);

            return true;
        }
        else
        {  // 실패했을 때 로직
            

            return false;
        }
    }
}
