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

    // 아이템 획득 (왼쪽부터 빈자리에 참)
    public bool AddItem(Item newItem)
    {
        // [수정] inventoryItems 사용
        if (inventoryItems.Count >= maxSlots)
        {
            Debug.Log("인벤토리 가득 참!");
            return false;
        }

        inventoryItems.Add(newItem); // 리스트에 추가하면 자동으로 0, 1, 2 순서가 됨
        newItem.OnAcquire();

        // UI 즉시 갱신
        inventoryUI.UpdateSlotUI(inventoryItems);
        return true;
    }

    // 아이템 사용 (UI에서 호출)
    public void UseItem(Item item)
    {
        // [수정] inventoryItems 사용
        if (inventoryItems.Contains(item))
        {
            item.OnUse(); // 효과 발동

            // 소모품이라면 제거 로직 (여기서는 사용 즉시 제거로 가정)
            RemoveItem(item);
        }
    }

    public void RemoveItem(Item item)
    {
        // [수정] inventoryItems 사용
        if (inventoryItems.Contains(item))
        {
            item.OnRemove();
            inventoryItems.Remove(item); // 리스트에서 빠지면 뒤에 있던 아이템들이 앞으로 당겨짐

            // UI 즉시 갱신 (빈자리가 메워짐)
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