using UnityEditor.Search;
using UnityEngine;

public class P008 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minHp;
    [SerializeField] private int minInfluence;
    [SerializeField] private int minPiety;
    [SerializeField] private int pietyCost;
    [SerializeField] private int hpDelta;
    [SerializeField] private int influenceDelta;
    [SerializeField] private int pietyDelta;


    void Reset()
    {
        // 설정 기본값
        plotID = "P008";
        plotGrade = PlotGrade.Common;
        
        // 텍스트 기본값
        plotName = "삼위일체";
        plotDescription = "트포다 트포";

        // 수치 기본값
        plotWeightBase = 20;
        plotWeightMultiplier = 0f;

        minHp = 33;
        minInfluence = 33;
        minPiety = 33;
        pietyCost = 0;
        hpDelta = 33;
        influenceDelta = 3;
        pietyDelta = 3;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return (performer.Influence >= minInfluence && 
            performer.Hp >= minHp &&
            performer.Piety >= minPiety);
    }

    public override void Execute(Cardinal performer)
    {
        if(!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        performer.ChangeHp(hpDelta);
        performer.ChangeInfluence(influenceDelta);
        performer.ChangePiety(pietyDelta);
    }
    
}
