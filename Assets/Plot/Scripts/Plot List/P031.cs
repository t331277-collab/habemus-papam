using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "P031", menuName = "Plot/숙면(미구현)")]

public class P031 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int influenceTarget;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P031";
        plotGrade = PlotGrade.Legendary;
        
        // 텍스트 기본값
        plotName = "숙면(미구현)";
        plotDescription = "드르렁 쿨쿨...";

        // 수치 기본값
        plotWeightBase = 10;
        plotWeightMultiplier = 0.05f;

        minInfluence = 85;
        pietyCost = 50;
        influenceTarget = 20;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Hp <= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        float currentInfluence = performer.Influence;

        performer.ChangeInfluence(-currentInfluence);
        performer.ChangeInfluence(influenceTarget);

        /*
         * 이번 콘클라베 동안 행동 불가
         */
    }

}
