using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FieldItem : MonoBehaviour
{
    [Header("아이템 데이터")]
    [Tooltip("획득할 ScriptableObject 아이템 데이터 연결")]
    [SerializeField] private Item itemData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 닿은 물체가 "Player" 태그인지 확인
        if (other.CompareTag("Player"))
        {
            if (itemData != null)
            {
                // 인벤토리에 추가 시도
                bool isAdded = InventoryManager.Instance.AddItem(itemData);

                // 인벤토리에 잘 들어갔으면 필드에서 삭제
                if (isAdded)
                {
                    Debug.Log($"[아이템 획득] {itemData.itemName}");
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("인벤토리가 가득 찼습니다.");
                }
            }
        }
    }
}