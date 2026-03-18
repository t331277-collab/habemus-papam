using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "E21100", menuName = "Events/신앙인가, 과학인가?")]
public class E21100 : Event
{
    void Reset()
    {
        eventID = "E21100";
        eventName = "신앙인가, 과학인가?";
        maxAppear = 1;

        eventWeightBase = 0f;
        eventWeightMultiplier = 0.2f;

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
            performer.ChangeInfluence(10f);
            var aiCardinals = CardinalManager.Instance.GetAICardinlas();

            aiCardinals[0].ChangeInfluence(10f);

            // 공작 피해 증폭로직

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
            performer.ChangeInfluence(10f);
            var aiCardinals = CardinalManager.Instance.Cardinals.Where(c => !c.CompareTag("Player")).ToList();

            aiCardinals[1].ChangeInfluence(10f);

            performer.prayDeltaHpEvent = 5f;

            return true;
        }
        else
        {  // 실패했을 때 로직
            

            return false;
        }
    }
}
