
using UnityEngine;

[CreateAssetMenu(fileName = "P006", menuName = "Plot/커피 회동", order = 006)]

public class P006 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int influenceDelta;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P006";
        plotGrade = PlotGrade.Common;
        
        // 텍스트 기본값
        plotName = "커피 회동";
        plotDescription = "커피 한 잔의 여유";

        // 수치 기본값
        plotWeightBase = 15;
        plotWeightMultiplier = 0f;

        minInfluence = 25;
        pietyCost = 20;
        influenceDelta = 10;
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

        performer.ChangeInfluence(influenceDelta);

        for (int i = 0; i < 3; i++)
        {
            cm.Cardinals[i].ChangeInfluence(influenceDelta);
        }
    }
}
    
