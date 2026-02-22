using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "I001", menuName = "Items/아령")]
public class I001 : Item
{
    [Header("아령 설정")]
    [SerializeField] private float initialReduction = 0.3f;
    [SerializeField] private float reductionPerTick = 0.01f;
    [SerializeField] private float tickInterval = 3.0f;
    [SerializeField] private int healAmount = 40;

    private Coroutine heavyRoutine;

    private float currentReductionLevel;

    // 초기화
    void Reset()
    {
        itemID = "I001";
        itemGrade = ItemGrade.Common;
        itemExpirationType = ItemExpirationType.Permanent;
        usageType = ItemUsageType.Active;

        itemName = "묵직한 아령";
        itemEffectDescription = "소지 시 이동 속도 30% 감소, 3초마다 1%씩 추가 감소. 사용 시 체력 40 회복.";

        initialReduction = 0.3f;
        reductionPerTick = 0.01f;
        tickInterval = 3.0f;
        healAmount = 40;
    }

    public override void OnAcquire()
    {
        currentReductionLevel = initialReduction;
        StartEffect(FindPlayer());
        Debug.Log("[아이템] 아령 획득: 무게 적용 시작");
    }

    public override void OnReapply(Cardinal owner)
    {
        StartEffect(owner);
    }

    public override void OnRemove()
    {
        Cardinal player = FindPlayer();
        StopEffect(player);
        Debug.Log("[아이템] 아령 제거: 무게 해제");
    }

    public override void OnUse()
    {
        Cardinal player = FindPlayer();
        if (player != null)
        {
            player.ChangeHp(healAmount);
        }
    }

    private void StartEffect(Cardinal target)
    {
        if (target != null)
        {
            if (heavyRoutine != null) target.StopCoroutine(heavyRoutine);

            heavyRoutine = target.StartCoroutine(BecomeHeavierRoutine(target));
        }
    }

    private void StopEffect(Cardinal target)
    {
        if (target != null)
        {
            if (heavyRoutine != null)
            {
                target.StopCoroutine(heavyRoutine);
                heavyRoutine = null;
            }
            target.RestoreMoveSpeed();
        }
    }

    private IEnumerator BecomeHeavierRoutine(Cardinal target)
    {
        while (true)
        {
            float speedMultiplier = 1.0f - currentReductionLevel;
            if (speedMultiplier < 0.01f) speedMultiplier = 0.01f;

            target.ChangeSpeed(speedMultiplier);

            yield return new WaitForSeconds(tickInterval);

            currentReductionLevel += reductionPerTick;

            if (currentReductionLevel > 0.99f) currentReductionLevel = 0.99f;
        }
    }

    private Cardinal FindPlayer()
    {
        if (InventoryManager.Instance != null) return InventoryManager.Instance.Player;
        return null;
    }
}