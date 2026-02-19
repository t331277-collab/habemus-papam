using UnityEditor.Search;
using UnityEngine;

public class P024 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minHp;
    [SerializeField] private int pietyCost;
    [SerializeField] private int statsDelta;


    void Reset()
    {
        // 설정 기본값
        plotID = "P024";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "가지치기";
        plotDescription = "줄건 줘";

        // 수치 기본값
        plotWeightBase = 15;
        plotWeightMultiplier = 0f;

        minHp = 40;
        pietyCost = 0;
        statsDelta = 15;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Hp >= minHp;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        float hp = performer.Hp;
        float pol = performer.Influence;
        float piety = performer.Piety;

        float minVal = Mathf.Min(hp, Mathf.Min(pol, piety));

        if (Mathf.Approximately(minVal, hp)) // 체력이 제일 적다면
        {
            performer.ChangeHp(-(hp / 2f)); 
            performer.ChangeInfluence(statsDelta);  
            performer.ChangePiety(statsDelta);
        }
        else if (Mathf.Approximately(minVal, pol)) // 정치력이 제일 적다면
        {
            performer.ChangeInfluence(-(pol / 2f));
            performer.ChangeHp(statsDelta);
            performer.ChangePiety(statsDelta);
        }
        else // 경건함이 제일 적다면
        {
            performer.ChangePiety(-(piety / 2f));
            performer.ChangeHp(statsDelta);
            performer.ChangeInfluence(statsDelta);
        }
    }

}
