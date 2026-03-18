using UnityEngine;

[CreateAssetMenu(fileName = "E21101", menuName = "Events/충격! 암살 사건!")]
public class E21101 : Event
{
    void Reset()
    {
        eventID = "E21101";
        eventName = "충격! 암살 사건!";
        maxAppear = 1;

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

        var aiCardinals = CardinalManager.Instance.GetAICardinlas();
        if(aiCardinals[0].Influence > aiCardinals[1].Influence)
        {
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
