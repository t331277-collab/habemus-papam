using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FieldItem : MonoBehaviour
{
    [Header("아이템 데이터")]
    [Tooltip("획득할 ScriptableObject 아이템 데이터 연결")]
    [SerializeField] private Item itemData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (itemData != null)
            {
                bool isAdded = InventoryManager.Instance.AddItem(itemData);

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