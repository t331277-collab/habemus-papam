using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class SushiUI : MonoBehaviour
{
    public static SushiUI Instance { get; private set; }

    [Header("등급별 스프라이트 설정")]
    [SerializeField] private Sprite commonSprite;
    [SerializeField] private Sprite rareSprite;

    [Header("UI 연결")]
    [SerializeField] private GameObject sushiPanel;
    [SerializeField] private SushiSlot[] slots = new SushiSlot[3];
    [SerializeField] private CanvasGroup totalPanelGroup; 
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text footerText;
    private string defaultSubtitle;
    private string defaultFooter;

    [Header("데이터")]
    [SerializeField] private List<Item> allItems;
    [SerializeField] private StatsUI statsUI;
    public bool IsOpen => sushiPanel != null && sushiPanel.activeInHierarchy;

    void Awake()
    {
        Instance = this;
        defaultSubtitle = subtitleText != null ? subtitleText.text : string.Empty;
        defaultFooter = footerText != null ? footerText.text : string.Empty;
        if (sushiPanel != null) sushiPanel.SetActive(false);
    }

    public bool Show(System.Action onReady = null)
    {
        if (allItems == null || allItems.Count < 3) return false;

        PreparePlayerReferences();

        sushiPanel.SetActive(true);

        List<Item> tempItems = new List<Item>(allItems);
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, tempItems.Count);
            Item selectedItem = tempItems[randomIndex];
            Sprite targetSprite = (selectedItem.itemGrade == ItemGrade.Rare) ? rareSprite : commonSprite;

            slots[i].Setup(selectedItem, targetSprite);
            slots[i].SetSelectable(false);
            tempItems.RemoveAt(randomIndex); 
        }

        StartCoroutine(SequenceRoutine(onReady));
        return true;
    }

    private IEnumerator SequenceRoutine(System.Action onReady)
    {
        int playerRank = CalculatePlayerRank();

        totalPanelGroup.alpha = 0.5f;
        yield return new WaitForSeconds(0.5f);
        totalPanelGroup.alpha = 1.0f;

        ApplyRankPenalty(playerRank);
        RefreshAvailabilityNotice();

        onReady?.Invoke();
    }

    public SushiSaveData CaptureSaveData()
    {
        SushiSaveData saveData = new SushiSaveData();

        foreach (SushiSlot slot in slots)
        {
            saveData.offeredItemIds.Add(slot != null ? slot.CurrentItemId : string.Empty);
            saveData.selectableSlots.Add(slot != null && slot.IsSelectable);
        }

        return saveData;
    }

    public void RestoreFromSave(SushiSaveData saveData)
    {
        PreparePlayerReferences();
        if (saveData == null || saveData.offeredItemIds == null || saveData.offeredItemIds.Count < slots.Length)
        {
            if (!Show()) InGameManager.Instance.StartConclaveAfterItemSelection();
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            Item item = allItems.FirstOrDefault(candidate => candidate != null && candidate.itemID == saveData.offeredItemIds[i]);
            if (item == null)
            {
                if (!Show()) InGameManager.Instance.StartConclaveAfterItemSelection();
                return;
            }

            Sprite gradeSprite = item.itemGrade == ItemGrade.Rare ? rareSprite : commonSprite;
            slots[i].Setup(item, gradeSprite);
            bool selectable = saveData.selectableSlots != null && i < saveData.selectableSlots.Count &&
                saveData.selectableSlots[i];
            slots[i].SetSelectable(selectable);
        }

        sushiPanel.SetActive(true);
        totalPanelGroup.alpha = 1f;
        RefreshAvailabilityNotice();
    }

    private void RefreshAvailabilityNotice()
    {
        bool inventoryFull = IsInventoryFull();
        if (inventoryFull)
            foreach (SushiSlot slot in slots) slot.SetSelectable(false);

        bool noSelection = !slots.Any(slot => slot != null && slot.IsSelectable);
        if (subtitleText != null)
            subtitleText.text = noSelection
                ? inventoryFull ? "아이템창이 가득 차 선택할 수 없습니다!" : "낮은 순위의 후보들이 아이템을 모두 가져갔습니다!"
                : defaultSubtitle;
        if (footerText != null)
            footerText.text = noSelection ? "순위를 낮게 유지해서 아이템 선택권을 늘리세요!" : defaultFooter;
    }

    private void PreparePlayerReferences()
    {
        if (statsUI == null && CardinalManager.Instance != null) statsUI = CardinalManager.Instance.StatsUI;
        if (statsUI != null && CardinalManager.Instance != null)
            statsUI.Initialize(CardinalManager.Instance.Cardinals);
        Cardinal player = statsUI != null ? statsUI.LinkedCardinals[0] : null;
        if (player != null && InventoryManager.Instance != null) InventoryManager.Instance.SetPlayer(player);
    }

    private static bool IsInventoryFull()
    {
        return InventoryManager.Instance != null &&
            InventoryManager.Instance.InventoryItems.Count >= InventoryManager.Instance.MaxSlots;
    }

    public void OnClickClose()
    {
        if (!IsOpen) return;
        Close();
        SaveManager.Instance?.SaveCheckpoint(
            SaveCheckpointType.SushiRewardAcquired, SaveResumeStep.StartNextConclave);
        InGameManager.Instance.StartConclaveAfterItemSelection();
    }

    private int CalculatePlayerRank()
    {
        Cardinal[] cardinals = statsUI != null ? statsUI.LinkedCardinals : null;
        if (cardinals == null || cardinals[0] == null) return 1;
        Cardinal player = cardinals[0]; 

        var sorted = cardinals.Where(c => c != null).OrderByDescending(c => c.Piety)
                              .ThenByDescending(c => c.Influence)
                              .ThenBy(c => c.Hp)
                              .ThenBy(c => Random.value)
                              .ToList();

        return sorted.IndexOf(player) + 1;
    }

    private void ApplyRankPenalty(int rank)
    {
        foreach (var slot in slots) slot.SetSelectable(true);

        if (rank == 2)
        {
            slots[Random.Range(0, 3)].SetSelectable(false);
        }
        else if (rank == 3)
        {
            int safeIndex = Random.Range(0, 3); // 남겨둘 아이템
            for (int i = 0; i < 3; i++)
            {
                if (i != safeIndex) slots[i].SetSelectable(false);
            }
        }
        else if (rank == 4)
        {
            // 전체 비활성화
            foreach (var slot in slots) slot.SetSelectable(false);
        }
    }

    public void Close()
    {
        StopAllCoroutines();
        sushiPanel.SetActive(false);
    }
}
