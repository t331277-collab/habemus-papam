using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int maxSlots = 3;
    [SerializeField] private InventoryUI inventoryUI; // UI 참조
    
    private Cardinal playerCardinal;

    private List<Item> inventoryItems = new List<Item>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SetPlayer(Cardinal player)
    {
        playerCardinal = player;
        Debug.Log("인벤토리 매니저에 플레이어가 연결되었습니다.");
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

    public void RemoveItem(Item item)
    {
        if (inventoryItems.Contains(item))
        {
            item.OnRemove();
            inventoryItems.Remove(item); 

            inventoryUI.UpdateSlotUI(inventoryItems);
        }
    }

    public void TriggerPassiveEffects_Pray()
    {
        foreach (var item in inventoryItems)
        {
            if (item.usageType == ItemUsageType.Passive) item.OnPray(playerCardinal);
        }
    }

    public void TriggerPassiveEffects_Speech()
    {
        foreach (var item in inventoryItems)
        {
            if (item.usageType == ItemUsageType.Passive) item.OnSpeech(playerCardinal);
        }
    }
}