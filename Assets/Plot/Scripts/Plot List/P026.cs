using UnityEngine;

[CreateAssetMenu(fileName = "P026", menuName = "Plot/주사위 굴리기(미구현)", order = 026)]

public class P026 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private GameObject diceItem;
    [SerializeField] private int rewardCount;

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
        rewardCount = 2;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence >= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        for (int i = 0; i < rewardCount; i++)
        {
            FieldItem rewardItem = diceItem.GetComponent<FieldItem>();

            if (rewardItem != null)
            {
                Item data = rewardItem.ItemData;

                if (data != null)
                {
                    InventoryManager.Instance.AddItem(data);
                }
            }
        }
    }

}
