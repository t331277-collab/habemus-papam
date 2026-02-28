using UnityEditor.Search;
using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "P022", menuName = "Plot/꼬리 자르기")]

public class P022 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int hpCost;
    [SerializeField] private float speedPercentDelta;
    [SerializeField] private int duration;

    public override int cost => hpCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P022";
        plotGrade = PlotGrade.Rare;
        
        // 텍스트 기본값
        plotName = "꼬리 자르기";
        plotDescription = "쌀을 내주고 벼를 취한다";

        // 수치 기본값
        plotWeightBase = 10;
        plotWeightMultiplier = 0.1f;

        minInfluence = 50;
        hpCost = 30;
        speedPercentDelta = 30f;
        duration = 30;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence >= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangeHp(-hpCost);

        performer.StartCoroutine(SpeedBoostRoutine(performer));
    }

    private IEnumerator SpeedBoostRoutine(Cardinal target)
    {
        if (target == null) yield break;

        float delta = target.MoveSpeed * (speedPercentDelta / 100f);

        target.ChangeSpeed(delta);

        yield return new WaitForSeconds(duration);

        if (target != null)
        {
            target.ChangeSpeed(-delta);
        }
    }

}
