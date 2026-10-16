using UnityEngine;

[CreateAssetMenu(fileName = "I008", menuName = "Items/태양 팔찌")]
public class I008 : Item
{
    [System.Serializable]
    private class RuntimeState
    {
        public int lastRewardDay;
    }

    private int lastRewardDay = -1;
    private GameContext subscribedContext;

    void Reset()
    {
        itemID = "I008";
        itemName = "태양 팔찌";
        itemDescription = "태양의 힘이 확실히 느껴지는 경건한 팔찌다!";
        itemEffectDescription = "매일 <color=#4488FF>정치력</color> 1, <color=#FFD84D>경건함</color> 1, <color=#5BD65B>체력</color> 1 획득. 버릴 수 없다.";

        itemGrade = ItemGrade.Common;
        itemExpirationType = ItemExpirationType.Permanent;
        usageType = ItemUsageType.Passive;
    }

    public override bool CanDrop() => false;

    public override void OnAcquire()
    {
        // The first daily reward is at the start of the day after acquisition.
        lastRewardDay = InGameManager.Instance != null ? InGameManager.Instance.GetCurrentDay() : -1;
        SubscribeToDays();
    }

    public override void OnReapply(Cardinal owner)
    {
        // Old bracelet saves have no daily state; do not grant a bonus just for loading.
        if (lastRewardDay < 0 && InGameManager.Instance != null)
            lastRewardDay = InGameManager.Instance.GetCurrentDay();
        SubscribeToDays();
    }

    public override void OnRemove() => UnsubscribeFromDays();

    private void SubscribeToDays()
    {
        GameContext context = InGameManager.Instance != null ? InGameManager.Instance.Context : null;
        if (context == null || context == subscribedContext) return;
        UnsubscribeFromDays();
        subscribedContext = context;
        subscribedContext.OnGameContextEvent += HandleContextEvent;
    }

    private void UnsubscribeFromDays()
    {
        if (subscribedContext == null) return;
        subscribedContext.OnGameContextEvent -= HandleContextEvent;
        subscribedContext = null;
    }

    private void HandleContextEvent(GameContext.GameContextEvent eventType)
    {
        if (eventType != GameContext.GameContextEvent.ConclaveStart || InGameManager.Instance == null) return;
        int day = InGameManager.Instance.GetCurrentDay();
        if (day <= lastRewardDay) return;
        Cardinal player = InventoryManager.Instance != null ? InventoryManager.Instance.Player : null;
        if (player == null) return;
        lastRewardDay = day;
        player.ChangeInfluence(1f);
        player.ChangePiety(1f);
        player.ChangeHp(1f);
    }

    public override void ResetRuntimeState()
    {
        UnsubscribeFromDays();
        lastRewardDay = -1;
    }

    public override string CaptureRuntimeState() =>
        JsonUtility.ToJson(new RuntimeState { lastRewardDay = lastRewardDay });

    public override void RestoreRuntimeState(string runtimeStateJson)
    {
        ResetRuntimeState();
        if (string.IsNullOrWhiteSpace(runtimeStateJson)) return;
        RuntimeState state = JsonUtility.FromJson<RuntimeState>(runtimeStateJson);
        if (state != null) lastRewardDay = state.lastRewardDay;
    }
}
