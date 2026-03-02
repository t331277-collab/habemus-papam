using UnityEngine;

[CreateAssetMenu(fileName = "P018", menuName = "Plot/스몰 토크")]

public class P018 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private int hpDelta;
    [SerializeField] private int influenceDelta;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P018";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "스몰 토크";
        plotDescription = "제가 LA에 갔을 때 말이죠...";

        // 수치 기본값
        plotWeightBase = 20;
        plotWeightMultiplier = 0f;

        minInfluence = 40;
        pietyCost = 35;
        hpDelta = -15;
        influenceDelta = 20;
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

        performer.ChangeInfluence(influenceDelta);

        for (int i = 0; i < 3; i++)
        {
            cm.Cardinals[i].ChangeHp(hpDelta);
        }
    }

}
