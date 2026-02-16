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

    private List<Item> inventoryItems = new List<Item>();

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
        if (eventType == GameContext.GameContextEvent.ConclaveStart)
        {
            CheckAndRemoveExpiredItems();
        }
    }

    private void CheckAndRemoveExpiredItems()
    {
        List<Item> itemsToRemove = new List<Item>();

        bool isNewDay = InGameManager.Instance.GetCurrentConclave() == GameContext.Conclave.Dawn;

        foreach (var item in inventoryItems)
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
            Debug.Log($" '{item.itemName}' 아이템이 만료되어 사라졌습니다.");
            RemoveItem(item);
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
            playerCardinal.AddPassiveItem(newItem);
        }

        inventoryUI.UpdateSlotUI(inventoryItems);
        return true;
    }

    public void UseItem(Item item)
    {

        if (inventoryItems.Contains(item))
        {
            item.OnUse(); // 효과 발동

            // 소모품이라면 제거
            RemoveItem(item);
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