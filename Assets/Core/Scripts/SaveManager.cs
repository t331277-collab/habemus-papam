using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private const string MainSceneName = "MainScene";
    private const string GameSceneName = "GameScene";
    private const string SaveFolderName = "Json";
    private const string SaveFileName = "autosave.json";

    public static SaveManager Instance { get; private set; }

    private bool pendingLoad = false;
    private bool pendingNewGame = false;
    private bool isApplyingLoad = false;

    public string SaveDirectoryPath => Path.Combine(Application.persistentDataPath, SaveFolderName);
    public string SaveFilePath => Path.Combine(SaveDirectoryPath, SaveFileName);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject saveManagerObject = new GameObject(nameof(SaveManager));
        DontDestroyOnLoad(saveManagerObject);
        Instance = saveManagerObject.AddComponent<SaveManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public bool HasSave()
    {
        return File.Exists(SaveFilePath);
    }

    public bool TryGetSavePreview(out SavePreviewData preview)
    {
        preview = null;

        if (!TryReadSave(out SaveModel saveModel) || saveModel == null || saveModel.gameContext == null)
        {
            return false;
        }

        CardinalSaveData playerSave = null;
        if (saveModel.cardinals != null)
        {
            foreach (var cardinalSave in saveModel.cardinals)
            {
                if (cardinalSave != null && cardinalSave.isPlayer)
                {
                    playerSave = cardinalSave;
                    break;
                }
            }
        }

        if (playerSave == null)
        {
            Debug.LogWarning("[Save] 플레이어 저장 데이터를 찾지 못해 기본값으로 미리보기를 구성합니다.");
        }

        int conclaveIndex = Mathf.Clamp(
            saveModel.gameContext.conclave,
            0,
            Enum.GetValues(typeof(GameContext.Conclave)).Length - 1);
        GameContext.Conclave conclave = (GameContext.Conclave)conclaveIndex;

        preview = new SavePreviewData
        {
            playerHp = playerSave != null ? playerSave.hp : 0f,
            playerInfluence = playerSave != null ? playerSave.influence : 0f,
            playerPiety = playerSave != null ? playerSave.piety : 0f,
            day = Mathf.Max(1, saveModel.gameContext.day),
            conclave = conclaveIndex,
            conclaveName = conclave.ToString()
        };

        return true;
    }

    public void StartNewGame()
    {
        pendingLoad = false;
        pendingNewGame = true;
        Time.timeScale = 1f;

        DeleteSave();
        SceneManager.LoadScene(GameSceneName);
    }

    public void LoadGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("[Save] 저장 파일이 없어 Load를 진행할 수 없습니다.");
            return;
        }

        pendingNewGame = false;
        pendingLoad = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(GameSceneName);
    }

    public void GoToMainMenu()
    {
        pendingLoad = false;
        pendingNewGame = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(MainSceneName);
    }

    public void AutoSave()
    {
        if (isApplyingLoad || SceneManager.GetActiveScene().name != GameSceneName)
        {
            return;
        }

        SaveModel saveModel = CaptureCurrentGame();
        if (saveModel == null)
        {
            return;
        }

        WriteSave(saveModel);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != GameSceneName)
        {
            return;
        }

        StartCoroutine(HandleGameSceneLoaded());
    }

    private IEnumerator HandleGameSceneLoaded()
    {
        yield return null;

        while (!AreManagersReady())
        {
            yield return null;
        }

        Dictionary<string, Item> itemCatalog = BuildItemCatalog();
        ResetAllItemRuntimeStates(itemCatalog);

        if (pendingLoad)
        {
            if (TryReadSave(out SaveModel saveModel))
            {
                isApplyingLoad = true;
                ApplySaveToScene(saveModel, itemCatalog);
                isApplyingLoad = false;
            }

            pendingLoad = false;
            yield break;
        }

        if (pendingNewGame)
        {
            AutoSave();
            pendingNewGame = false;
        }
    }

    private bool AreManagersReady()
    {
        return InGameManager.Instance != null &&
               CardinalManager.Instance != null &&
               InventoryManager.Instance != null &&
               PlotManager.Instance != null;
    }

    private SaveModel CaptureCurrentGame()
    {
        if (InGameManager.Instance == null)
        {
            return null;
        }

        SaveModel saveModel = new SaveModel
        {
            savedAtUtc = DateTime.UtcNow.ToString("o"),
            sceneName = SceneManager.GetActiveScene().name,
            gameContext = InGameManager.Instance.CaptureSaveData(),
            cardinals = CardinalManager.Instance != null ? CardinalManager.Instance.CaptureSaveData() : new List<CardinalSaveData>(),
            inventory = InventoryManager.Instance != null ? InventoryManager.Instance.CaptureSaveData() : new InventorySaveData(),
            events = InGameManager.Instance.EventManager != null ? InGameManager.Instance.EventManager.CaptureSaveData() : new EventManagerSaveData(),
            plots = PlotManager.Instance != null ? PlotManager.Instance.CaptureSaveData() : new PlotManagerSaveData(),
            fieldItems = InGameManager.Instance.CaptureFieldItemSaveData()
        };

        return saveModel;
    }

    private void ApplySaveToScene(SaveModel saveModel, Dictionary<string, Item> itemCatalog)
    {
        if (saveModel == null || InGameManager.Instance == null)
        {
            return;
        }

        Time.timeScale = 1f;

        InGameManager.Instance.StopTimer();
        InGameManager.Instance.ClearFieldItems();
        InGameManager.Instance.RestoreGameContext(saveModel.gameContext);

        if (CardinalManager.Instance != null)
        {
            CardinalManager.Instance.RestoreFromSave(saveModel.cardinals);
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RestoreFromSave(saveModel.inventory, itemCatalog);
        }

        if (InGameManager.Instance.EventManager != null)
        {
            InGameManager.Instance.EventManager.RestoreFromSave(saveModel.events);
        }

        if (PlotManager.Instance != null)
        {
            PlotManager.Instance.RestoreFromSave(saveModel.plots);
        }

        InGameManager.Instance.RestoreFieldItems(saveModel.fieldItems);
        RefreshSceneUi(saveModel);
    }

    private void RefreshSceneUi(SaveModel saveModel)
    {
        TimeUI timeUI = FindAnyObjectByType<TimeUI>();
        if (timeUI != null)
        {
            timeUI.ResetUI();
        }

        bool hasActiveCardinals = false;
        if (saveModel != null && saveModel.cardinals != null)
        {
            foreach (var cardinal in saveModel.cardinals)
            {
                if (cardinal != null && cardinal.isActive)
                {
                    hasActiveCardinals = true;
                    break;
                }
            }
        }

        if (hasActiveCardinals && CardinalManager.Instance != null && CardinalManager.Instance.StatsUI != null)
        {
            CardinalManager.Instance.StatsUI.Initialize(CardinalManager.Instance.Cardinals);
        }
    }

    private Dictionary<string, Item> BuildItemCatalog()
    {
        Dictionary<string, Item> itemCatalog = new Dictionary<string, Item>();
        Item[] loadedItems = Resources.FindObjectsOfTypeAll<Item>();

        foreach (var item in loadedItems)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.itemID))
            {
                continue;
            }

            if (!itemCatalog.ContainsKey(item.itemID))
            {
                itemCatalog[item.itemID] = item;
            }
        }

        return itemCatalog;
    }

    private void ResetAllItemRuntimeStates(Dictionary<string, Item> itemCatalog)
    {
        if (itemCatalog == null)
        {
            return;
        }

        foreach (var item in itemCatalog.Values)
        {
            if (item == null)
            {
                continue;
            }

            item.ResetRuntimeState();
        }
    }

    private void WriteSave(SaveModel saveModel)
    {
        try
        {
            Directory.CreateDirectory(SaveDirectoryPath);
            string json = JsonUtility.ToJson(saveModel, true);
            File.WriteAllText(SaveFilePath, json);

            Debug.Log($"[Save] 저장 완료: {SaveFilePath}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[Save] 저장 실패: {exception}");
        }
    }

    private bool TryReadSave(out SaveModel saveModel)
    {
        saveModel = null;

        if (!HasSave())
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            saveModel = JsonUtility.FromJson<SaveModel>(json);

            if (saveModel == null)
            {
                Debug.LogWarning("[Save] 저장 파일을 읽었지만 복원 가능한 데이터가 없습니다.");
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[Save] 로드 실패: {exception}");
            return false;
        }
    }

    private void DeleteSave()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }

            Directory.CreateDirectory(SaveDirectoryPath);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[Save] 저장 파일 삭제 실패: {exception}");
        }
    }
}
