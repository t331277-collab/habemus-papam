using UnityEditor.Search;
using UnityEngine;

public class P025 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int influenceDelta;


    void Reset()
    {
        // 설정 기본값
        plotID = "P025";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "앙코르(미구현)";
        plotDescription = "좋은 건 한 번 더 해야지";

        // 수치 기본값
        plotWeightBase = 10;
        plotWeightMultiplier = 0.1f;

        minInfluence = 60;
        pietyCost = 40;
        influenceDelta = 30;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Hp >= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        /*
         * 지난 콘클라베에서 선출되었던 후보 정치력 증가 로직 예정
         */
    }

}
