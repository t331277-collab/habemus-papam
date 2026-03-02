using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ElectionManager : MonoBehaviour
{
    public static ElectionManager Instance { get; private set; }

    [Header("UI 연결")]
    [SerializeField] private StatsUI statsUI;
    [Tooltip("판정을 시작할 팝업 UI 패널")]
    [SerializeField] private GameObject judgmentStartPanel;
    [Tooltip("콘클라베 종료 후 나타나는 '판정 창 열기' 버튼")]
    [SerializeField] private Button openJudgeButton;
    [Tooltip("판정을 진행할 팝업 UI 패널")]
    [SerializeField] private GameObject judgmentPanel;
    [Tooltip("판정 팝업 내 당선 확률을 표시할 TMP 텍스트")]
    [SerializeField] private TextMeshProUGUI probabilityText;
    [Tooltip("판정 팝업 안에 있는 '판정 시작' 버튼")]
    [SerializeField] private Button startJudgmentButton;
    [Tooltip("플레이어 당선 시 (게임 오버)")]
    [SerializeField] private GameObject gameOverPanel;
    [Tooltip("NPC 당선 시 (게임 클리어)")]
    [SerializeField] private GameObject gameClearPanel;

    [Tooltip("아무도 당선되지 않았을 때(부결) 띄울 창")]
    [SerializeField] private GameObject electionFailedPanel;
    [Tooltip("부결 창을 닫는 버튼")]
    [SerializeField] private Button closeFailedPanelButton;

    [Header("당선 확률 공식 설정")]
    [Tooltip("진행도를 나눌 기준값 (기본: 100)")]
    [SerializeField] private float progressDivisor = 100f;

    [Tooltip("진행도에 곱할 지수(제곱) 값 (기본: 1.5)")]
    [SerializeField] private float progressExponent = 1.5f;

    [Tooltip("후보자 합산 점수(경건함+정치력)를 나눌 값 (기본: 2)")]
    [SerializeField] private float scoreDivisor = 2f;

    [Tooltip("최종 계산식에 곱할 가중치 (기본: 0.7)")]
    [SerializeField] private float finalWeight = 0.7f;

    [Tooltip("기본으로 깔고 가는 최소 당선 확률 (기본: 0)")]
    [SerializeField] private float baseProbability = 0f;

    [Tooltip("숫자가 빙그르르 지속될 시간 (초)")]
    [SerializeField] private float jackpotDuration = 2.5f;

    private Cardinal currentWinnerCandidate;
    private Coroutine jackpotCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (openJudgeButton != null)
        {
            openJudgeButton.onClick.AddListener(OpenJudgmentPanel);
            openJudgeButton.gameObject.SetActive(false); 
        }
        if (startJudgmentButton != null)
        {
            startJudgmentButton.onClick.AddListener(ExecuteJudgment);
        }

        if (closeFailedPanelButton != null)
        {
            closeFailedPanelButton.onClick.AddListener(CloseFailedPanel);
        }

        if (judgmentPanel != null) judgmentPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
    }

    public void OnConclaveEnded()
    {
        if (judgmentStartPanel != null)
        {
            judgmentStartPanel.gameObject.SetActive(true);
            openJudgeButton.gameObject.SetActive(true);

        }
    }

    private void OpenJudgmentPanel()
    {
        if (openJudgeButton != null) openJudgeButton.gameObject.SetActive(false);
        if(judgmentStartPanel != null) judgmentStartPanel.gameObject.SetActive(false);
        if (statsUI == null) return;

        Cardinal[] cardinals = statsUI.LinkedCardinals;
        float maxCombinedStat = -1f;
        currentWinnerCandidate = null;

        foreach (var c in cardinals)
        {
            if (c == null) continue;

            float combinedStat = c.Piety + c.Influence;
            if (combinedStat > maxCombinedStat)
            {
                maxCombinedStat = combinedStat;
                currentWinnerCandidate = c;
            }
        }

        if (currentWinnerCandidate != null)
        {
            float winProbability = CalculateWinProbability(currentWinnerCandidate);

            if (judgmentPanel != null) judgmentPanel.SetActive(true);

            if (jackpotCoroutine != null) StopCoroutine(jackpotCoroutine);
            jackpotCoroutine = StartCoroutine(JackpotRoutine(winProbability));
        }
    }

    private IEnumerator JackpotRoutine(float finalProb)
    {
        float elapsed = 0f;

        if (startJudgmentButton != null) startJudgmentButton.interactable = false;

        while (elapsed < jackpotDuration)
        {
            elapsed += Time.deltaTime;

            float randomTick = Random.Range(0f, 100f);

            if (probabilityText != null)
            {
                probabilityText.text = $" <color=black>{randomTick:F1}%</color>";
            }

            yield return null;
        }

        if (probabilityText != null)
        {
            probabilityText.text = $" <color=black>{finalProb:F1}%</color>";
        }

        if (startJudgmentButton != null) startJudgmentButton.interactable = true;

        jackpotCoroutine = null;
    }

    // 최종 확률 판정 및 게임 결과 도출
    private void ExecuteJudgment()
    {
        if (currentWinnerCandidate == null || InGameManager.Instance == null) return;

        float progress = InGameManager.Instance.GetProgress();

        float candidateScore = currentWinnerCandidate.Piety + currentWinnerCandidate.Influence;

        // 공식: (진행도 / 진행도기준값)^지수 * (후보점수 / 점수나누기값) * 가중치 + 기본확률
        float winProbability = Mathf.Pow(progress / progressDivisor, progressExponent)
                               * (candidateScore / scoreDivisor)
                               * finalWeight
                               + baseProbability;

        float diceRoll = UnityEngine.Random.Range(0f, 100f);
        bool isElected = diceRoll <= winProbability;

        if (isElected)
        {
            Debug.Log($"-> {currentWinnerCandidate.name} 당선!");

            if (currentWinnerCandidate.CompareTag("Player"))
            {
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
            }
            else
            {
                if (gameClearPanel != null) gameClearPanel.SetActive(true);
            }

            Time.timeScale = 0f;
            if (judgmentPanel != null) judgmentPanel.SetActive(false);
        }
        else
        {
            if (judgmentPanel != null) judgmentPanel.SetActive(false);
            if (electionFailedPanel != null) electionFailedPanel.SetActive(true);
        }
    }

    private void CloseFailedPanel()
    {
        if (electionFailedPanel != null)
        {
            electionFailedPanel.SetActive(false);
        }

        if (InGameManager.Instance.IsSushiOn && SushiUI.Instance != null)
        {
            SushiUI.Instance.Show();
        }
        else
        {
            InGameManager.Instance.StartConclaveCycle();
        }
    }

    private float CalculateWinProbability(Cardinal candidate)
    {
        float progress = InGameManager.Instance.GetProgress();
        float candidateScore = candidate.Piety + candidate.Influence;

        float prob = Mathf.Pow(progress / progressDivisor, progressExponent)
                   * (candidateScore / scoreDivisor)
                   * finalWeight
                   + baseProbability;

        return Mathf.Clamp(prob, 0f, 100f);
    }
}