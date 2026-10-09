using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
/*
오브젝트 : 상단 UI
하위 오브젝트
- 좌측 텍스트 : 콘클라베 일자와 현재 턴의 시간(1턴 새벽/2턴 아침/3턴 오후/4턴 저녁)을 표시
- 우측 텍스트 : 남은 시간을 0:00.00으로 표시
- 시계 : 각 턴의 시작 시각에서 해당 턴의 행동 완료마다 1시간씩 시침이 이동
- 표시등 4개 : '꺼짐' 상태로 있다가 콘클라베가 시작되면 시간에 맞도록 켜짐
*/
public class TimeUI : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] TextMeshProUGUI LeftText1;
    [SerializeField] TextMeshProUGUI LeftText2;
    [SerializeField] TextMeshProUGUI RightText1;
    [SerializeField] TextMeshProUGUI RightText2;
    [Space(10f)]
    [Header("오브젝트")]
    [SerializeField] RectTransform ClockHand;
    [SerializeField] Image Dawn;
    [SerializeField] Image Morning;
    [SerializeField] Image Afternoon;
    [SerializeField] Image Evening;
    [Space(10f)]
    [Header("이미지")]
    [SerializeField] List<Sprite> LightList;

    [Header("턴 시작 안내")]
    [Tooltip("나타났다 사라질 TurnIndicator 오브젝트. CanvasGroup이 없으면 자동으로 추가합니다.")]
    [SerializeField] private GameObject turnIndicator;
    [Tooltip("현재 시간대를 표시할 TMP 텍스트. 비워두면 TurnIndicator 내부에서 찾습니다.")]
    [SerializeField] private TMP_Text turnIndicatorText;
    [SerializeField, Min(0f)] private float turnFadeInDuration = 0.6f;
    [SerializeField, Min(0f)] private float turnHoldDuration = 0.6f;
    [SerializeField, Min(0f)] private float turnFadeOutDuration = 0.6f;

    [Header("시간대 색상 전환 대상")]
    [Tooltip("색상을 바꿀 배경 오브젝트. Graphic, SpriteRenderer, Light2D, Light 또는 Camera를 지원합니다.")]
    [SerializeField] private GameObject backgroundObject;
    [Tooltip("Spot Light 2D 오브젝트의 URP Light2D 컴포넌트를 연결합니다. Light Type이 Sprite여도 지원합니다.")]
    [SerializeField] private Light2D spotlightLight2D;
    // Keep earlier GameObject assignments available while upgrading to the typed Light2D reference.
    [SerializeField, HideInInspector] private GameObject spotlightObject;

    [Serializable]
    private struct TurnTimeColors
    {
        public Color background;
        public Color spotlight;
    }

    [Header("시간대별 색상 (안내 연출 전체 시간에 걸쳐 전환)")]
    [SerializeField] private TurnTimeColors dawnColors = new TurnTimeColors { background = Color.white, spotlight = Color.white };
    [SerializeField] private TurnTimeColors morningColors = new TurnTimeColors { background = Color.white, spotlight = Color.white };
    [SerializeField] private TurnTimeColors afternoonColors = new TurnTimeColors { background = Color.white, spotlight = Color.white };
    [SerializeField] private TurnTimeColors eveningColors = new TurnTimeColors { background = Color.white, spotlight = Color.white };

    private CanvasGroup canvasGroup;
    private Coroutine alphaCoroutine;
    private GameContext.Conclave? displayedConclave;
    private CanvasGroup turnIndicatorGroup;
    private Coroutine turnPresentationCoroutine;
    private int presentedDay = -1;
    private int presentedPosition = -1;

    public bool IsTurnPresentationRunning => isActiveAndEnabled && turnPresentationCoroutine != null;

    public void EnsureTurnPresentation(GameContext context)
    {
        TryPresentTurn(context);
    }

    private void Awake()
    {
        ValidateClockHandRules();
        PrepareTurnIndicator();
        HideForConclaveEntrance();
        ApplyCurrentTimeColorsImmediately();
    }

    private void OnEnable()
    {
        ApplyCurrentTimeColorsImmediately();
    }

    private void OnValidate()
    {
        if (spotlightLight2D == null && spotlightObject != null)
            spotlightLight2D = spotlightObject.GetComponent<Light2D>();
    }

    void Start()
    {
        // All manager Awake calls have completed, even if TimeUI was initialized first.
        if (!IsTurnPresentationRunning) ApplyCurrentTimeColorsImmediately();
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent += HandleGameContextEvent;
        }
    }
    void Update()
    {

        if (InGameManager.Instance == null) return;

        if (InGameManager.Instance.IsTimeRunning)
        {
            GameContext context = InGameManager.Instance.Context;
            RefreshTimeOfDay(context);
            TryPresentTurn(context);
            RightText2.text = FormatActionProgress(context);
            UpdateClockHand(context);
        }
    }

    void OnDestroy()
    {
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent -= HandleGameContextEvent;
        }
    }

    private void OnDisable()
    {
        StopTurnPresentation();
    }

    private void HandleGameContextEvent(GameContext.GameContextEvent evt)
    {
        switch (evt)
        {
            case GameContext.GameContextEvent.ConclaveStart:
            case GameContext.GameContextEvent.TurnStart:
            case GameContext.GameContextEvent.ActionPositionStart:
                ResetUI();
                break;
            case GameContext.GameContextEvent.ConclaveEnd:
                EndConclaveUI();
                break;
        }
    }

    public void ResetUI()
    {
        GameContext context = InGameManager.Instance.Context;

        LeftText1.text = $"Day {context.CurrentDay}";
        RightText1.text = "Action";
        RightText2.text = FormatActionProgress(context);

        RefreshTimeOfDay(context, true);
        UpdateClockHand(context);
        TryPresentTurn(context);
    }

    public void EndConclaveUI()
    {
        StopTurnPresentation();
        if (InGameManager.Instance != null)
        {
            GameContext context = InGameManager.Instance.Context;
            RefreshTimeOfDay(context);
            RightText2.text = FormatActionProgress(context);
            UpdateClockHand(context);
        }

        Dawn.sprite = LightList[0];
        Morning.sprite = LightList[0];
        Afternoon.sprite = LightList[0];
        Evening.sprite = LightList[0];
        displayedConclave = null;
    }

    public void HideForConclaveEntrance()
    {
        StopTurnPresentation();
        EnsureCanvasGroup();
        if (alphaCoroutine != null)
        {
            StopCoroutine(alphaCoroutine);
            alphaCoroutine = null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void FadeInAfterConclaveEntrance(float duration = 1f)
    {
        EnsureCanvasGroup();
        if (alphaCoroutine != null)
        {
            StopCoroutine(alphaCoroutine);
        }

        alphaCoroutine = StartCoroutine(FadeCanvasAlpha(1f, duration));
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private IEnumerator FadeCanvasAlpha(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = targetAlpha > 0f;
        canvasGroup.blocksRaycasts = targetAlpha > 0f;
        alphaCoroutine = null;
    }

    
    private void RefreshTimeOfDay(GameContext context, bool force = false)
    {
        GameContext.Conclave currentCon = GetTurnConclave(context);
        if (!force && displayedConclave == currentCon) return;

        displayedConclave = currentCon;
        LeftText2.text = currentCon.ToString();
        UpdateLights(currentCon);
    }

    private static GameContext.Conclave GetTurnConclave(GameContext context)
    {
        // The four action positions are the turns within a single conclave.
        return context.CurrentTimeOfDay;
    }

    public void ApplyCurrentTimeColorsImmediately()
    {
        if (InGameManager.Instance == null || InGameManager.Instance.Context == null) return;

        StopTurnPresentation();
        TurnTimeColors colors = GetTurnTimeColors(GetTurnConclave(InGameManager.Instance.Context));
        SetTargetColor(ResolveColorTarget(backgroundObject), colors.background);
        Component spotlight = spotlightLight2D != null ? spotlightLight2D : ResolveColorTarget(spotlightObject);
        SetTargetColor(spotlight, colors.spotlight);
    }

    private void TryPresentTurn(GameContext context)
    {
        if (!isActiveAndEnabled || InGameManager.Instance == null || !InGameManager.Instance.IsTimeRunning ||
            context.AreActionsComplete()) return;
        if (presentedDay == context.CurrentDay && presentedPosition == context.CurrentActionPositionIndex) return;

        presentedDay = context.CurrentDay;
        presentedPosition = context.CurrentActionPositionIndex;
        if (turnPresentationCoroutine != null)
        {
            StopCoroutine(turnPresentationCoroutine);
            turnPresentationCoroutine = null;
        }

        GameContext.Conclave timeOfDay = GetTurnConclave(context);
        PrepareTurnIndicator();
        if (turnIndicatorText != null) turnIndicatorText.text = timeOfDay.ToString();
        if (turnIndicator != null) turnIndicator.SetActive(true);

        Component background = ResolveColorTarget(backgroundObject);
        Component spotlight = spotlightLight2D != null ? spotlightLight2D : ResolveColorTarget(spotlightObject);
        if (turnIndicator == null && background == null && spotlight == null) return;
        TurnTimeColors colors = GetTurnTimeColors(timeOfDay);
        float fadeIn = Mathf.Max(0f, turnFadeInDuration);
        float hold = Mathf.Max(0f, turnHoldDuration);
        float fadeOut = Mathf.Max(0f, turnFadeOutDuration);
        if (fadeIn + hold + fadeOut <= 0f)
        {
            SetTargetColor(background, colors.background);
            SetTargetColor(spotlight, colors.spotlight);
            return;
        }

        turnPresentationCoroutine = StartCoroutine(AnimateTurnPresentation(
            background, spotlight, colors, fadeIn, hold, fadeOut));
    }

    private void PrepareTurnIndicator()
    {
        if (turnIndicator == null) return;
        turnIndicatorGroup = turnIndicator.GetComponent<CanvasGroup>();
        if (turnIndicatorGroup == null) turnIndicatorGroup = turnIndicator.AddComponent<CanvasGroup>();
        turnIndicatorGroup.alpha = 0f;
        turnIndicatorGroup.interactable = false;
        turnIndicatorGroup.blocksRaycasts = false;
        if (turnIndicatorText == null) turnIndicatorText = turnIndicator.GetComponentInChildren<TMP_Text>(true);
    }

    private void StopTurnPresentation()
    {
        if (turnPresentationCoroutine != null)
        {
            StopCoroutine(turnPresentationCoroutine);
            turnPresentationCoroutine = null;
        }
        if (turnIndicatorGroup != null) turnIndicatorGroup.alpha = 0f;
        presentedDay = -1;
        presentedPosition = -1;
    }

    private IEnumerator AnimateTurnPresentation(Component background, Component spotlight,
        TurnTimeColors colors, float fadeIn, float hold, float fadeOut)
    {
        Color initialBackground = GetTargetColor(background);
        Color initialSpotlight = GetTargetColor(spotlight);
        float duration = fadeIn + hold + fadeOut;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (turnIndicatorGroup != null)
                turnIndicatorGroup.alpha = GetTurnIndicatorAlpha(elapsed, fadeIn, hold, fadeOut);
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            SetTargetColor(background, Color.Lerp(initialBackground, colors.background, progress));
            SetTargetColor(spotlight, Color.Lerp(initialSpotlight, colors.spotlight, progress));
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        SetTargetColor(background, colors.background);
        SetTargetColor(spotlight, colors.spotlight);
        if (turnIndicatorGroup != null) turnIndicatorGroup.alpha = 0f;
        turnPresentationCoroutine = null;
    }

    private static float GetTurnIndicatorAlpha(float elapsed, float fadeIn, float hold, float fadeOut)
    {
        if (fadeIn > 0f && elapsed < fadeIn) return Mathf.Clamp01(elapsed / fadeIn);
        if (elapsed < fadeIn + hold) return 1f;
        return fadeOut > 0f ? Mathf.Clamp01(1f - (elapsed - fadeIn - hold) / fadeOut) : 0f;
    }

    private TurnTimeColors GetTurnTimeColors(GameContext.Conclave timeOfDay)
    {
        return timeOfDay switch
        {
            GameContext.Conclave.Morning => morningColors,
            GameContext.Conclave.Afternoon => afternoonColors,
            GameContext.Conclave.Evening => eveningColors,
            _ => dawnColors
        };
    }

    private static Component ResolveColorTarget(GameObject target)
    {
        if (target == null) return null;
        Component component = target.GetComponent<Graphic>();
        if (component == null) component = target.GetComponent<SpriteRenderer>();
        if (component == null) component = target.GetComponent<Light2D>();
        if (component == null) component = target.GetComponent<Light>();
        if (component == null) component = target.GetComponent<Camera>();
        return component;
    }

    private static Color GetTargetColor(Component target)
    {
        if (target == null) return Color.white;
        return target switch
        {
            Graphic graphic => graphic.color,
            SpriteRenderer sprite => sprite.color,
            Light2D light2D => light2D.color,
            Light light => light.color,
            Camera camera => camera.backgroundColor,
            _ => Color.white
        };
    }

    private static void SetTargetColor(Component target, Color color)
    {
        if (target == null) return;
        switch (target)
        {
            case Graphic graphic: graphic.color = color; break;
            case SpriteRenderer sprite: sprite.color = color; break;
            case Light2D light2D: light2D.color = color; break;
            case Light light: light.color = color; break;
            case Camera camera: camera.backgroundColor = color; break;
        }
    }

    private void UpdateLights(GameContext.Conclave currentCon)
    {
        Dawn.sprite = LightList[0];
        Morning.sprite = LightList[0];
        Afternoon.sprite = LightList[0];
        Evening.sprite = LightList[0];

        switch (currentCon)
        {
            case GameContext.Conclave.Dawn:
                Dawn.sprite = LightList[1];
                break;
            case GameContext.Conclave.Morning:
                Dawn.sprite = LightList[1];
                Morning.sprite = LightList[2];
                break;
            case GameContext.Conclave.Afternoon:
                Dawn.sprite = LightList[1];
                Morning.sprite = LightList[2];
                Afternoon.sprite = LightList[3];
                break;
            case GameContext.Conclave.Evening:
                Dawn.sprite = LightList[1];
                Morning.sprite = LightList[2];
                Afternoon.sprite = LightList[3];
                Evening.sprite = LightList[4];
                break;
        }
    }

    private static string FormatActionProgress(GameContext context)
    {
        int total = context.CurrentPositionActionCount;
        int current = context.CurrentActionNumber;
        return $"{current} / {total}";
    }

    private void UpdateClockHand(GameContext context)
    {
        if (ClockHand == null || context == null) return;
        int hour = GetTurnClockHour(context);
        ClockHand.rotation = Quaternion.Euler(0f, 0f, -30f * (hour % 12));
    }

    private static int GetTurnClockHour(GameContext context)
    {
        // Moving past the final position clears its counter; retain its finished hour.
        int completedActions = context.AreActionsComplete()
            ? context.CurrentPositionActionCount
            : context.CompletedActionsInPosition;
        return GetClockHour(GetTurnConclave(context), completedActions);
    }

    private static int GetClockHour(GameContext.Conclave conclave, int completedActions)
    {
        (int startHour, int maxAdvance) = conclave switch
        {
            GameContext.Conclave.Dawn => (4, 4),
            GameContext.Conclave.Morning => (9, 3),
            GameContext.Conclave.Afternoon => (1, 5),
            GameContext.Conclave.Evening => (7, 8),
            _ => (12, 0)
        };
        int hour = startHour + Mathf.Clamp(completedActions, 0, maxAdvance);
        return (hour - 1) % 12 + 1;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void ValidateClockHandRules()
    {
        Debug.Assert(
            GetClockHour(GameContext.Conclave.Dawn, 0) == 4 &&
            GetClockHour(GameContext.Conclave.Dawn, 99) == 8 &&
            GetClockHour(GameContext.Conclave.Morning, 0) == 9 &&
            GetClockHour(GameContext.Conclave.Morning, 99) == 12 &&
            GetClockHour(GameContext.Conclave.Afternoon, 0) == 1 &&
            GetClockHour(GameContext.Conclave.Afternoon, 99) == 6 &&
            GetClockHour(GameContext.Conclave.Evening, 0) == 7 &&
            GetClockHour(GameContext.Conclave.Evening, 99) == 3,
            "시간대별 시침 시작/종료 규칙이 손상됐습니다.");
    }
}
