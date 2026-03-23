using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "P030", menuName = "Plot/결코 다시 전쟁!(미구현)", order = 030)]

public class P030 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int influenceDelta;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P030";
        plotGrade = PlotGrade.Legendary;

        // 수치 기본값
        plotWeightBase = 10;
        plotWeightMultiplier = 0f;

        minInfluence = 55;
        pietyCost = 0;
        influenceDelta = 20;

        // 텍스트 기본값
        plotName = "결코 다시 전쟁!(미구현)";
        plotDescription = "성전이다, 우매한 이단들아!";
        plotEffect = "모든 후보 정치력<sprite name=influence> 40 증가\n이번 콘클라베 동안 자신의 체력<sprite name=hp> 최소 1 유지";
        plotCondiText = $"<sprite name=influence>{minInfluence}<sprite name=up>";
        plotCostText = $"<sprite name=piety>  {cost}";

    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence >= minInfluence;
    }

    public override bool IsCostEnough(Cardinal performer)
    {
        return performer.Piety >= cost;
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

        /*
         * 이번 콘클라베 동안 모든 후보 체력 최소 1 유지
         */
    }

}
