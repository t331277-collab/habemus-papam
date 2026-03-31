using System;
using System.Collections.Generic;
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
    bool[] triggeredEventTimes;

    public event Action<GameContextEvent> OnGameContextEvent;
    public int CurrentDay => currentDay;
    public Conclave CurrentConclave => currentConclave;
    public float RemainingTime => remainingTime;

    private Event currentEvent;
    public Event CurrentEvent => currentEvent;

    public void InitGameContext(int day=1, Conclave conclave=Conclave.Dawn)
    {
        currentDay = day;
        currentConclave = conclave;
        currentEvent = ScriptableObject.CreateInstance<E11100>();
        remainingTime = InGameManager.Instance.Balance.MaxConclaveTime;
        ResetEventTimeTriggers();
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
        ResetEventTimeTriggers();

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
            float previousRemainingTime = remainingTime;
            remainingTime -= deltaTime;
            if (remainingTime < 0) remainingTime = 0;
            CheckEventTimeThresholds(previousRemainingTime);
        }
    }

    public void ChangeRemainingTime(float deltaTime)
    {
        float previousRemainingTime = remainingTime;
        remainingTime = Mathf.Clamp(remainingTime + deltaTime, 0f, InGameManager.Instance.Balance.MaxConclaveTime);
        CheckEventTimeThresholds(previousRemainingTime);
    }

    public void StartGame()
    {
        OnGameContextEvent?.Invoke(GameContextEvent.ConclaveStart);
    }
    public void SetNewEvent()
    {
        currentEvent = InGameManager.Instance.EventManager.GetNewEvent();
    }
    public void SetEvent(Event evt)
    {
        currentEvent = evt;
    }

    void ResetEventTimeTriggers() //AI Slop
    {
        float[] eventTimes = InGameManager.Instance.Balance.EventTime;
        triggeredEventTimes = eventTimes == null ? Array.Empty<bool>() : new bool[eventTimes.Length];
    }

    void CheckEventTimeThresholds(float previousRemainingTime)
    {
        float[] eventTimes = InGameManager.Instance.Balance.EventTime;
        if (eventTimes == null || eventTimes.Length == 0) return;
        if (triggeredEventTimes == null || triggeredEventTimes.Length != eventTimes.Length)
        {
            ResetEventTimeTriggers();
        }

        for (int i = 0; i < eventTimes.Length; i++)
        {
            if (triggeredEventTimes[i]) continue;

            float threshold = eventTimes[i];
            if (previousRemainingTime > threshold && remainingTime <= threshold)
            {
                triggeredEventTimes[i] = true;
                Debug.Log(i + "번째 이벤트 발생");
                SetNewEvent();

                UIManager.Instance.Ingame.Event.UISetEvent();
            }
        }
    }
}

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [SerializeField] private GameBalance balance;
    private GameContext gameContext;
    [SerializeField] private EventManager eventManager;
    [SerializeField] private Event CurrentEvent;

    [Header("UI 연결")]
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject inventoryUIPanel;

    [Header("아이템 스폰 설정")]
    [Tooltip("아이템이 스폰될 수 있는 위치들")]
    [SerializeField] private List<Transform> spawnPoints;

    [Tooltip("필드에 드랍될 '일반' 등급 아이템 프리팹들")]
    [SerializeField] private List<GameObject> commonItemPrefabs;

    [Tooltip("필드에 드랍될 '고급' 등급 아이템 프리팹들")]
    [SerializeField] private List<GameObject> rareItemPrefabs;

    [Range(0, 100)][SerializeField] private float spawnChance = 100f;
    [Range(0, 100)][SerializeField] private float spawnTwoItemsChance = 30f; 
    [Range(0, 100)][SerializeField] private float rareItemChance = 20f; 

    private List<GameObject> spawnedFieldItems = new List<GameObject>();

    // 시간 흐름 제어 플래그
    private bool isTimeRunning = false;
    //첫 시작, 두번째 시작 판별 플래그 (공동선택 , 아이템 관련 관련 플래그)
    private bool isFirstStart = true; //아이템 스폰 관련
    public bool IsFirstStart => isFirstStart;

    private bool isSushiOn = false; //공동선택 활성화 관련
    public bool IsSushiOn => isSushiOn;

    // 프로퍼티
    public GameBalance Balance => balance;
    public GameContext Context => gameContext;
    public bool IsTimeRunning => isTimeRunning;
    public EventManager EventManager => eventManager;

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

        eventManager = GetComponent<EventManager>();
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
            isSushiOn = true;
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
        Debug.Log("타이머 작동 시작");

        if (inventoryUIPanel != null)
        {
            inventoryUIPanel.SetActive(true);
        }

        SpawnFieldItems();
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

            if (ElectionManager.Instance != null)
            {
                ElectionManager.Instance.OnConclaveEnded();
            }
        }
    }

    void InitGame()
    {
        isTimeRunning = false;

        gameContext.InitGameContext();
        if (inventoryUIPanel != null) inventoryUIPanel.SetActive(false);
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
                if (inventoryUIPanel != null)
                {
                    inventoryUIPanel.SetActive(false);
                }

                ClearFieldItems();

                // 추기경 퇴장 시작 명령
                if (CardinalManager.Instance != null)
                    CardinalManager.Instance.StopConClave();
                //판정 시작
                
                break;
        }
    }

    private void SpawnFieldItems()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return;
        if (commonItemPrefabs.Count == 0 && rareItemPrefabs.Count == 0) return;

        if (UnityEngine.Random.Range(0f, 100f) > spawnChance) return;

        int spawnCount = (UnityEngine.Random.Range(0f, 100f) <= spawnTwoItemsChance) ? 2 : 1;

        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < spawnCount; i++)
        {
            if (availablePoints.Count == 0) break;

            int pointIndex = UnityEngine.Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[pointIndex];
            availablePoints.RemoveAt(pointIndex);

            GameObject prefabToSpawn = GetRandomItemPrefab();
            if (prefabToSpawn != null)
            {
                GameObject spawnedObj = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
                spawnedFieldItems.Add(spawnedObj); // 추적 리스트에 추가
            }
        }
    }

    public GameObject GetRandomItemPrefab()
    {
        bool isRare = (UnityEngine.Random.Range(0f, 100f) <= rareItemChance);

        if (isRare && rareItemPrefabs.Count > 0)
        {
            return rareItemPrefabs[UnityEngine.Random.Range(0, rareItemPrefabs.Count)];
        }
        else if (commonItemPrefabs.Count > 0)
        {
            return commonItemPrefabs[UnityEngine.Random.Range(0, commonItemPrefabs.Count)];
        }

        return null;
    }

    private void ClearFieldItems()
    {
        foreach (var item in spawnedFieldItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        spawnedFieldItems.Clear(); // 리스트 비우기
    }

    public int GetProgress()
    {
        if (CardinalManager.Instance == null) return 0;
        CardinalManager cm = CardinalManager.Instance;

        int dayFactor = (gameContext.CurrentDay - 1) * 10;
        int hpFactor = Mathf.RoundToInt(Mathf.Clamp((400 - cm.GetCardinalHpSum()) * 0.05f, 0, 10));
        int polFactor = Mathf.RoundToInt(Mathf.Clamp(cm.GetCardinalPolSum() * 0.2f, 0, 30));

        return Mathf.Clamp((dayFactor + hpFactor + polFactor), 0 , 100);
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
    public Event GetCurrentEvent()
    {
        return gameContext.CurrentEvent;
    }
}
