using System;
using UnityEngine;
using UnityEngine.UI;

public class GameContext
{
    public enum Conclave
    {
        Dawn, Morning, Afternoon, Evening
    }

    public enum GameContextEvent
    {
        ConclaveStart, ConclaveEnd
    }

    int currentDay;
    Conclave currentConclave;
    float remainingTime;

    public event Action<GameContextEvent> OnGameContextEvent;
    public int CurrentDay => currentDay;
    public Conclave CurrentConclave => currentConclave;
    public float RemainingTime => remainingTime;

    public void InitGameContext(int day=1, Conclave conclave=Conclave.Dawn)
    {
        currentDay = day;
        currentConclave = conclave;
        remainingTime = InGameManager.Instance.Balance.MaxConclaveTime;
    }

    public void AdvanceConclave()
    {
        if(currentConclave == Conclave.Evening)
        {
            currentConclave = Conclave.Dawn;
            currentDay++;
        }
        else
        {
            currentConclave++;
        }

        remainingTime = InGameManager.Instance.Balance.MaxConclaveTime;

        OnGameContextEvent?.Invoke(GameContextEvent.ConclaveStart);
    }

    public void TimeOver()
    {
        OnGameContextEvent?.Invoke(GameContextEvent.ConclaveEnd);
    }

    public void Tick(float deltaTime)
    {
        if (remainingTime > 0)
        {
            remainingTime -= deltaTime;
            if (remainingTime < 0) remainingTime = 0;
        }
    }

    public void StartGame()
    {
        OnGameContextEvent?.Invoke(GameContextEvent.ConclaveStart);
    }
}

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [SerializeField] private GameBalance balance;
    private GameContext gameContext;

    [Header("UI 연결")]
    [SerializeField] private Button startButton;


    // 시간 흐름 제어 플래그
    private bool isTimeRunning = false;
    //첫 시작인지 판별 (공동선택 관련 플래그)
    private bool isFirstStart = true;

    // 프로퍼티
    public GameBalance Balance => balance;
    public GameContext Context => gameContext;
    public bool IsTimeRunning => isTimeRunning;

    void Awake()
    {
        // 싱글톤
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Awake 함수에 들어가야할 로직은 이 아래에
        gameContext = new GameContext();
        gameContext.OnGameContextEvent += HandleGameContextEvent;
    }

    void Start()
    {
        InitGame();

    }

    public void StartConclaveCycle()
    {
        if (startButton != null)
        {
            startButton.interactable = false;
            startButton.gameObject.SetActive(false);
        }

        if (isFirstStart)
        {
            Debug.Log(">>> 게임 최초 시작");
            isFirstStart = false;
            gameContext.StartGame();
        }
        else
        {
            Debug.Log(">>> 다음 콘클라베 진행");
            gameContext.AdvanceConclave();
        }
    }

    void Update()
    {
        if(isTimeRunning)
        {
            gameContext.Tick(Time.deltaTime);

            if (gameContext.RemainingTime <= 0)
            {
                StopTimer();             // 타이머 정지
                gameContext.TimeOver();  // 종료 이벤트 발생
            }

            if (CardinalManager.Instance != null)
            {
                CardinalManager.Instance.DrainAllCardinalHp(balance.HpDeltaPerSec * Time.deltaTime);
            }
        }
    }

    public void StartTimer()
    {
        isTimeRunning = true;
    }

    public void StopTimer()
    {
        isTimeRunning = false;
    }

    public void OnExitSequenceFinished()
    {
        Debug.Log("퇴장 완료.");

        if (startButton != null)
        {
            startButton.interactable = true;
            startButton.gameObject.SetActive(true);
        }
    }

    void InitGame()
    {
        isTimeRunning = false;

        gameContext.InitGameContext();
        
    }

    void HandleGameContextEvent(GameContext.GameContextEvent eventType)
    {
        switch (eventType)
        {
            case GameContext.GameContextEvent.ConclaveStart:
                Debug.Log($"[InGameManager] 콘클라베 시작: {gameContext.CurrentConclave}");

                // 추기경 입장 시작 명령
                if (CardinalManager.Instance != null)
                    CardinalManager.Instance.StartConClave();
                break;

            case GameContext.GameContextEvent.ConclaveEnd:
                Debug.Log($"[InGameManager] 콘클라베 종료 (Time Over)");

                // 추기경 퇴장 시작 명령
                if (CardinalManager.Instance != null)
                    CardinalManager.Instance.StopConClave();
                break;
        }
    }

    public int GetProgress()
    {
        if (CardinalManager.Instance == null) return 0;
        CardinalManager cm = CardinalManager.Instance;

        int dayFactor = (gameContext.CurrentDay - 1) * 10;
        int hpFactor = Mathf.RoundToInt(Mathf.Clamp((400 - cm.GetCardinalHpSum()) * 0.025f, 0, 10));
        int polFactor = Mathf.RoundToInt(Mathf.Clamp(cm.GetCardinalPolSum() * 0.075f, 0, 30));

        return dayFactor + hpFactor + polFactor;
    }

    public int GetCurrentDay()
    {
        return gameContext.CurrentDay;
    }
    public GameContext.Conclave GetCurrentConclave()
    {
        return gameContext.CurrentConclave;
    }
    public float GetRemainingTime()
    {
        return gameContext.RemainingTime;
    }
}
