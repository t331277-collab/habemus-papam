using System.Collections.Generic;
using System.Linq;
using Habemus.Events;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private const int CurrentScheduleVersion = 7;

    [SerializeField] public List<Event> allEvents;

    private readonly HashSet<Event> appeared = new();
    private readonly Dictionary<Event, int> appearedCnt = new();
    private readonly Dictionary<string, ChoiceRecord> choiceRecords = new();
    private readonly HashSet<string> option1ChosenEventIds = new();
    private readonly Dictionary<int, float> plotDamageBonuses = new();
    private readonly List<PendingGuaranteedEventSaveData> pendingGuaranteedEvents = new();
    private readonly Dictionary<int, CandidateDeathSaveData> candidateDeaths = new();
    private readonly Dictionary<string, EventBranchProgressSaveData> branchProgress = new();

    // 0 means no candidate is currently supported; 1~3 are stable candidate slots.
    public int SupportedCandidateNumber { get; private set; }
    public int ConsecutivePrayerCount { get; private set; }
    public bool PrayerRewardPending { get; private set; }

    private bool subEventOccurredThisTurn;
    private int lastEventSelectionDay = -1;
    private int lastEventSelectionPosition = -1;
    private bool guaranteeNextPrayerOrSpeech;
    private bool freePlotPietyForCurrentConclave;

    private struct ChoiceRecord
    {
        public int optionIndex;
        public bool succeeded;
        public int occurrence;
    }

    void Start()
    {
        EventTrigger.ValidateRules();
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent += HandleGameContextEvent;
        }
    }

    void OnDestroy()
    {
        if (InGameManager.Instance != null && InGameManager.Instance.Context != null)
        {
            InGameManager.Instance.Context.OnGameContextEvent -= HandleGameContextEvent;
        }
    }

    public Event PickAnyEvent()
    {
        return PickUniformEvent(allEvents == null ? new List<Event>() : allEvents.Where(e => e != null).ToList());
    }

    public Event GetNewEvent()
    {
        return InGameManager.Instance == null ? null : GetEventForPosition(new EventTriggerContext(
            InGameManager.Instance.GetCurrentDay(),
            InGameManager.Instance.GetCurrentConclave(),
            InGameManager.Instance.GetCurrentTurn(),
            InGameManager.Instance.GetCurrentTurnPhase()));
    }

    public Event GetEventForPosition(EventTriggerContext context)
    {
        // Each displayed turn (action position) gets one scheduled event selection.
        if (lastEventSelectionDay == context.Day && lastEventSelectionPosition == context.ActionPosition)
            return null;
        lastEventSelectionDay = context.Day;
        lastEventSelectionPosition = context.ActionPosition;

        Event priorityEvent = PickCandidateBranch(context);
        if (priorityEvent == null) priorityEvent = TakePendingEvent(PendingEventTiming.NextEnteredPosition);
        // Discard every other follow-up due here instead of carrying it to another turn.
        pendingGuaranteedEvents.RemoveAll(pending =>
            pending != null && pending.timing == PendingEventTiming.NextEnteredPosition);

        List<TriggerCandidate> candidates = GetEligibleCandidates(context)
            .Where(candidate => candidate.evt != priorityEvent).ToList();
        Event primary = PickPrimaryPositionEvent(candidates, priorityEvent);
        if (primary == null)
        {
            Event assassination = candidates.FirstOrDefault(candidate => candidate.evt.eventID == "E21101").evt;
            if (assassination != null && Random.value < new EventTrigger("E21101").GetChance())
                primary = SelectEvent(assassination);
        }

        if (primary == null)
        {
            Event prayerReward = candidates.FirstOrDefault(candidate =>
                candidate.trigger.Tier == EventTier.ActionReward).evt;
            if (prayerReward != null) primary = SelectEvent(prayerReward);
        }

        if (primary == null && !subEventOccurredThisTurn)
        {
            List<Event> subEvents = candidates.Where(candidate => candidate.trigger.Tier == EventTier.Sub)
                .Select(candidate => candidate.evt).ToList();
            if (subEvents.Count > 0)
            {
                int leastAppearances = subEvents.Min(evt => GetAppearCount(evt.eventID));
                Event subEvent = PickUniformEvent(subEvents.Where(evt =>
                    GetAppearCount(evt.eventID) == leastAppearances).ToList());
                subEventOccurredThisTurn = true;
                primary = SelectEvent(subEvent);
            }
        }

        return primary;
    }

    private Event PickPrimaryPositionEvent(List<TriggerCandidate> candidates, Event priorityEvent)
    {
        if (priorityEvent != null) return priorityEvent;
        List<Event> guaranteed = candidates
            .Where(candidate => candidate.trigger.Tier == EventTier.GuaranteedStory)
            .OrderBy(candidate => candidate.evt.eventID)
            .Select(candidate => candidate.evt)
            .ToList();
        List<Event> introEvents = guaranteed.Where(evt => IsRandomIntroEvent(evt.eventID)).ToList();
        if (introEvents.Count > 0)
        {
            // Pick one unseen intro at each of 1-2, 1-3, and 1-4.
            guaranteed.RemoveAll(evt => IsRandomIntroEvent(evt.eventID));
            guaranteed.Add(PickUniformEvent(introEvents));
        }

        if (guaranteed.Count > 0) return SelectEvent(guaranteed[0]);

        List<TriggerCandidate> stories = candidates
            .Where(candidate => candidate.trigger.Tier == EventTier.Story && candidate.evt.eventID != "E21101")
            .OrderBy(candidate => candidate.evt.eventID)
            .ToList();

        float storyTotal = stories.Sum(candidate => candidate.trigger.GetChance());
        float roll = Random.value;
        float accumulated = 0f;

        if (storyTotal > 1f)
        {
            foreach (TriggerCandidate candidate in stories)
            {
                accumulated += candidate.trigger.GetChance() / storyTotal;
                if (roll <= accumulated) return SelectEvent(candidate.evt);
            }

            return stories.Count > 0 ? SelectEvent(stories[stories.Count - 1].evt) : null;
        }

        foreach (TriggerCandidate candidate in stories)
        {
            accumulated += candidate.trigger.GetChance();
            if (roll <= accumulated) return SelectEvent(candidate.evt);
        }

        return null;
    }

    private static bool IsRandomIntroEvent(string eventId) =>
        eventId == "E12300" || eventId == "E12100" || eventId == "E12200";

    public Event GetChainedEvent()
    {
        return TakePendingEvent(PendingEventTiming.AfterCurrentEvent);
    }

    public bool HasRemaining(Event evt)
    {
        if (evt == null) return false;
        if (EventTrigger.IsSubEventId(evt.eventID)) return true;
        int limit = evt.eventID == "E20000" ? 3 :
            evt.eventID == "E21000" || evt.eventID == "E21100" || evt.eventID == "E31212" ||
            evt.eventID == "E31100" || evt.eventID == "E31200" ? 2 : 1;
        if (evt.eventID == "E31200" && branchProgress.TryGetValue(evt.eventID, out var progress) &&
            progress.firstOptionIndex == 2 && !progress.firstSucceeded) return false;
        if ((evt.eventID == "E20000" || evt.eventID == "E21000" || evt.eventID == "E21100") &&
            HasChosenOption1(evt.eventID)) return false;
        return GetAppearCount(evt.eventID) < limit;
    }

    public bool HasRemaining(string eventId) => HasRemaining(GetEventById(eventId));

    public int GetAppearCount(string eventId)
    {
        Event evt = GetEventById(eventId);
        return evt != null && appearedCnt.TryGetValue(evt, out int count) ? count : 0;
    }

    public bool HasChosenOption1(string eventId) => option1ChosenEventIds.Contains(eventId);

    public bool HasAppeared(string eventId)
    {
        Event evt = GetEventById(eventId);
        return evt != null && appeared.Contains(evt);
    }

    public bool HasPendingEvent(string eventId)
    {
        return pendingGuaranteedEvents.Any(pending => pending != null && pending.eventId == eventId);
    }

    public void SetSupportedCandidate(int candidateNumber)
    {
        SupportedCandidateNumber = Mathf.Clamp(candidateNumber, 0, 3);
    }

    public void RecordPlayerAction(NPCBehaviour completedAction)
    {
        ConsecutivePrayerCount = completedAction == NPCBehaviour.Pray ? ConsecutivePrayerCount + 1 : 0;
        // Once earned, keep the reward pending even if a story delays its appearance.
        if (ConsecutivePrayerCount >= 5 && !HasAppeared("E60000")) PrayerRewardPending = true;
    }

    public void RecordCandidateDeath(int candidateNumber, int day)
    {
        if (candidateNumber < 1 || candidateNumber > 3 || candidateDeaths.ContainsKey(candidateNumber)) return;
        candidateDeaths[candidateNumber] = new CandidateDeathSaveData
        {
            candidateNumber = candidateNumber,
            day = Mathf.Max(1, day),
            sequence = candidateDeaths.Count == 0 ? 1 : candidateDeaths.Values.Max(death => death.sequence) + 1
        };
    }

    public int GetCandidateDeathDay(int candidateNumber) =>
        candidateDeaths.TryGetValue(candidateNumber, out var death) ? death.day : -1;

    private int GetFirstEliminatedBranchCandidate()
    {
        candidateDeaths.TryGetValue(2, out var candidate2);
        candidateDeaths.TryGetValue(3, out var candidate3);
        if (candidate2 == null) return candidate3 == null ? 0 : 3;
        if (candidate3 == null) return 2;
        if (candidate2.day != candidate3.day) return candidate2.day < candidate3.day ? 2 : 3;
        // Old saves cannot recover the order of two deaths on the same day.
        if (candidate2.sequence == candidate3.sequence) return 0;
        return candidate2.sequence < candidate3.sequence ? 2 : 3;
    }

    public bool IsCandidateBranchDue(string eventId, EventTriggerContext context)
    {
        if (!HasAppeared("E31000") || !HasRemaining(eventId)) return false;
        int first = GetFirstEliminatedBranchCandidate();
        int candidateNumber = eventId == "E31100" ? 3 : eventId == "E31200" ? 2 : 0;
        if (candidateNumber == 0 || first != candidateNumber) return false;
        int offset = candidateNumber == 3 ? 2 : 1;
        int expectedCount = context.ActionPosition == 2 ? 0 : context.ActionPosition == 4 ? 1 : -1;
        return context.Day == GetCandidateDeathDay(candidateNumber) + offset &&
            expectedCount >= 0 && GetAppearCount(eventId) == expectedCount;
    }

    public int GetBranchPietyRequirement(string eventId, int baseRequirement)
    {
        branchProgress.TryGetValue(eventId, out var progress);
        bool supportDiscount = SupportedCandidateNumber == 3 || progress != null && progress.supportDiscount;
        bool secondRoundPenalty = GetAppearCount(eventId) >= 2 && progress != null &&
            progress.firstOptionIndex == 2 && (eventId == "E31100" || progress.firstSucceeded);
        return Mathf.Max(0, baseRequirement - (supportDiscount ? 3 : 0) + (secondRoundPenalty ? 2 : 0));
    }

    public Event GetEventById(string eventId)
    {
        if (allEvents == null) return null;
        return allEvents.FirstOrDefault(evt => evt != null && evt.eventID == eventId);
    }

    public Event TakeEventOnce(string eventId)
    {
        return HasAppeared(eventId) ? null : SelectEvent(GetEventById(eventId));
    }

    public void InitEventManager()
    {
        appeared.Clear();
        appearedCnt.Clear();
        choiceRecords.Clear();
        option1ChosenEventIds.Clear();
        plotDamageBonuses.Clear();
        pendingGuaranteedEvents.Clear();
        candidateDeaths.Clear();
        branchProgress.Clear();
        SupportedCandidateNumber = 0;
        ConsecutivePrayerCount = 0;
        PrayerRewardPending = false;
        subEventOccurredThisTurn = false;
        lastEventSelectionDay = -1;
        lastEventSelectionPosition = -1;
        ClearConclaveEffects();
    }

    public void RecordChoice(string eventId, int optionIndex, bool succeeded)
    {
        if (string.IsNullOrWhiteSpace(eventId)) return;
        if (optionIndex == 1) option1ChosenEventIds.Add(eventId);

        Event chosenEvent = GetEventById(eventId);
        if (chosenEvent != null && !appeared.Contains(chosenEvent)) MarkEventAppeared(chosenEvent);

        int occurrence = Mathf.Max(1, GetAppearCount(eventId));
        if (choiceRecords.TryGetValue(eventId, out ChoiceRecord existing) && existing.occurrence == occurrence &&
            existing.optionIndex == optionIndex && existing.succeeded == succeeded)
        {
            return;
        }

        choiceRecords[eventId] = new ChoiceRecord
            { optionIndex = optionIndex, succeeded = succeeded, occurrence = occurrence };

        if ((eventId == "E31100" || eventId == "E31200") && occurrence == 1)
        {
            if (!branchProgress.TryGetValue(eventId, out var progress))
            {
                progress = new EventBranchProgressSaveData
                    { eventId = eventId, supportDiscount = SupportedCandidateNumber == 3 };
                branchProgress[eventId] = progress;
            }
            if (progress.firstOptionIndex == 0)
            {
                progress.firstOptionIndex = optionIndex;
                progress.firstSucceeded = succeeded;
            }
        }

        switch (eventId)
        {
            case "E11100":
                QueueGuaranteedEvent("E11200", PendingEventTiming.AfterCurrentEvent);
                break;
            case "E30000":
                QueueGuaranteedEvent("E31000", PendingEventTiming.NextEnteredPosition, true);
                break;
            case "E31000":
                SetSupportedCandidate(optionIndex == 2 ? 3 : 2);
                break;
            case "E31100" when optionIndex == 1:
                SetSupportedCandidate(3);
                break;
            case "E31100" when optionIndex == 2 && occurrence == 1:
                SetSupportedCandidate(1);
                break;
            case "E31200":
                SetSupportedCandidate(optionIndex == 1 ? 2 : 3);
                break;
            case "E21100":
                SetSupportedCandidate(optionIndex == 1 ? 1 : 2);
                break;
            case "E31213":
                SetSupportedCandidate(optionIndex == 1 ? 1 : 3);
                break;
            case "E31210" when optionIndex == 2 && !succeeded && !IsCandidateEliminated(1):
                QueueGuaranteedEvent("E31211", PendingEventTiming.NextEnteredPosition);
                break;
            case "E32000" when optionIndex == 1 && !succeeded:
                QueueGuaranteedEvent("E32001", PendingEventTiming.NextEnteredPosition);
                break;
            case "E32001" when optionIndex == 1:
                QueueGuaranteedEvent("E32002", PendingEventTiming.NextEnteredPosition);
                break;
            case "E32001" when optionIndex == 2:
                CancelPendingEvent("E32002");
                break;
        }
    }

    public bool WasChoice(string eventId, int optionIndex, bool? succeeded = null)
    {
        if (!choiceRecords.TryGetValue(eventId, out ChoiceRecord record) || record.optionIndex != optionIndex)
        {
            return false;
        }

        return !succeeded.HasValue || record.succeeded == succeeded.Value;
    }

    public bool TryGetTemporaryFlagValue(string flagId, out int value)
    {
        value = 0;
        switch (flagId)
        {
            case "F10000":
                value = WasChoice("E20000", 1, true) ? 1 : 0;
                return value != 0;
            case "F10001":
                value = WasChoice("E21101", 1) || WasChoice("E31100", 2) ||
                    WasChoice("E31211", 1) || WasChoice("E32001", 1) ? 1 : 0;
                return value != 0;
            case "F10011": return TryGetRecordedOption("E21100", out value);
            case "F20000": return TryGetBooleanChoice("E30000", 1, out value);
            case "F21000": return TryGetRecordedOption("E31000", out value);
            case "F21100": return TryGetBooleanChoice("E31100", 1, out value);
            case "F21110": return TryGetRecordedOption("E31101", out value);
            case "F21200": return TryGetBooleanChoice("E31200", 1, out value);
            case "F21210":
                value = WasChoice("E31210", 2, false) ? 1 : 0;
                return value != 0;
            case "F30000":
                if (!choiceRecords.TryGetValue("E31212", out ChoiceRecord ascension) || ascension.optionIndex != 1)
                    return false;
                value = ascension.succeeded ? 1 : 0;
                return true;
            case "F31000": return TryGetBooleanChoice("E31213", 2, out value);
            case "F31100": return TryGetBooleanChoice("E32000", 1, out value);
            case "F31110": return TryGetBooleanChoice("E32001", 2, out value);
            default: return false;
        }
    }

    public void SetPlotDamageBonus(int candidateNumber, float bonus)
    {
        plotDamageBonuses[candidateNumber] = bonus;
    }

    public float GetPlotDamageBonus(int candidateNumber)
    {
        return plotDamageBonuses.TryGetValue(candidateNumber, out float bonus) ? bonus : 0f;
    }

    public float ModifyPlotHpDelta(Cardinal performer, Cardinal target, float delta)
    {
        if (delta >= 0f || performer == null || target == null || !performer.CompareTag("Player")) return delta;
        int candidateNumber = GetCandidateNumber(target);
        return candidateNumber > 0 ? delta - GetPlotDamageBonus(candidateNumber) : delta;
    }

    public void GuaranteeNextPrayerOrSpeech()
    {
        guaranteeNextPrayerOrSpeech = true;
    }

    public bool TryConsumeGuaranteedPrayerOrSpeech(Cardinal performer)
    {
        if (performer == null || !performer.CompareTag("Player") || !guaranteeNextPrayerOrSpeech) return false;
        guaranteeNextPrayerOrSpeech = false;
        return true;
    }

    public void SetFreePlotPietyForCurrentConclave()
    {
        freePlotPietyForCurrentConclave = true;
    }

    public bool FreePlotPietyForCurrentConclave => freePlotPietyForCurrentConclave;

    public bool IsPlotPietyCostWaived(Cardinal performer)
    {
        return freePlotPietyForCurrentConclave && performer != null && performer.CompareTag("Player");
    }

    public EventManagerSaveData CaptureSaveData()
    {
        EventManagerSaveData saveData = new EventManagerSaveData
        {
            scheduleVersion = CurrentScheduleVersion,
            supportedCandidateNumber = SupportedCandidateNumber,
            consecutivePrayerCount = ConsecutivePrayerCount,
            prayerRewardPending = PrayerRewardPending,
            subEventOccurredThisTurn = subEventOccurredThisTurn,
            lastEventSelectionDay = lastEventSelectionDay,
            lastEventSelectionPosition = lastEventSelectionPosition,
            guaranteeNextPrayerOrSpeech = guaranteeNextPrayerOrSpeech,
            freePlotPietyForCurrentConclave = freePlotPietyForCurrentConclave
        };

        foreach (var pair in appearedCnt)
        {
            if (pair.Key == null) continue;
            saveData.records.Add(new EventRecordSaveData
            {
                eventId = pair.Key.eventID,
                appearCount = pair.Value
            });
        }

        foreach (var pair in choiceRecords.OrderBy(pair => pair.Key))
        {
            saveData.choices.Add(new EventChoiceSaveData
            {
                eventId = pair.Key,
                optionIndex = pair.Value.optionIndex,
                succeeded = pair.Value.succeeded,
                occurrence = pair.Value.occurrence
            });
        }

        saveData.option1ChosenEventIds.AddRange(option1ChosenEventIds.OrderBy(eventId => eventId));
        foreach (CandidateDeathSaveData death in candidateDeaths.Values.OrderBy(death => death.sequence))
            saveData.candidateDeaths.Add(new CandidateDeathSaveData
                { candidateNumber = death.candidateNumber, day = death.day, sequence = death.sequence });
        foreach (EventBranchProgressSaveData progress in branchProgress.Values.OrderBy(progress => progress.eventId))
            saveData.branchProgress.Add(new EventBranchProgressSaveData
            {
                eventId = progress.eventId, firstOptionIndex = progress.firstOptionIndex,
                firstSucceeded = progress.firstSucceeded, supportDiscount = progress.supportDiscount
            });

        foreach (var pair in plotDamageBonuses.OrderBy(pair => pair.Key))
        {
            saveData.plotDamageBonuses.Add(new EventPlotDamageBonusSaveData
            {
                candidateNumber = pair.Key,
                bonus = pair.Value
            });
        }

        foreach (PendingGuaranteedEventSaveData pending in pendingGuaranteedEvents)
        {
            if (pending == null || string.IsNullOrWhiteSpace(pending.eventId)) continue;
            saveData.pendingGuaranteedEvents.Add(new PendingGuaranteedEventSaveData
            {
                eventId = pending.eventId,
                timing = pending.timing
            });
        }

        return saveData;
    }

    public void RestoreFromSave(EventManagerSaveData saveData)
    {
        appeared.Clear();
        appearedCnt.Clear();
        choiceRecords.Clear();
        option1ChosenEventIds.Clear();
        plotDamageBonuses.Clear();
        pendingGuaranteedEvents.Clear();
        candidateDeaths.Clear();
        branchProgress.Clear();
        SupportedCandidateNumber = 0;
        ConsecutivePrayerCount = 0;
        PrayerRewardPending = false;
        subEventOccurredThisTurn = false;
        lastEventSelectionDay = -1;
        lastEventSelectionPosition = -1;
        ClearConclaveEffects();

        if (saveData == null) return;

        if (saveData.scheduleVersion >= 7)
        {
            lastEventSelectionDay = saveData.lastEventSelectionDay;
            lastEventSelectionPosition = saveData.lastEventSelectionPosition;
        }

        RestoreEventRecords(saveData.records);
        RestoreChoiceRecords(saveData.choices);
        ConsecutivePrayerCount = Mathf.Max(0, saveData.consecutivePrayerCount);
        PrayerRewardPending = !HasAppeared("E60000") &&
            (saveData.prayerRewardPending || ConsecutivePrayerCount >= 5);
        SetSupportedCandidate(saveData.scheduleVersion >= 5 ? saveData.supportedCandidateNumber :
            HasAppeared("E31000") ? (WasChoice("E31000", 2) ? 3 : 2) : 0);
        if (saveData.candidateDeaths != null)
        {
            foreach (CandidateDeathSaveData death in saveData.candidateDeaths)
            {
                if (death == null || death.candidateNumber < 1 || death.candidateNumber > 3 || death.day < 1) continue;
                candidateDeaths[death.candidateNumber] = new CandidateDeathSaveData
                    { candidateNumber = death.candidateNumber, day = death.day, sequence = Mathf.Max(0, death.sequence) };
            }
        }
        if (saveData.scheduleVersion < 5) RestoreLegacyCandidateDeaths();
        if (saveData.branchProgress != null)
        {
            foreach (EventBranchProgressSaveData progress in saveData.branchProgress)
            {
                if (progress == null || (progress.eventId != "E31100" && progress.eventId != "E31200")) continue;
                branchProgress[progress.eventId] = new EventBranchProgressSaveData
                {
                    eventId = progress.eventId, firstOptionIndex = Mathf.Clamp(progress.firstOptionIndex, 0, 2),
                    firstSucceeded = progress.firstSucceeded, supportDiscount = progress.supportDiscount
                };
            }
        }
        if (saveData.option1ChosenEventIds != null)
        {
            foreach (string eventId in saveData.option1ChosenEventIds)
                if (!string.IsNullOrWhiteSpace(eventId) && GetEventById(eventId) != null)
                    option1ChosenEventIds.Add(eventId);
        }
        RestorePlotBonuses(saveData.plotDamageBonuses);

        if (saveData.scheduleVersion >= 3 && saveData.pendingGuaranteedEvents != null)
        {
            foreach (PendingGuaranteedEventSaveData pending in saveData.pendingGuaranteedEvents)
            {
                if (pending == null || string.IsNullOrWhiteSpace(pending.eventId) ||
                    pending.eventId == "E31100" || pending.eventId == "E31200" ||
                    new EventTrigger(pending.eventId).Tier != EventTier.GuaranteedChain ||
                    (pending.timing == PendingEventTiming.AfterCurrentEvent &&
                     pending.eventId != "E11200" && pending.eventId != "E31000") ||
                    !HasRemaining(pending.eventId)) continue;

                pendingGuaranteedEvents.Add(new PendingGuaranteedEventSaveData
                {
                    eventId = pending.eventId,
                    timing = pending.eventId == "E31000" ? PendingEventTiming.NextEnteredPosition : pending.timing
                });
            }

            subEventOccurredThisTurn = saveData.subEventOccurredThisTurn;
        }

        guaranteeNextPrayerOrSpeech = saveData.guaranteeNextPrayerOrSpeech;
        freePlotPietyForCurrentConclave = saveData.freePlotPietyForCurrentConclave;
    }

    public bool IsCandidateEliminated(int candidateNumber)
    {
        if (CardinalManager.Instance == null) return false;

        StatsUI statsUI = CardinalManager.Instance.StatsUI;
        Cardinal[] linked = statsUI != null ? statsUI.LinkedCardinals : null;
        Cardinal candidate = linked != null && linked.Length > candidateNumber ? linked[candidateNumber] : null;

        if (candidate == null)
        {
            List<Cardinal> ai = CardinalManager.Instance.GetAICardinlas();
            int index = candidateNumber - 1;
            candidate = index >= 0 && index < ai.Count ? ai[index] : null;
        }

        return candidate != null && (candidate.Hp <= 0f || candidate.IsKnockedOut);
    }

    private List<TriggerCandidate> GetEligibleCandidates(EventTriggerContext context)
    {
        List<TriggerCandidate> candidates = new();
        if (allEvents == null) return candidates;

        foreach (Event evt in allEvents)
        {
            if (evt == null || !HasRemaining(evt) || HasPendingEvent(evt.eventID)) continue;
            EventTrigger trigger = new EventTrigger(evt.eventID);
            if (trigger.Tier == EventTier.GuaranteedChain || !trigger.IsEligible(context, this)) continue;
            candidates.Add(new TriggerCandidate(evt, trigger));
        }

        return candidates;
    }

    private Event PickCandidateBranch(EventTriggerContext context)
    {
        Event event311 = GetEventById("E31100");
        Event event312 = GetEventById("E31200");
        bool can311 = event311 != null && new EventTrigger("E31100").IsEligible(context, this);
        bool can312 = event312 != null && new EventTrigger("E31200").IsEligible(context, this);
        if (!can311 && !can312) return null;

        Event picked = can311 && can312 ? (Random.value < 0.5f ? event311 : event312) : can311 ? event311 : event312;
        return SelectEvent(picked);
    }

    private void RestoreLegacyCandidateDeaths()
    {
        if (CardinalManager.Instance == null) return;
        List<Cardinal> ai = CardinalManager.Instance.GetAICardinlas();
        Cardinal[] linked = CardinalManager.Instance.StatsUI != null ? CardinalManager.Instance.StatsUI.LinkedCardinals : null;
        for (int number = 1; number <= 3; number++)
        {
            Cardinal candidate = linked != null && linked.Length > number ? linked[number] :
                ai.Count >= number ? ai[number - 1] : null;
            if (candidate == null || !candidate.IsDead || candidate.DeathDay < 1) continue;
            candidateDeaths[number] = new CandidateDeathSaveData
                { candidateNumber = number, day = candidate.DeathDay, sequence = 0 };
        }
    }

    private void QueueGuaranteedEvent(string eventId, PendingEventTiming timing, bool prepend = false)
    {
        if (string.IsNullOrWhiteSpace(eventId) || !HasRemaining(eventId) || HasPendingEvent(eventId)) return;
        var pending = new PendingGuaranteedEventSaveData { eventId = eventId, timing = timing };
        if (prepend) pendingGuaranteedEvents.Insert(0, pending);
        else pendingGuaranteedEvents.Add(pending);
    }

    private void CancelPendingEvent(string eventId)
    {
        pendingGuaranteedEvents.RemoveAll(pending => pending != null && pending.eventId == eventId);
    }

    private Event TakePendingEvent(PendingEventTiming timing)
    {
        for (int index = 0; index < pendingGuaranteedEvents.Count; index++)
        {
            PendingGuaranteedEventSaveData pending = pendingGuaranteedEvents[index];
            if (pending == null || pending.timing != timing) continue;
            pendingGuaranteedEvents.RemoveAt(index--);
            if (string.IsNullOrWhiteSpace(pending.eventId) || !HasRemaining(pending.eventId)) continue;

            Event evt = GetEventById(pending.eventId);
            if (evt != null) return SelectEvent(evt);
            Debug.LogWarning($"[Event] 대기 이벤트 '{pending.eventId}'가 등록되지 않았습니다.");
        }

        return null;
    }

    private Event SelectEvent(Event evt)
    {
        if (evt == null) return null;
        MarkEventAppeared(evt);
        return evt;
    }

    private void MarkEventAppeared(Event evt)
    {
        if (evt == null) return;
        if (evt.eventID == "E60000") PrayerRewardPending = false;
        appeared.Add(evt);
        if ((evt.eventID == "E31100" || evt.eventID == "E31200") && !branchProgress.ContainsKey(evt.eventID))
            branchProgress[evt.eventID] = new EventBranchProgressSaveData
                { eventId = evt.eventID, supportDiscount = SupportedCandidateNumber == 3 };
        appearedCnt[evt] = appearedCnt.TryGetValue(evt, out int count) ? count + 1 : 1;
    }

    private static Event PickUniformEvent(List<Event> candidates)
    {
        if (candidates == null || candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    private bool TryGetRecordedOption(string eventId, out int optionIndex)
    {
        optionIndex = 0;
        if (!choiceRecords.TryGetValue(eventId, out ChoiceRecord record)) return false;
        optionIndex = record.optionIndex;
        return true;
    }

    private bool TryGetBooleanChoice(string eventId, int expectedOption, out int value)
    {
        value = WasChoice(eventId, expectedOption) ? 1 : 0;
        return value != 0;
    }

    private int GetCandidateNumber(Cardinal target)
    {
        if (target == null || CardinalManager.Instance == null) return 0;

        StatsUI statsUI = CardinalManager.Instance.StatsUI;
        Cardinal[] linked = statsUI != null ? statsUI.LinkedCardinals : null;
        if (linked != null)
        {
            for (int candidateNumber = 1; candidateNumber <= 3 && candidateNumber < linked.Length; candidateNumber++)
            {
                if (linked[candidateNumber] == target) return candidateNumber;
            }
        }

        List<Cardinal> ai = CardinalManager.Instance.GetAICardinlas();
        for (int index = 0; index < 3 && index < ai.Count; index++)
        {
            if (ai[index] == target) return index + 1;
        }

        return 0;
    }

    private void RestoreEventRecords(List<EventRecordSaveData> records)
    {
        if (records == null) return;
        foreach (EventRecordSaveData record in records)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.eventId)) continue;
            Event restoredEvent = GetEventById(record.eventId);
            if (restoredEvent == null)
            {
                Debug.LogWarning($"[Save] 이벤트 '{record.eventId}'를 찾지 못해 복원을 건너뜁니다.");
                continue;
            }

            appeared.Add(restoredEvent);
            appearedCnt[restoredEvent] = Mathf.Max(0, record.appearCount);
        }
    }

    private void RestoreChoiceRecords(List<EventChoiceSaveData> choices)
    {
        if (choices == null) return;
        foreach (EventChoiceSaveData choice in choices)
        {
            if (choice == null || string.IsNullOrWhiteSpace(choice.eventId) ||
                choice.optionIndex < 1 || choice.optionIndex > 2) continue;

            Event restoredEvent = GetEventById(choice.eventId);
            if (restoredEvent == null)
            {
                Debug.LogWarning($"[Save] 선택 결과 이벤트 '{choice.eventId}'를 찾지 못해 복원을 건너뜁니다.");
                continue;
            }

            choiceRecords[choice.eventId] = new ChoiceRecord
            {
                optionIndex = choice.optionIndex,
                succeeded = choice.succeeded,
                occurrence = choice.occurrence > 0 ? choice.occurrence : Mathf.Max(1, GetAppearCount(choice.eventId))
            };
            if (choice.optionIndex == 1) option1ChosenEventIds.Add(choice.eventId);
            appeared.Add(restoredEvent);
            if (!appearedCnt.ContainsKey(restoredEvent)) appearedCnt[restoredEvent] = 1;
        }
    }

    private void RestorePlotBonuses(List<EventPlotDamageBonusSaveData> bonuses)
    {
        if (bonuses == null) return;
        foreach (EventPlotDamageBonusSaveData bonus in bonuses)
        {
            if (bonus == null || bonus.candidateNumber < 1 || bonus.candidateNumber > 3 ||
                float.IsNaN(bonus.bonus) || float.IsInfinity(bonus.bonus)) continue;
            plotDamageBonuses[bonus.candidateNumber] = Mathf.Max(0f, bonus.bonus);
        }
    }

    private void HandleGameContextEvent(GameContext.GameContextEvent eventType)
    {
        if (eventType == GameContext.GameContextEvent.TurnStart) subEventOccurredThisTurn = false;
        if (eventType == GameContext.GameContextEvent.ConclaveEnd) ClearConclaveEffects();
    }

    private void ClearConclaveEffects()
    {
        plotDamageBonuses.Clear();
        guaranteeNextPrayerOrSpeech = false;
        freePlotPietyForCurrentConclave = false;

        if (CardinalManager.Instance == null) return;
        foreach (Cardinal cardinal in CardinalManager.Instance.Cardinals)
        {
            if (cardinal == null) continue;
            cardinal.SetMinHpOneEffect("E50600", false);
            cardinal.SetMinHpOneEffect("P030", false);
        }
    }

    private readonly struct TriggerCandidate
    {
        public Event evt { get; }
        public EventTrigger trigger { get; }

        public TriggerCandidate(Event evt, EventTrigger trigger)
        {
            this.evt = evt;
            this.trigger = trigger;
        }
    }
}
