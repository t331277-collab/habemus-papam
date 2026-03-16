using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "P033", menuName = "Plot/나는 용서하마(미구현)", order = 033)]

public class P033 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P033";
        plotGrade = PlotGrade.Legendary;
        
        // 텍스트 기본값
        plotName = "나는 용서하마(미구현)";
        plotDescription = "하지만 이 녀석이 용서할까?";

        // 수치 기본값
        plotWeightBase = 15;
        plotWeightMultiplier = 0f;

        minInfluence = 60;
        pietyCost = 60;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Hp <= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        /*
         * 다음 콘클라베 때, 이번 콘클라베 동안 감소한 체력만큼 모든 상대 후보의 체력 감소
         */
    }

}
