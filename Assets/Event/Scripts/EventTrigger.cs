using UnityEngine;

namespace Habemus.Events
{
public enum EventTier
{
    GuaranteedChain,
    GuaranteedStory,
    Story,
    ActionReward,
    Sub
}

public readonly struct EventTriggerContext
{
    // Uses the same one-based day number as the in-game Day display.
    public int Day { get; }
    public GameContext.Conclave Conclave { get; }
    public int Turn { get; }
    public int ActionPosition { get; }

    public EventTriggerContext(int day, GameContext.Conclave conclave, int turn, int actionPosition)
    {
        Day = Mathf.Max(1, day);
        Conclave = conclave;
        Turn = Mathf.Clamp(turn, 1, 4);
        ActionPosition = Mathf.Max(1, actionPosition);
    }
}

public sealed class EventTrigger
{
    public string EventId { get; }
    public EventTier Tier { get; }

    public EventTrigger(string eventId)
    {
        EventId = eventId;
        Tier = GetTier(eventId);
    }

    public bool MatchesPosition(EventTriggerContext context)
    {
        int day = context.Day;
        int action = context.ActionPosition;

        switch (EventId)
        {
            case "E11100": return IsPosition(day, action, 1, 1);
            case "E12300":
            case "E12100":
            case "E12200": return day == 1 && action >= 2 && action <= 4;

            case "E20000":
                return action == 1 && (day == 2 || day == 4 || day == 6);
            case "E21000":
                return action == 1 && day >= 3 && day <= 11 && day % 2 == 1;
            case "E21100": return action == 1 && day >= 4 && day <= 12 && day % 2 == 0;
            case "E21101": return day >= 5;

            case "E30000": return IsPosition(day, action, 2, 2);
            case "E31100":
            case "E31200": return action == 2 || action == 4;
            case "E31101": return IsPosition(day, action, 13, 1);
            case "E31210": return IsPosition(day, action, 14, 2);

            case "E31212": return action == 3 && (day == 3 || day == 4);
            case "E31213": return IsPosition(day, action, 6, 2);
            case "E32000":
                return day == 7 && action >= 1 && action <= 3;
            case "E60000": return true;

            case "E11200":
            case "E11300":
            case "E31000":
            case "E31211":
            case "E32001":
            case "E32002": return false;
            default: return IsSubEventId(EventId);
        }
    }

    public bool IsEligible(EventTriggerContext context, EventManager manager)
    {
        if (manager == null || !manager.HasRemaining(EventId) || !MatchesPosition(context)) return false;

        switch (EventId)
        {
            case "E20000":
            case "E21000": return !manager.HasChosenOption1(EventId);
            case "E21100": return manager.HasAppeared("E20000") && !manager.HasChosenOption1(EventId);
            case "E31100":
            case "E31200": return manager.IsCandidateBranchDue(EventId, context);
            case "E31101": return manager.WasChoice("E31100", 1);
            case "E31210": return manager.WasChoice("E31200", 2, true);
            case "E31213": return manager.HasAppeared("E31212");
            case "E32000": return manager.WasChoice("E31213", 2);
            case "E60000": return manager.PrayerRewardPending;
            case "E11200":
            case "E11300":
            case "E31000":
            case "E31211":
            case "E32001":
            case "E32002": return false;
            default: return true;
        }
    }

    public float GetChance()
    {
        switch (EventId)
        {
            case "E21101": return 0.2f;
            case "E32000": return 0.3f;
            default: return Tier == EventTier.Sub ? 0f : 1f;
        }
    }

    public static bool IsSubEventId(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId) || eventId.Length < 2 ||
            !int.TryParse(eventId.Substring(1), out int number)) return false;

        return number >= 40000 && number <= 40700 || number >= 50000 && number <= 50600;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void ValidateRules()
    {
        EventTriggerContext scheduled = new EventTriggerContext(2, GameContext.Conclave.Morning, 2, 1);
        Debug.Assert(new EventTrigger("E20000").MatchesPosition(scheduled), "E20000 위치 규칙이 손상됐습니다.");
        Debug.Assert(!new EventTrigger("E20000").MatchesPosition(
            new EventTriggerContext(2, GameContext.Conclave.Morning, 2, 2)), "E20000이 지정되지 않은 위치와 일치합니다.");
        Debug.Assert(new EventTrigger("E12100").MatchesPosition(
            new EventTriggerContext(1, GameContext.Conclave.Dawn, 1, 3)),
            "초반 무작위 이벤트는 Day 1의 위치 2~4에 발생해야 합니다.");
        Debug.Assert(!new EventTrigger("E11300").MatchesPosition(
            new EventTriggerContext(1, GameContext.Conclave.Dawn, 1, 2)),
            "연설 튜토리얼은 일반 예약에서 발생하면 안 됩니다.");
        Debug.Assert(new EventTrigger("E31213").MatchesPosition(
            new EventTriggerContext(6, GameContext.Conclave.Morning, 2, 2)),
            "두 번째 Morning 이벤트는 Day 6에 발생해야 합니다.");
        EventTrigger delayedStory = new EventTrigger("E21101");
        Debug.Assert(!delayedStory.MatchesPosition(new EventTriggerContext(4, GameContext.Conclave.Evening, 4, 1)) &&
            delayedStory.MatchesPosition(new EventTriggerContext(5, GameContext.Conclave.Dawn, 1, 1)) &&
            delayedStory.MatchesPosition(new EventTriggerContext(12, GameContext.Conclave.Evening, 4, 4)) &&
            delayedStory.MatchesPosition(new EventTriggerContext(13, GameContext.Conclave.Dawn, 1, 1)),
            "암살 사건은 Day 5부터 종료일 제한 없이 발생 가능해야 합니다.");
        Debug.Assert(new EventTrigger("E31000").Tier == EventTier.GuaranteedChain &&
            !new EventTrigger("E31000").MatchesPosition(scheduled),
            "E31000은 E30000 이후 다음 턴의 예약 이벤트로만 발생해야 합니다.");
        Debug.Assert(IsSubEventId("E40000") && IsSubEventId("E50600") && !IsSubEventId("E32000"),
            "서브 이벤트 범위 규칙이 손상됐습니다.");
    }

    private static EventTier GetTier(string eventId)
    {
        if (IsSubEventId(eventId)) return EventTier.Sub;

        switch (eventId)
        {
            case "E11200":
            case "E11300":
            case "E31000":
            case "E31211":
            case "E32001":
            case "E32002": return EventTier.GuaranteedChain;
            case "E21101":
            case "E32000": return EventTier.Story;
            case "E60000": return EventTier.ActionReward;
            default: return EventTier.GuaranteedStory;
        }
    }

    private static bool IsPosition(int day, int action, int expectedDay, int expectedAction)
    {
        return day == expectedDay && action == expectedAction;
    }
}
}
