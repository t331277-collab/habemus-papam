using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "P000", menuName = "Plot/더미공작", order = 000)]

public class P000 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;


    void Reset()
    {
        // 설정 기본값
        plotID = "P000";
        plotGrade = PlotGrade.Legendary;
        
        // 텍스트 기본값
        plotName = "더미 공작";
        plotDescription = "미구현 공작 땜빵용 더미 데이터";

        // 수치 기본값
        plotWeightBase = 15;
        plotWeightMultiplier = 0f;

        minInfluence = 101;
        pietyCost = 60;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence >= minInfluence;
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
