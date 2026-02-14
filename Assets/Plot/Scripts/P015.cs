using UnityEditor.Search;
using UnityEngine;

public class P015 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int pietyDelta;


    void Reset()
    {
        // 설정 기본값
        plotID = "P015";
        plotGrade = PlotGrade.Common;
        
        // 텍스트 기본값
        plotName = "열렬한 찬양";
        plotDescription = "추기경님 축지법 쓰신다";

        // 수치 기본값
        plotWeightBase = 20;
        plotWeightMultiplier = 0f;

        minInfluence = 40;
        pietyCost = 15;
        pietyDelta = 30;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence >= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        var cm = CardinalManager.Instance;

        int highestInfluenceCardinal = 0;

        for (int i = 1; i < 3; i++)
        {
            if (cm.Cardinals[highestInfluenceCardinal].Piety < cm.Cardinals[i].Piety)
            {
                highestInfluenceCardinal = i;
            }
        }

        cm.Cardinals[highestInfluenceCardinal].ChangePiety(pietyDelta);
    }

}
