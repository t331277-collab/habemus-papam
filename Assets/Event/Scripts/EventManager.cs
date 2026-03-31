using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [SerializeField] public List<Event> allEvents;

    private readonly HashSet<Event> appeared = new();
    private readonly Dictionary<Event, int> appearedCnt = new();

    void Start()
    {

    }

    public Event PickAnyEvent()
    {
        Event pickedEvent = null;
        
        float totalWeight = 0f;
        foreach(var e in allEvents) totalWeight += e.GetEventWeight();
        
        float roll = Random.value * totalWeight;

        float accWeight = 0f;
        foreach(var e in allEvents)
        {
            accWeight += e.GetEventWeight();

            if(roll <= accWeight)
            {
                pickedEvent = e;
                break;
            }

            continue;
        }

        return pickedEvent;
    }

    public Event GetNewEvent()
    {
        Event pickedEvent = null;

        int tryCnt = 100;
        while (tryCnt--  > 0)
        {
            pickedEvent = PickAnyEvent();

            if(pickedEvent == null) continue;

            if(!HasRemaining(pickedEvent)) continue;
            if(!PreEventSatisfied(pickedEvent)) continue;
            if(!ConflictEventSatisfied(pickedEvent)) continue;

            MarkEventAppeared(pickedEvent);
            Debug.Log($"이벤트 \"{pickedEvent}\" 선택");
            return pickedEvent;
        }
        Debug.Log($"이벤트 선택 실패, currentEvent = \"{pickedEvent}\"");
        return pickedEvent;
    }

    public bool HasRemaining(Event e)
    {
        int count = 0;
        appearedCnt.TryGetValue(e, out count);

        return count < e.maxAppear;
    }

    public bool PreEventSatisfied(Event e)
    {
        var pres = e.preEvents;

        if(pres == null || pres.Count == 0) return true;

        foreach(var pre in pres)
        {
            if(!pre) continue;

            if (!appeared.Contains(pre))
            {
                return false;
            }
        }

        return true;
    }

    public void MarkEventAppeared(Event e)
    {
        appeared.Add(e);

        int count = 0;
        appearedCnt.TryGetValue(e, out count);

        appearedCnt[e] = count + 1;
    }

    public bool ConflictEventSatisfied(Event e)
    {
        var conflicts = e.conflictEvents;

        if(conflicts == null || conflicts.Count == 0) return true;

        foreach(var conf in conflicts)
        {
            if(!conf) continue;

            if(appeared.Contains(conf)) return false;
        }

        return true;
    }

    public Event GetEventById(string eventId)
    {
        foreach(var e in allEvents)
        {
            if(!e) continue;

            if(e.eventID == eventId) return e;
        }

        return null;
    }

    public void InitEventManager()
    {
        appeared.Clear();
        appearedCnt.Clear();
    }

    public EventManagerSaveData CaptureSaveData()
    {
        EventManagerSaveData saveData = new EventManagerSaveData();

        foreach (var pair in appearedCnt)
        {
            if (pair.Key == null)
            {
                continue;
            }

            saveData.records.Add(new EventRecordSaveData
            {
                eventId = pair.Key.eventID,
                appearCount = pair.Value
            });
        }

        return saveData;
    }

    public void RestoreFromSave(EventManagerSaveData saveData)
    {
        appeared.Clear();
        appearedCnt.Clear();

        if (saveData == null || saveData.records == null)
        {
            return;
        }

        foreach (var record in saveData.records)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.eventId))
            {
                continue;
            }

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
}
