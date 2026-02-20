using UnityEditor.Search;
using UnityEngine;

public class P014 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int pietyDelta;


    void Reset()
    {
        // 설정 기본값
        plotID = "P014";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "태양 경배 자세";
        plotDescription = "Yoga 고수가 될 거야!";

        // 수치 기본값
        plotWeightBase = 20;
        plotWeightMultiplier = 0f;

        minInfluence = 0;
        pietyCost = 0;
        pietyDelta = 30;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence >= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if(!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        performer.ChangeInfluence(pietyDelta);
    }
    
}
