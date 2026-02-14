using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button slotButton;

    private Item currentItem;
    private InventoryUI inventoryUI; // 부모 UI 참조

    public void Setup(InventoryUI ui)
    {
        inventoryUI = ui;
        slotButton.onClick.AddListener(OnSlotClicked);
        ClearSlot(); // 시작할 땐 비워둠
    }

    // 아이템 정보 채우기
    public void SetItem(Item item)
    {
        currentItem = item;
        if (item != null)
        {
            iconImage.sprite = item.itemImage;
            iconImage.gameObject.SetActive(true);
            slotButton.interactable = true; // 클릭 가능하게
        }
        else
        {
            ClearSlot();
        }
    }

    // 빈 슬롯으로 만들기
    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.gameObject.SetActive(false); // 아이콘 숨김
        slotButton.interactable = false; // 클릭 불가능하게
    }

    // 클릭 시 부모에게 상세정보 요청
    private void OnSlotClicked()
    {
        if (currentItem != null)
        {
            inventoryUI.ShowDetailPanel(currentItem);
        }
    }
}