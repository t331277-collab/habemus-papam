using UnityEditor.Search;
using UnityEngine;

public class P005 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int hpDelta;


    void Reset()
    {
        // 설정 기본값
        plotID = "P005";
        plotGrade = PlotGrade.Common;
        
        // 텍스트 기본값
        plotName = "골탕 먹이기";
        plotDescription = "안 아 줘 요";

        // 수치 기본값
        plotWeightBase = 20;
        plotWeightMultiplier = 0f;

        minInfluence = 20;
        pietyCost = 15;
        hpDelta = -15;
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

        int targetIndex = Random.Range(0, 3);

        cm.Cardinals[targetIndex].ChangeHp(hpDelta);
    }
}
    