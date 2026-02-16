using UnityEngine;

[CreateAssetMenu(fileName = "I004", menuName = "Items/은으로 만든 성배")]
public class I004 : Item
{
    [Header("은으로 만든 성배")]
    [SerializeField] private int hpDelta;
    [SerializeField] private int influenceDelta;

    void Reset()
    {
        // 설정 기본값
        itemID = "I004";
        itemGrade = ItemGrade.Common;
        itemExpirationType = ItemExpirationType.Conclave;
        usageType = ItemUsageType.Passive;

        // 텍스트 기본값
        itemName = "은으로 만든 성배";
        itemEffectDescription = "이번 콘클라베 동안 기도 시 체력 회복량 5 감소, 연설 시 정치력 회복량 5 증가";
        itemDescription = "이 잔에 미사를 할 때마다 태양주를 한 잔씩 마실 수 있다. 그것이 미사니까! (끄덕)";
        

        // 수치 기본값
        hpDelta = -5;
        influenceDelta = 5;
    }

    public override void OnPray(Cardinal owner)
    {
        // 확인용 로그 (이전 체력)
        float beforeHp = owner.Hp;

        owner.ChangeHp(hpDelta);

        Debug.Log($"[아이템 효과 발동] 기도: 체력 {beforeHp} -> {owner.Hp} (변화량: {hpDelta})");
    }

    // 2. 연설할 때: 정치력(Influence)을 올림
    public override void OnSpeech(Cardinal owner)
    {
        // 확인용 로그 (이전 정치력)
        float beforeInf = owner.Influence;

        // ChangePiety가 아니라 ChangeInfluence를 써야 함!
        owner.ChangeInfluence(influenceDelta);

        Debug.Log($"[아이템 효과 발동] 연설: 정치력 {beforeInf} -> {owner.Influence} (변화량: {influenceDelta})");
    }
}
