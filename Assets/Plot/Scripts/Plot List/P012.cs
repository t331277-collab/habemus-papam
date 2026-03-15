using UnityEngine;

[CreateAssetMenu(fileName = "P012", menuName = "Plot/푹 쉬기", order = 012)]

public class P012 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int hpDelta;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P012";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "푹 쉬기";
        plotDescription = "쉬었음 추기경";

        // 수치 기본값
        plotWeightBase = 20;
        plotWeightMultiplier = 0f;

        minInfluence = 40;
        pietyCost = 35;
        hpDelta = 20;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence >= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if(!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);
        
        performer.ChangeHp(hpDelta);
    }
    
}
