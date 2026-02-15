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
        else Destroy(gameObject);
    }

    void Start()
    {
        // [추가] InGameManager의 시간 변화 이벤트 구독
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent += OnGameContextChanged;
        }
    }

    void OnDestroy()
    {
        // [추가] 오브젝트 파괴 시 이벤트 구독 해제 (메모리 누수 방지)
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent -= OnGameContextChanged;
        }
    }

    // [추가] 시간 변화 감지 함수
    private void OnGameContextChanged(GameContext.GameContextEvent eventType)
    {
        // 새로운 콘클라베(시간대)가 시작될 때
        if (eventType == GameContext.GameContextEvent.ConclaveStart)
        {
            CheckAndRemoveExpiredItems();
        }
    }

    private void CheckAndRemoveExpiredItems()
    {
        // 리스트를 순회하면서 삭제할 때 오류(Collection Modified)를 방지하기 위해
        // 삭제할 아이템을 먼저 임시 리스트에 담습니다.
        List<Item> itemsToRemove = new List<Item>();

        foreach (var item in inventoryItems)
        {
            // 조건: 만료 타입이 'Conclave'인 경우
            if (item.itemExpirationType == ItemExpirationType.Conclave)
            {
                itemsToRemove.Add(item);
            }
        }

        // 임시 리스트에 담긴 아이템들을 실제로 제거
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