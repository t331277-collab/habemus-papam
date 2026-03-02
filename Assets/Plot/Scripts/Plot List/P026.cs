using UnityEngine;

[CreateAssetMenu(fileName = "P026", menuName = "Plot/주사위 굴리기(미구현)")]

public class P026 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P026";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "주사위 굴리기(미구현)";
        plotDescription = "주사위는 던져졌다";

        // 수치 기본값
        plotWeightBase = 15;
        plotWeightMultiplier = 0f;

        minInfluence = 0;
        pietyCost = 20;
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
         * '주사위' 아이템 2개 획득
         */
    }

}
