using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("슬롯 설정 (하단 바)")]
    [SerializeField] private ItemSlotUI[] slots; // 슬롯 3개 연결

    [Header("상세 정보창 (팝업)")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI typeText; // [자동] or [수동]
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Button useButton;   // 사용하기 버튼
    [SerializeField] private Button closeButton; // 닫기 버튼

    private Item currentDetailItem; // 현재 보고 있는 아이템

    void Start()
    {
        // 슬롯 초기화
        foreach (var slot in slots)
        {
            slot.Setup(this);
        }

        // 버튼 리스너 연결
        if (useButton != null) useButton.onClick.AddListener(OnUseButtonClicked);
        if (closeButton != null) closeButton.onClick.AddListener(CloseDetailPanel);

        // 시작 시 상세창 닫기
        CloseDetailPanel();
    }

    // InventoryManager에서 호출: 데이터가 변경되면 UI 갱신
    public void UpdateSlotUI(List<Item> items)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < items.Count)
            {
                // 왼쪽(0번)부터 데이터 채움
                slots[i].SetItem(items[i]);
            }
            else
            {
                // 나머지는 비움
                slots[i].ClearSlot();
            }
        }
    }

    // 슬롯 클릭 시 호출됨 -> 상세창 띄우기
    public void ShowDetailPanel(Item item)
    {
        if (item == null) return;

        currentDetailItem = item;
        detailPanel.SetActive(true); // 패널 켜기

        // 정보 매핑
        detailImage.sprite = item.itemImage;
        titleText.text = item.itemName;
        descText.text = item.itemDescription;

        // 타입에 따른 버튼/텍스트 처리
        if (item.usageType == ItemUsageType.Active)
        {
            typeText.text = "<color=green>[수동 사용]</color>";
            useButton.gameObject.SetActive(true); // 사용 버튼 보임
        }
        else
        {
            typeText.text = "<color=yellow>[자동 적용]</color>";
            useButton.gameObject.SetActive(false); // 사용 버튼 숨김 (패시브니까)
        }
    }

    public void CloseDetailPanel()
    {
        detailPanel.SetActive(false);
        currentDetailItem = null;
    }

    // 상세창의 '사용하기' 버튼 클릭 시
    private void OnUseButtonClicked()
    {
        if (currentDetailItem != null && currentDetailItem.usageType == ItemUsageType.Active)
        {
            // 매니저에게 사용 요청
            InventoryManager.Instance.UseItem(currentDetailItem);

            // 사용 후 창 닫기
            CloseDetailPanel();
        }
    }
}