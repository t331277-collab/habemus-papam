using UnityEditor.Search;
using UnityEngine;

public class P021 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int influenceDelta;


    void Reset()
    {
        // 설정 기본값
        plotID = "P021";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "무릎 꿇기(미구현)";
        plotDescription = "추진력을 얻기 위함이었다!";

        // 수치 기본값
        plotWeightBase = 15;
        plotWeightMultiplier = 0.05f;

        minInfluence = 55;
        pietyCost = 10;
        influenceDelta = -15;
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

        for (int i = 0; i < 3; i++)
        {
            cm.Cardinals[i].ChangeInfluence(influenceDelta);
        }

        /*
         * 다음 날 콘클라베가 되면 다시 정치력 증가하는 함수
         */
    }

}
