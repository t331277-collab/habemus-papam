using UnityEditor.Search;
using UnityEngine;

public class P004 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int influenceDelta;


    void Reset()
    {
        // 설정 기본값
        plotID = "P004";
        plotGrade = PlotGrade.Common;
        
        // 텍스트 기본값
        plotName = "광대 놀음";
        plotDescription = "상갓집 개 노릇";

        // 수치 기본값
        plotWeightBase = 10;
        plotWeightMultiplier = 0.1f;

        minInfluence = 60;
        pietyCost = 30;
        influenceDelta = -10;
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
