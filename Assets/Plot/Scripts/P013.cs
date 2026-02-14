using UnityEditor.Search;
using UnityEngine;

public class P013 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int influenceDelta;


    void Reset()
    {
        // 설정 기본값
        plotID = "P013";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "심도 있는 논의";
        plotDescription = "파인애플 피자는 이단인가?";

        // 수치 기본값
        plotWeightBase = 10;
        plotWeightMultiplier = 0.15f;

        minInfluence = 0;
        pietyCost = 15;
        influenceDelta = 15;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence >= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if(!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        performer.ChangeInfluence(influenceDelta);
    }
    
}
