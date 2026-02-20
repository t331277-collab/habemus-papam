using UnityEditor.Search;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class P029 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int hpDelta;
    [SerializeField] private int influenceDelta;


    void Reset()
    {
        // 설정 기본값
        plotID = "P029";
        plotGrade = PlotGrade.Legendary;
        
        // 텍스트 기본값
        plotName = "점심 복사 버그(미구현)";
        plotDescription = "떡과 생선이 복사가 돼요";

        // 수치 기본값
        plotWeightBase = 5;
        plotWeightMultiplier = 0f;

        minInfluence = 0;
        pietyCost = 35;
        hpDelta = 5;
        influenceDelta = 2;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Hp <= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        performer.ChangeHp(hpDelta);
        performer.ChangeInfluence(influenceDelta);

        float random = Random.Range(0f, 100f);

        if (random < performer.Piety)
        {
            // 특수 엔딩 실행 로직
        }
    }

}
