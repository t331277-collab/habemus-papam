using UnityEngine;

[CreateAssetMenu(fileName = "P023", menuName = "Plot/노블레스 오블리주")]

public class P023 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int pietyIncrease;
    [SerializeField] private int pietyDecrease;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P023";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "노블레스 오블리주";
        plotDescription = "큰 힘에는 큰 책임이 따른다";

        // 수치 기본값
        plotWeightBase = 15;
        plotWeightMultiplier = 0f;

        minInfluence = 0;
        pietyCost = 0;
        pietyIncrease = 30;
        pietyDecrease = -30;
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

        int highestInfluCardinal = 0;
        int lowestInfluCardinal = 0;

        for (int i = 1; i < 3; i++)
        {
            if (cm.Cardinals[lowestInfluCardinal].Piety > cm.Cardinals[i].Piety)
            {
                lowestInfluCardinal = i;
            }
            if (cm.Cardinals[highestInfluCardinal].Piety < cm.Cardinals[i].Piety)
            {
                highestInfluCardinal = i;
            }
        }

        cm.Cardinals[lowestInfluCardinal].ChangePiety(pietyIncrease);

        cm.Cardinals[highestInfluCardinal].ChangePiety(pietyDecrease);
    }

}
