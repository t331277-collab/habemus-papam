using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "P034", menuName = "Plot/언더독", order = 034)]

public class P034 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int pietyDelta;

    public override int cost => pietyCost;


    void Reset()
    {
        // 설정 기본값
        plotID = "P034";
        plotGrade = PlotGrade.Legendary;
        
        // 텍스트 기본값
        plotName = "언더독";
        plotDescription = "중요한 것은 깎이지 않는 마음";

        // 수치 기본값
        plotWeightBase = 20;
        plotWeightMultiplier = 0f;

        minInfluence = 50;
        pietyCost = 40;
        pietyDelta = 60;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Hp <= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        var cm = CardinalManager.Instance;

        int lowestHpCardinal = 0;

        for (int i = 1; i < 3; i++)
        {
            if (cm.Cardinals[lowestHpCardinal].Piety > cm.Cardinals[i].Piety)
            {
                lowestHpCardinal = i;
            }
        }

        if (cm.Cardinals[lowestHpCardinal].Hp > performer.Hp)
        {
            performer.ChangePiety(pietyDelta);
        }
        else
        {
            cm.Cardinals[lowestHpCardinal].ChangePiety(pietyDelta);
        }
    }

}
