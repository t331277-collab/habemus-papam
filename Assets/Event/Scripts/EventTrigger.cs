using UnityEngine;

namespace Habemus.Events
{
public enum EventTier
{
    GuaranteedChain,
    GuaranteedStory,
    Story,
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

        // Preserve the original event positions after making each conclave one day:
        // displayed day = (former day - 1) * 4 + former turn.
        switch (EventId)
        {
            case "E11100": return IsPosition(day, action, 1, 1);
            case "E11300": return IsPosition(day, action, 1, 2);
            case "E12300": return IsPosition(day, action, 2, 1);
            case "E12100": return IsPosition(day, action, 2, 2);
            case "E12200": return IsPosition(day, action, 2, 3);

            case "E20000":
                return day >= 3 && day <= 4 && (action == 2 || action == 4);
            case "E21000":
                return day >= 6 && day <= 7 && (action == 2 || action == 4);
            case "E21100": return IsPosition(day, action, 9, 1);
            case "E21101": return day >= 9 && day <= 12;

            case "E30000":
                return day >= 6 && day <= 7 && (action == 1 || action == 3);
            case "E31000": return day >= 5 && day <= 12;
            case "E31100":
            case "E31200": return true;
            case "E31101": return IsPosition(day, action, 13, 1);
            case "E31210": return IsPosition(day, action, 14, 2);

            case "E31212":
                return day == 4 && action >= 1 && action <= 4;
            case "E31213": return IsPosition(day, action, 6, 2);
            case "E32000":
                return day == 7 && action >= 1 && action <= 3;

            case "E11200":
            case "E31211":
            case "E32001":
            case "E32002": return false;
            default: return IsSubEventId(EventId);
        }
    }

    public bool IsEligible(EventTriggerContext context, EventManager manager)
    {
        if (manager == null || manager.HasAppeared(EventId) || !MatchesPosition(context)) return false;

        switch (EventId)
        {
            case "E11300": return manager.HasAppeared("E11200");
            case "E21000": return !manager.HasAppeared("E20000");
            case "E21100": return manager.WasChoice("E20000", 1);
            case "E21101": return !manager.HasAppeared("E21100");
            case "E31000": return manager.HasAppeared("E30000");
            case "E31100":
                return manager.IsCandidateBranchAvailable() && manager.IsCandidateEliminated(3);
            case "E31200":
                return manager.IsCandidateBranchAvailable() && manager.IsCandidateEliminated(2) &&
                    !manager.IsCandidateEliminated(1);
            case "E31101": return manager.WasChoice("E31100", 1);
            case "E31210": return manager.WasChoice("E31200", 1);
            case "E31213": return manager.HasAppeared("E31212");
            case "E32000": return manager.WasChoice("E31213", 2);
            case "E11200":
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
            case "E20000":
            case "E21000": return 0.4f;
            case "E21101": return 0.2f;
            case "E31000": return 0.7f;
            case "E31212":
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
        EventTriggerContext scheduled = new EventTriggerContext(3, GameContext.Conclave.Afternoon, 3, 2);
        Debug.Assert(new EventTrigger("E20000").MatchesPosition(scheduled), "E20000 위치 규칙이 손상됐습니다.");
        Debug.Assert(!new EventTrigger("E20000").MatchesPosition(
            new EventTriggerContext(3, GameContext.Conclave.Afternoon, 3, 3)), "E20000이 지정되지 않은 위치와 일치합니다.");
        Debug.Assert(new EventTrigger("E12100").MatchesPosition(
            new EventTriggerContext(2, GameContext.Conclave.Morning, 2, 2)),
            "첫 번째 Morning 이벤트는 Day 2에 발생해야 합니다.");
        Debug.Assert(new EventTrigger("E31213").MatchesPosition(
            new EventTriggerContext(6, GameContext.Conclave.Morning, 2, 2)),
            "두 번째 Morning 이벤트는 Day 6에 발생해야 합니다.");
        EventTrigger delayedStory = new EventTrigger("E21101");
        Debug.Assert(!delayedStory.MatchesPosition(new EventTriggerContext(8, GameContext.Conclave.Evening, 4, 1)) &&
            delayedStory.MatchesPosition(new EventTriggerContext(9, GameContext.Conclave.Dawn, 1, 1)) &&
            delayedStory.MatchesPosition(new EventTriggerContext(12, GameContext.Conclave.Evening, 4, 4)) &&
            !delayedStory.MatchesPosition(new EventTriggerContext(13, GameContext.Conclave.Dawn, 1, 1)),
            "기존 3일차 이벤트 범위는 Day 9~12여야 합니다.");
        Debug.Assert(Mathf.Approximately(new EventTrigger("E31000").GetChance(), 0.7f),
            "E31000 확률 규칙이 손상됐습니다.");
        Debug.Assert(IsSubEventId("E40000") && IsSubEventId("E50600") && !IsSubEventId("E32000"),
            "서브 이벤트 범위 규칙이 손상됐습니다.");
    }

    private static EventTier GetTier(string eventId)
    {
        if (IsSubEventId(eventId)) return EventTier.Sub;

        switch (eventId)
        {
            case "E11200":
            case "E31211":
            case "E32001":
            case "E32002": return EventTier.GuaranteedChain;
            case "E20000":
            case "E21000":
            case "E21101":
            case "E31000":
            case "E31212":
            case "E32000": return EventTier.Story;
            default: return EventTier.GuaranteedStory;
        }
    }

    private static bool IsPosition(int day, int action, int expectedDay, int expectedAction)
    {
        return day == expectedDay && action == expectedAction;
    }
}
}
