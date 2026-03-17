using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlotUI : MonoBehaviour
{
    [Header("--- 공작 선택 UI ---")]
    public GameObject plotSelectUI;
    public Image[] plotPanels = new Image[3];
    public Button[] plotUseButtons = new Button[3];

    [Header("--- 공작 정보 텍스트 ---")]
    public TextMeshProUGUI[] plotNameList = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] plotDescList = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] plotEffectList = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] plotCondiList = new TextMeshProUGUI[3];

    [Header("등급별 스프라이트 설정")]
    [SerializeField] private Sprite commonSprite;
    [SerializeField] private Sprite rareSprite;
    [SerializeField] private Sprite legendSprite;



    [Header("--- 테스트용 공작 ---")]
    public Plot testPlot;

    private Cardinal performer;

    void Start()
    {
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent += OnGameContextChanged;
        }

        plotSelectUI.SetActive(false);

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            plotUseButtons[index].onClick.AddListener(() => OnSelectPlot(index));
        }

        ResetPlotUI();
    }

    void OnDestroy()
    {
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent -= OnGameContextChanged;
        }
    }

    private void OnGameContextChanged(GameContext.GameContextEvent eventType)
    {
        if (eventType == GameContext.GameContextEvent.ConclaveEnd)
        {
            OnClickClose();
        }
    }


    public void ShowPlotUI(Cardinal performer)
    {
        this.performer = performer;

        SetPlotUI();

        plotSelectUI.SetActive(true);

        /* 다른 상호작용 버튼 비활성화
        foreach (var item in actionButtons)
        {
            item.interactable = false;
        }*/
    }

    public void OnClickClose()
    {
        plotSelectUI.SetActive(false);

        /* 다른 상호작용 버튼 활성화
        foreach (var item in actionButtons)
        {
            item.interactable = true;
        }*/
    }

    public void SetPlotUI()
    {
        ResetPlotUI();

        var pm = PlotManager.Instance;

        for (int i = 0; i < 3; i++)
        {
            var currentPlot = pm.AvailPlotSets[0].plots[i];
            var buttonText = plotUseButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            // 공작 등급에 따른 뒷 배경 세팅
            switch (currentPlot.plotGrade)
            {
                case PlotGrade.Common:
                    plotPanels[i].sprite = commonSprite;
                    break;

                case PlotGrade.Rare:
                    plotPanels[i].sprite = rareSprite;
                    break;

                case PlotGrade.Legendary:
                    plotPanels[i].sprite = legendSprite;
                    break;

                default:
                    // 혹시 모를 예외 처리
                    break;
            }

            // 공작 텍스트 정보 세팅
            plotNameList[i].text = currentPlot.plotName;
            plotDescList[i].text = currentPlot.plotDescription;
            plotCondiList[i].text = currentPlot.plotCondiText;
            plotEffectList[i].text = currentPlot.plotEffect;
            buttonText.text = currentPlot.plotCostText;

            if (pm.AvailPlotSets[0].isUsed[i])
            {
                Debug.Log($"{i}번 공작 사용됨");
                plotPanels[i].color = new Color(0.8f, 0.8f, 0.8f);
                plotUseButtons[i].interactable = false;
            }
            else
            {
                Debug.Log($"{i}번 공작사용안됨");
                plotPanels[i].color = new Color(1f, 1f, 1f);

                // 조건 확인
                bool isPietyEnough = performer.Piety >= currentPlot.cost;
                bool canExecute = currentPlot.CanExecute(performer);

                // 버튼 활성화 여부 설정
                plotUseButtons[i].interactable = isPietyEnough && canExecute;

                if (!isPietyEnough)
                {
                    buttonText.text += "<br><color=red><size=60%>경건함이 부족합니다</size></color>";
                }
            }
        }
    }
    
    // 공작 UI 정보 리셋 함수
    public void ResetPlotUI()
    {
        for (int i = 0; i < 3; i++)
        {
            plotNameList[i].text = "";
            plotDescList[i].text = "";
            plotEffectList[i].text = "";
            plotUseButtons[i].interactable = false;
        }
    }

    public void OnSelectPlot(int index)
    {
        var pm = PlotManager.Instance;

        pm.AvailPlotSets[0].plots[index].Execute(performer);
        pm.AvailPlotSets[0].use(index);

        Debug.Log($"{index}번째 공작 사용");

        if (pm.AvailPlotSets[0].isAllUsed())
        {
            pm.IfUseAllPlot();
        }

        OnClickClose();
    }

    public void PlotTest()
    {
        testPlot.Execute(performer);
    }
}