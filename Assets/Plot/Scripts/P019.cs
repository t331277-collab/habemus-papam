using UnityEditor.Search;
using UnityEngine;

public class P019 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int maxInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int stunTime;


    void Reset()
    {
        // 설정 기본값
        plotID = "P019";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "목 좀 축이세요(미구현)";
        plotDescription = "푸룬 주스가 뭘까...?";

        // 수치 기본값
        plotWeightBase = 15;
        plotWeightMultiplier = 0f;

        maxInfluence = 45;
        pietyCost = 45;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence <= maxInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        var cm = CardinalManager.Instance;

        int highestHpCardinal = 0;

        for (int i = 1; i < 3; i++)
        {
            if (cm.Cardinals[highestHpCardinal].Piety < cm.Cardinals[i].Piety)
            {
                highestHpCardinal = i;
            }
        }

        /*
         * 행동 불가 함수
        cm.Cardinals[highestHpCardinal].Stun(stunTime);
        */
    }

}
