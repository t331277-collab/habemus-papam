using UnityEditor.Search;
using UnityEngine;

[CreateAssetMenu(fileName = "P017", menuName = "Plot/이 불경한 자가")]

public class P017 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int hpDelta;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P017";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "이 불경한 자가";
        plotDescription = "깡!";

        // 수치 기본값
        plotWeightBase = 10;
        plotWeightMultiplier = 0.05f;

        minInfluence = 50;
        pietyCost = 50;
        hpDelta = -40;
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

        int lowestPietyCardinal = 0;

        for (int i = 1; i < 3; i++)
        {
            if (cm.Cardinals[lowestPietyCardinal].Piety > cm.Cardinals[i].Piety)
            {
                lowestPietyCardinal = i;
            }
        }

        cm.Cardinals[lowestPietyCardinal].ChangeHp(hpDelta);
    }

}
