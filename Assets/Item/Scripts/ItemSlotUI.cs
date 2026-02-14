using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button slotButton;

    private Item currentItem;
    private InventoryUI inventoryUI; 

    public void Setup(InventoryUI ui)
    {
        inventoryUI = ui;
        slotButton.onClick.AddListener(OnSlotClicked);
        ClearSlot(); 
    }

    public void SetItem(Item item)
    {
        currentItem = item;
        if (item != null)
        {
            iconImage.sprite = item.itemImage;
            iconImage.gameObject.SetActive(true);
            slotButton.interactable = true; 
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.gameObject.SetActive(false); 
        slotButton.interactable = false; 
    }

    private void OnSlotClicked()
    {
        if (currentItem != null)
        {
            inventoryUI.ShowDetailPanel(currentItem);
        }
    }
}