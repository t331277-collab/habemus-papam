using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int maxSlots = 3;
    [SerializeField] private InventoryUI inventoryUI;

    private Cardinal playerCardinal;
    private List<Item> inventoryItems = new List<Item>();
    private List<Item> activeBuffs = new List<Item>();

    public Cardinal Player => playerCardinal;
    public IReadOnlyList<Item> InventoryItems => inventoryItems;
    public IReadOnlyList<Item> ActiveBuffs => activeBuffs;
    public int MaxSlots => maxSlots;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent += OnGameContextChanged;
        }
    }

    void OnDestroy()
    {
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent -= OnGameContextChanged;
        }
    }

    private void OnGameContextChanged(GameContext.GameContextEvent eventType)
    {
        if (eventType == GameContext.GameContextEvent.ConclaveEnd)
        {
            CheckAndRemoveExpiredItems();
        }
    }

    private void CheckAndRemoveExpiredItems()
    {
        if (InGameManager.Instance == null)
        {
            return;
        }

        bool isEndOfDay = InGameManager.Instance.GetCurrentConclave() == GameContext.Conclave.Evening;

        RemoveExpiredFromList(inventoryItems, isEndOfDay, true);
        RemoveExpiredFromList(activeBuffs, isEndOfDay, false);
    }

    private void RemoveExpiredFromList(List<Item> targetList, bool isNewDay, bool shouldUpdateUI)
    {
        List<Item> itemsToRemove = new List<Item>();

        foreach (var item in targetList)
        {
            if (item.itemExpirationType == ItemExpirationType.Conclave)
            {
                itemsToRemove.Add(item);
            }
            else if (item.itemExpirationType == ItemExpirationType.Day && isNewDay)
            {
                itemsToRemove.Add(item);
            }
        }

        foreach (var item in itemsToRemove)
        {
            Debug.Log($"'{item.itemName}' 버프/아이템 만료됨 (유형: {item.itemExpirationType})");

            item.OnRemove();
            targetList.Remove(item);

            if (playerCardinal != null)
            {
                playerCardinal.RemovePassiveItem(item);
            }
        }

        if (shouldUpdateUI && inventoryUI != null)
        {
            inventoryUI.UpdateSlotUI(inventoryItems);
        }
    }

    public void SetPlayer(Cardinal player)
    {
        playerCardinal = player;
    }

    public Item GetItemByID(string id)
    {
        foreach (var item in inventoryItems)
        {
            if (item != null && item.itemID == id)
            {
                return item;
            }
        }

        foreach (var item in activeBuffs)
        {
            if (item != null && item.itemID == id)
            {
                return item;
            }
        }

        return null;
    }

    public bool AddItem(Item newItem)
    {
        if (inventoryItems.Count >= maxSlots)
        {
            Debug.Log("인벤토리가 가득 참");
            return false;
        }

        inventoryItems.Add(newItem);
        newItem.OnAcquire();

        if (playerCardinal != null && newItem.usageType == ItemUsageType.Passive)
        {
            playerCardinal.AddPassiveItem(newItem);
        }

        if (inventoryUI != null)
        {
            inventoryUI.UpdateSlotUI(inventoryItems);
        }

        return true;
    }

    public void UseItem(Item item)
    {
        if (!inventoryItems.Contains(item))
        {
            return;
        }

        item.OnUse();

        if (item.IsDurationBuff)
        {
            inventoryItems.Remove(item);
            activeBuffs.Add(item);

            if (playerCardinal != null)
            {
                playerCardinal.AddPassiveItem(item);
            }

            if (inventoryUI != null)
            {
                inventoryUI.UpdateSlotUI(inventoryItems);
            }

            Debug.Log($"[시스템] '{item.itemName}'의 효과가 버프 목록에 등록되었습니다.");
        }
        else if (item.ConsumeOnUse)
        {
            RemoveItem(item);
        }
    }

    public void DropItem(Item item)
    {
        if (!inventoryItems.Contains(item))
        {
            return;
        }

        RemoveItem(item);
        Debug.Log($"아이템 제거: {item.itemName}");
    }

    public void RemoveItem(Item item)
    {
        if (!inventoryItems.Contains(item))
        {
            return;
        }

        item.OnRemove();
        inventoryItems.Remove(item);

        if (playerCardinal != null)
        {
            playerCardinal.RemovePassiveItem(item);
        }

        if (inventoryUI != null)
        {
            inventoryUI.UpdateSlotUI(inventoryItems);
        }
    }

    public InventorySaveData CaptureSaveData()
    {
        InventorySaveData saveData = new InventorySaveData
        {
            maxSlots = maxSlots
        };

        foreach (var item in inventoryItems)
        {
            if (item == null)
            {
                continue;
            }

            saveData.inventoryItems.Add(new ItemSaveData
            {
                itemId = item.itemID,
                runtimeStateJson = item.CaptureRuntimeState()
            });
        }

        foreach (var item in activeBuffs)
        {
            if (item == null)
            {
                continue;
            }

            saveData.activeBuffs.Add(new ItemSaveData
            {
                itemId = item.itemID,
                runtimeStateJson = item.CaptureRuntimeState()
            });
        }

        return saveData;
    }

    public void RestoreFromSave(InventorySaveData saveData, Dictionary<string, Item> itemCatalog)
    {
        inventoryItems.Clear();
        activeBuffs.Clear();

        if (playerCardinal != null)
        {
            playerCardinal.ClearPassiveItems();
        }

        if (saveData == null || itemCatalog == null)
        {
            if (inventoryUI != null)
            {
                inventoryUI.UpdateSlotUI(inventoryItems);
            }
            return;
        }

        RestoreItemList(saveData.inventoryItems, inventoryItems, itemCatalog);
        RestoreItemList(saveData.activeBuffs, activeBuffs, itemCatalog);

        ReapplyItemsAfterLoad();

        if (inventoryUI != null)
        {
            inventoryUI.UpdateSlotUI(inventoryItems);
        }
    }

    private void RestoreItemList(List<ItemSaveData> source, List<Item> target, Dictionary<string, Item> itemCatalog)
    {
        if (source == null)
        {
            return;
        }

        foreach (var itemSave in source)
        {
            if (itemSave == null || string.IsNullOrWhiteSpace(itemSave.itemId))
            {
                continue;
            }

            if (!itemCatalog.TryGetValue(itemSave.itemId, out Item item) || item == null)
            {
                Debug.LogWarning($"[Save] 아이템 '{itemSave.itemId}'를 찾지 못해 복원을 건너뜁니다.");
                continue;
            }

            item.RestoreRuntimeState(itemSave.runtimeStateJson);
            target.Add(item);
        }
    }

    private void ReapplyItemsAfterLoad()
    {
        if (playerCardinal == null)
        {
            return;
        }

        playerCardinal.ClearPassiveItems();

        foreach (var item in inventoryItems)
        {
            if (item == null)
            {
                continue;
            }

            item.OnReapply(playerCardinal);

            if (item.usageType == ItemUsageType.Passive)
            {
                playerCardinal.AddPassiveItem(item);
            }
        }

        foreach (var item in activeBuffs)
        {
            if (item == null)
            {
                continue;
            }

            item.OnReapply(playerCardinal);
            playerCardinal.AddPassiveItem(item);
        }
    }
}
