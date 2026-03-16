using UnityEngine;

[CreateAssetMenu(fileName = "P027", menuName = "Plot/젠체하기", order = 027)]

public class P027 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P027";
        plotGrade = PlotGrade.Legendary;
        
        // 텍스트 기본값
        plotName = "젠체하기";
        plotDescription = "큰 망신을 당할 수도...";

        // 수치 기본값
        plotWeightBase = 20;
        plotWeightMultiplier = 0f;

        minInfluence = 50;
        pietyCost = 70;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence >= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        
    }

}
