using UnityEngine;

[CreateAssetMenu(fileName = "I006", menuName = "Items/최고급 태양주")]
public class I006 : Item
{
    [Header("최고급 태양주 설정")]
    [Tooltip("기도 시 추가로 회복할 체력량")]
    [SerializeField] private int prayerBonusHp;

    void Reset()
    {
        itemID = "I005";
        itemGrade = ItemGrade.Rare; 

        itemExpirationType = ItemExpirationType.Day;

        usageType = ItemUsageType.Passive;

        itemName = "최고급 태양주";
        itemDescription = "최고급 이탈리아 포도를 발효한 후 수도원 지하에서 다섯 번 증류한 화끈한 술.";
        itemEffectDescription = "오늘 기도 시 체력 회복량 두 배";

        prayerBonusHp = 10;
    }

    public override void OnPray(Cardinal owner)
    {
        float beforeHp = owner.Hp;

        // 추가 체력 회복
        owner.ChangeHp(prayerBonusHp);
    }
}