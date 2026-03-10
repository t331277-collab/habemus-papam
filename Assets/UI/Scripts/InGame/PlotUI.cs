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
    public TextMeshProUGUI[] plotGradeList = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] plotNameList = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] plotDescList = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] plotEffectList = new TextMeshProUGUI[3];

    [Header("--- 테스트용 공작 ---")]
    public Plot testPlot;

    private Cardinal performer;

    void Start()
    {
        plotSelectUI.SetActive(false);

        ResetPlotUI();
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
            switch (pm.AvailPlotSets[0].plots[i].plotGrade)
            {
                case PlotGrade.Common:
                    plotGradeList[i].text = "일반";
                    break;

                case PlotGrade.Rare:
                    plotGradeList[i].text = "고급";
                    break;

                case PlotGrade.Legendary:
                    plotGradeList[i].text = "TOP SECRET";
                    break;

                default:
                    // 혹시 모를 예외 처리
                    break;
            }
            plotNameList[i].text = pm.AvailPlotSets[0].plots[i].plotName;
            plotDescList[i].text = pm.AvailPlotSets[0].plots[i].plotDescription;
            plotEffectList[i].text = pm.AvailPlotSets[0].plots[i].plotEffect;
            plotUseButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = $"경건함 {pm.AvailPlotSets[0].plots[i].cost}";

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
                plotUseButtons[i].interactable = pm.AvailPlotSets[0].plots[i].CanExecute(performer);
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