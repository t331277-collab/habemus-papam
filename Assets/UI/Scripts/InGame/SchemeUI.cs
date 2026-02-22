using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SchemeUI : MonoBehaviour
{
    [Header("--- 공작 선택 UI ---")]
    public GameObject plotSelectUI;
    public Image[] plotPanels = new Image[3];
    public Button[] plotButtons = new Button[3];

    public void ShowPlotUI(int npcID)
    {

        // SetPlotUI(npcID); UI 정보 업데이트 함수

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

    public void SetPlotUI(int npcID)
    {
        ResetPlotUI();

        /*
        int count = gameManager.GetAvailPlotCount(npcID);

        for (int i = 0; i < count; i++)
        {
            plotNames[i].text = gameManager.GetPlotName(npcID, i);     // 공작 이름 전달
            plotDescs[i].text = gameManager.GetPlotDesc(npcID, i);     // 공작 설명 전달
            plotEffect[i].text = gameManager.GetPlotEffectText(npcID, i);     // 공작 효과 및 비용 전달
            if (gameManager.CheckPlotUsed(npcID, i))
            {
                Debug.Log("사용됨");
                plotPanels[i].color = new Color(0.8f, 0.8f, 0.8f);
                plotButtons[i].interactable = false;
            }
            else
            {
                Debug.Log("사용안됨");
                plotPanels[i].color = new Color(1f, 1f, 1f);
                plotButtons[i].interactable = gameManager.CheckCanSelect(npcID, i);
            }
        }*/
    }

    // 공작 UI 정보 리셋 함수
    public void ResetPlotUI()
    {
        /* 공작 UI 정보 리셋
        for (int i = 0; i < 3; i++)
        {
            plotNames[i].text = "";
            plotDescs[i].text = "";
            plotEffect[i].text = "";
            plotButtons[i].interactable = false;
        }*/
    }
    }