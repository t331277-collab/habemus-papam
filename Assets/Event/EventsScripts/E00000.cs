using UnityEngine;

[CreateAssetMenu(fileName = "E00000", menuName = "Events/이벤트이름")]
public class E00000 : Event
{
    void Reset()
    {
        eventID = "E00000";
        eventName = "이벤트이름";
        maxAppear = 2;

        eventWeightBase = 0f;
        eventWeightMultiplier = 0f;

        option1Chance = 1f;
        option2Chance = 1f;
    }

    public override bool CanChoiceOption1(Cardinal performer)
    {
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


            return true;
        }
        else
        {  // 실패했을 때 로직
            

            return false;
        }
    }
}
