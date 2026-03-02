using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int maxSlots = 3;
    [SerializeField] private InventoryUI inventoryUI; // UI 참조
    
    private Cardinal playerCardinal;

    //프로퍼티
    public Cardinal Player => playerCardinal;

    //인벤토리
    private List<Item> inventoryItems = new List<Item>();

    //버프 관리 리스트
    private List<Item> activeBuffs = new List<Item>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
        if (InGameManager.Instance == null) return;
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
                itemsToRemove.Add(item);
            else if (item.itemExpirationType == ItemExpirationType.Day && isNewDay)
                itemsToRemove.Add(item);
        }

        foreach (var item in itemsToRemove)
        {
            Debug.Log($"'{item.itemName}' 버프/아이템 만료됨 (유형: {item.itemExpirationType})");

            item.OnRemove();
            targetList.Remove(item);

            if (playerCardinal != null)
                playerCardinal.RemovePassiveItem(item); 
        }

        if (shouldUpdateUI)
        {
            inventoryUI.UpdateSlotUI(inventoryItems);
        }
    }



    public void SetPlayer(Cardinal player)
    {
        playerCardinal = player;
        
    }



    public bool AddItem(Item newItem)
    {
        if (inventoryItems.Count >= maxSlots)
        {
            Debug.Log("인벤토리 가득 참!");
            return false;
        }

        inventoryItems.Add(newItem); 
        newItem.OnAcquire();

        if (playerCardinal != null)
        {
            if (newItem.usageType == ItemUsageType.Passive)
            {
                playerCardinal.AddPassiveItem(newItem);
            }
        }

        inventoryUI.UpdateSlotUI(inventoryItems);
        return true;
    }

    public void UseItem(Item item)
    {

        if (inventoryItems.Contains(item))
        {
            item.OnUse();

            if (item.IsDurationBuff)
            {
                inventoryItems.Remove(item);
                activeBuffs.Add(item);

                if (playerCardinal != null)
                {
                    playerCardinal.AddPassiveItem(item);
                }

                inventoryUI.UpdateSlotUI(inventoryItems);

                Debug.Log($"[시스템] '{item.itemName}'의 효과가 버프 목록에 등록되어 지속됩니다.");
            }
            else if (item.ConsumeOnUse)
            {
                RemoveItem(item);
            }
        }
    }

    public void DropItem(Item item)
    {
        if (inventoryItems.Contains(item))
        {
            RemoveItem(item);
            Debug.Log($"아이템 삭제됨: {item.itemName}");
        }
    }

    public void RemoveItem(Item item)
    {
        if (inventoryItems.Contains(item))
        {
            item.OnRemove();
            inventoryItems.Remove(item);

            if (playerCardinal != null)
            {
                playerCardinal.RemovePassiveItem(item);
            }

            inventoryUI.UpdateSlotUI(inventoryItems);
        }
    }

    
}