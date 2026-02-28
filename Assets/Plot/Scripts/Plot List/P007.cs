using UnityEditor.Search;
using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "P007", menuName = "Plot/드랍 더 비트")]

public class P007 : Plot
{
    [Header("해당 공작 설정")]
    [SerializeField] private int minInfluence;
    [SerializeField] private int pietyCost;
    [SerializeField] private float speedPercentDelta;
    [SerializeField] private int duration;

    public override int cost => pietyCost;

    void Reset()
    {
        // 설정 기본값
        plotID = "P007";
        plotGrade = PlotGrade.Common;
        
        // 텍스트 기본값
        plotName = "드랍 더 비트";
        plotDescription = "새긴다! 태양의 비트!";

        // 수치 기본값
        plotWeightBase = 15;
        plotWeightMultiplier = 0f;

        minInfluence = 20;
        pietyCost = 15;
        speedPercentDelta = 10f;
        duration = 20;
    }

    public override bool CanExecute(Cardinal performer)
    {
        return performer.Influence >= minInfluence;
    }

    public override void Execute(Cardinal performer)
    {
        if (!CanExecute(performer)) return;

        performer.ChangePiety(-pietyCost);

        var cm = CardinalManager.Instance;

        performer.StartCoroutine(SpeedBoostRoutine(performer));

        for (int i = 0; i < 3; i++)
        {
            var target = cm.Cardinals[i];
            target.StartCoroutine(SpeedBoostRoutine(target));
        }
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
