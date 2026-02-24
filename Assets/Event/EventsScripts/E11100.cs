using Unity.Multiplayer.Center.Common;
using Unity.VisualScripting;
using UnityEngine;

public class E11100 : Event
{
    void Reset()
    {
        eventID = "E11100";
        eventName = "큰일났다!";

        eventWeightBase = 0f;
        eventWeightMultiplier = 0f;

        option1Chance = 1f;
        option2Chance = 1f;
    }

    public override bool OnChoiceOption1(Cardinal performer)
    {
        if(Random.value > option1Chance) return false;

        performer.ChangePiety(80f);
        performer.ChangeInfluence(40f);
        performer.hpDrainMultiplier *= 2f;

        return true;
    }

    public override bool OnChocieOption2(Cardinal performer)
    {
        return true;
    }
}
