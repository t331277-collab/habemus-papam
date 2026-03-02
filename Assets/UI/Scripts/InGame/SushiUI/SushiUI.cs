using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SushiUI : MonoBehaviour
{
    public static SushiUI Instance { get; private set; }

    [Header("UI 연결")]
    [SerializeField] private GameObject sushiPanel;
    [SerializeField] private SushiSlot[] slots = new SushiSlot[3];
    [SerializeField] private Slider timerSlider; 
    [SerializeField] private CanvasGroup totalPanelGroup; 

    [Header("데이터")]
    [SerializeField] private List<Item> allItems;
    [SerializeField] private StatsUI statsUI;

    private Coroutine timerCoroutine;

    void Awake()
    {
        Instance = this;
        if (sushiPanel != null) sushiPanel.SetActive(false);
    }

    public void Show()
    {
        if (allItems.Count < 3) return;

        sushiPanel.SetActive(true);

        List<Item> tempItems = new List<Item>(allItems);
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, tempItems.Count);
            slots[i].Setup(tempItems[randomIndex]);
            tempItems.RemoveAt(randomIndex); 
        }

        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        int playerRank = CalculatePlayerRank();
        Debug.Log($"[SushiUI] 플레이어 순위: {playerRank}등");

        totalPanelGroup.alpha = 0.5f;
        yield return new WaitForSeconds(0.5f);
        totalPanelGroup.alpha = 1.0f;

        ApplyRankPenalty(playerRank);

        float duration = (playerRank == 4) ? 5f : 30f;
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerRoutine(duration));
    }

    private int CalculatePlayerRank()
    {
        Cardinal[] cardinals = statsUI.LinkedCardinals;
        Cardinal player = cardinals[0]; 

        var sorted = cardinals.OrderByDescending(c => c.Piety)
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

    private IEnumerator TimerRoutine(float duration)
    {
        float elapsed = 0f;
        timerSlider.maxValue = duration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            timerSlider.value = duration - elapsed;
            yield return null;
        }

        // 시간 종료 시 강제 다음 콘클라베
        Close();
        InGameManager.Instance.StartConclaveCycle();
    }

    public void Close()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        sushiPanel.SetActive(false);
    }
}