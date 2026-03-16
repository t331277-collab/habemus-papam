using UnityEngine;

[CreateAssetMenu(fileName = "P003", menuName = "Plot/마니또", order = 003)]

public class P003 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int pietyDelta;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P003";
        plotGrade = PlotGrade.Common;
        
        // 텍스트 기본값
        plotName = "마니또";
        plotDescription = "소소한 선물이야";

        // 수치 기본값
        plotWeightBase = 20;
        plotWeightMultiplier = 0f;

        minInfluence = 30;
        pietyCost = 15;
        pietyDelta = 30;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence >= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if(!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        var cm = CardinalManager.Instance;

        int targetIndex = Random.Range(0, 3);

        cm.Cardinals[targetIndex].ChangePiety(pietyDelta);
    }
    
}
