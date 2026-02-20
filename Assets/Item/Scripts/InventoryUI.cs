using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("슬롯 설정 (하단 바)")]
    [SerializeField] private ItemSlotUI[] slots; 

    [Header("상세 정보창 (팝업)")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI typeText; // [자동] or [수동]
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private Button useButton;   // 사용하기 버튼
    [SerializeField] private Button closeButton; // 닫기 버튼
    [SerializeField] private Button dropButton;  // 버리기(삭제)

    private Item currentDetailItem; // 현재 보고 있는 아이템

    void Start()
    {
        foreach (var slot in slots)
        {
            slot.Setup(this);
        }

        if (useButton != null) useButton.onClick.AddListener(OnUseButtonClicked);
        if (dropButton != null) dropButton.onClick.AddListener(OnDropButtonClicked);
        if (closeButton != null) closeButton.onClick.AddListener(CloseDetailPanel);

        CloseDetailPanel();
    }

    public void UpdateSlotUI(List<Item> items)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < items.Count)
            {
                slots[i].SetItem(items[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    public void ShowDetailPanel(Item item)
    {
        if (item == null) return;

        currentDetailItem = item;
        detailPanel.SetActive(true);

        detailImage.sprite = item.itemImage;
        titleText.text = item.itemName;

        if (effectText != null)
            effectText.text = item.itemEffectDescription;

        // 설정 설명 (예: 이 잔은...)
        if (descText != null)
            descText.text = item.itemDescription;

        string gradeStr = item.GetColoredGrade();
        string typeStr = (item.usageType == ItemUsageType.Active)
            ? "<color=green>[수동 사용]</color>"
            : "<color=yellow>[자동 적용]</color>";

        typeText.text = $"{gradeStr}  {typeStr}";

        if (item.usageType == ItemUsageType.Active)
        {
            typeText.text = $"{gradeStr} <color=green>[수동 사용]</color>";
            if (useButton) useButton.gameObject.SetActive(true);
        }
        else
        {
            typeText.text = $"{gradeStr} <color=yellow>[자동 적용]</color>";
            if (useButton) useButton.gameObject.SetActive(false);
        }
    }

    public void CloseDetailPanel()
    {
        detailPanel.SetActive(false);
        currentDetailItem = null;
    }

    private void OnUseButtonClicked()
    {
        if (currentDetailItem != null && currentDetailItem.usageType == ItemUsageType.Active)
        {
            InventoryManager.Instance.UseItem(currentDetailItem);

            CloseDetailPanel();
        }
    }

    private void OnDropButtonClicked()
    {
        if (currentDetailItem != null)
        {
            InventoryManager.Instance.DropItem(currentDetailItem);
            CloseDetailPanel();
        }
    }
}