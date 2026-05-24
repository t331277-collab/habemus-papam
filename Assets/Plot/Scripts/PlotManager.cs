using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlotSet
{
    public Plot[] plots = new Plot[3];
    public bool[] isUsed = new bool[3];

    public PlotSet(Plot[] plots)
    {
        for(int i = 0; i < 3; i++)
        {
            this.plots[i] = plots[i];
            this.isUsed[i] = false;
        }
    }

    public void use(int slot)
    {
        isUsed[slot] = true;
    }

    public bool isAllUsed()
    {
        return isUsed[0] && isUsed[1] && isUsed[2];
    }
}


public class PlotManager : MonoBehaviour
{
    public static PlotManager Instance { get; private set; }

    [SerializeField] private PlotUI plotUI;

    [Header("공작 SO 리스트")]
    [SerializeField] private List<Plot> plots;

    private PlotSet[] availPlotSets = new PlotSet[2];

    private List<Plot> usedPlots;

    private Cardinal performer;

    public PlotSet[] AvailPlotSets => availPlotSets;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        usedPlots = new List<Plot>();
    }
    void Start()
    {
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent += OnGameContextChanged;
        }
    }

    void OnDestroy()
    {
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent -= OnGameContextChanged;
        }
    }

    public PlotSet GeneratePlotSet()
    {
        float p = InGameManager.Instance.GetProgress(); // 진행도 (0~100 가정)
        Plot[] selectedPlots = new Plot[3];

        // 1. 가운데 슬롯 (index 1) : 희귀 / 전설
        float midRoll = Random.Range(0f, 100f);
        float midLegendaryProb = 10f + (p * 0.3f);
        
        PlotGrade midGrade = (midRoll < midLegendaryProb) ? PlotGrade.Legendary : PlotGrade.Rare;
        selectedPlots[1] = GetWeightedRandPlot(midGrade);

        // 2. 양 옆 슬롯 (index 0, 2) : 일반 / 희귀 / 전설
        float commonLimit = 70f - (p * 0.5f);
        float rareLimit = commonLimit + (20f + (p * 0.4f));

        for (int i = 0; i < 3; i++)
        {
            if (i == 1) continue; // 가운데는 이미 뽑음

            float sideRoll = Random.Range(0f, 100f);
            PlotGrade sideGrade;

            if (sideRoll < commonLimit) sideGrade = PlotGrade.Common;
            else if (sideRoll < rareLimit) sideGrade = PlotGrade.Rare;
            else sideGrade = PlotGrade.Legendary;

            selectedPlots[i] = GetWeightedRandPlot(sideGrade);
        }

        return new PlotSet(selectedPlots);
    }

    public void RefreshPlotManager()
    {
        usedPlots.Clear();
    }

    private Plot GetWeightedRandPlot(PlotGrade grade)
    {
        var candidates = plots.Where(p => p.plotGrade == grade && !usedPlots.Contains(p)).ToList();

        float weightSum = candidates.Sum(p => p.GetPlotWeight());
        
        float randChoice = Random.Range(0f, weightSum);
        float currentSum = 0f;

        foreach(var p in candidates)
        {
            currentSum += p.GetPlotWeight();
            if (currentSum >= randChoice)
            {
                return p;
            }
        }

        return candidates[0];
    }

    // 콘클라베 시작 시 새로운 공작 Set 생성
    private void OnGameContextChanged(GameContext.GameContextEvent eventType)
    {
        if (eventType == GameContext.GameContextEvent.ConclaveStart)
        {
            availPlotSets[0] = GeneratePlotSet();
            availPlotSets[1] = GeneratePlotSet();
        }
    }

    public void InitializePlotSession(Cardinal performer)
    {
        this.performer = performer;

        plotUI.ShowPlotUI(performer);
    }

    public void UsePlot(int plotSet, int index)
    {
        AvailPlotSets[plotSet].plots[index].Execute(performer);
        performer?.OnPlotExecuted();
        AvailPlotSets[plotSet].use(index);

        if (ActionRecordManager.Instance != null)
        {
            ActionRecordManager.Instance.RecordPlot(performer);
        }

        CheckIsAllUsed(plotSet);
    }

    public void CheckIsAllUsed(int plotSet = 0)
    {
        if (AvailPlotSets[plotSet].isAllUsed())
        {
            RerollPlotSet(plotSet);
        }
    }

    public void RerollPlotSet(int plotSet = 0)
    {
        availPlotSets[plotSet] = GeneratePlotSet();
    }

    public Plot GetPlotById(string plotId)
    {
        if (string.IsNullOrWhiteSpace(plotId))
        {
            return null;
        }

        return plots.Find(plot => plot != null && plot.plotID == plotId);
    }

    public PlotManagerSaveData CaptureSaveData()
    {
        PlotManagerSaveData saveData = new PlotManagerSaveData();

        for (int i = 0; i < availPlotSets.Length; i++)
        {
            PlotSetSaveData setSave = new PlotSetSaveData();
            PlotSet currentSet = availPlotSets[i];

            for (int slot = 0; slot < 3; slot++)
            {
                string plotId = string.Empty;
                bool used = false;

                if (currentSet != null)
                {
                    Plot currentPlot = currentSet.plots[slot];
                    plotId = currentPlot != null ? currentPlot.plotID : string.Empty;
                    used = currentSet.isUsed[slot];
                }

                setSave.plotIds.Add(plotId);
                setSave.usedSlots.Add(used);
            }

            saveData.plotSets.Add(setSave);
        }

        return saveData;
    }

    public void RestoreFromSave(PlotManagerSaveData saveData)
    {
        usedPlots.Clear();
        availPlotSets = new PlotSet[2];

        if (saveData == null || saveData.plotSets == null)
        {
            return;
        }

        for (int i = 0; i < availPlotSets.Length && i < saveData.plotSets.Count; i++)
        {
            PlotSetSaveData setSave = saveData.plotSets[i];
            if (setSave == null || setSave.plotIds == null || setSave.plotIds.Count < 3)
            {
                continue;
            }

            Plot[] restoredPlots = new Plot[3];
            bool canRestore = true;

            for (int slot = 0; slot < 3; slot++)
            {
                restoredPlots[slot] = GetPlotById(setSave.plotIds[slot]);
                if (restoredPlots[slot] == null)
                {
                    canRestore = false;
                    break;
                }
            }

            if (!canRestore)
            {
                Debug.LogWarning($"[Save] {i}번 공작 세트를 완전히 복원하지 못했습니다.");
                continue;
            }

            availPlotSets[i] = new PlotSet(restoredPlots);

            if (setSave.usedSlots != null)
            {
                for (int slot = 0; slot < 3 && slot < setSave.usedSlots.Count; slot++)
                {
                    if (setSave.usedSlots[slot])
                    {
                        availPlotSets[i].use(slot);
                    }
                }
            }
        }
    }
}
