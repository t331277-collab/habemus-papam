using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveModel
{
    public int version = 1;
    public string savedAtUtc;
    public string sceneName = "GameScene";
    public GameContextSaveData gameContext = new GameContextSaveData();
    public List<CardinalSaveData> cardinals = new List<CardinalSaveData>();
    public InventorySaveData inventory = new InventorySaveData();
    public EventManagerSaveData events = new EventManagerSaveData();
    public PlotManagerSaveData plots = new PlotManagerSaveData();
    public List<FieldItemSaveData> fieldItems = new List<FieldItemSaveData>();
    public GameNameSaveData names = new GameNameSaveData();
}

[Serializable]
public class SavePreviewData
{
    public string playerName = string.Empty;
    public float playerHp;
    public float playerInfluence;
    public float playerPiety;
    public int day = 1;
    public int conclave;
    public string conclaveName = string.Empty;
}

[Serializable]
public class GameNameSaveData
{
    public string playerName = string.Empty;
    public List<string> npcNames = new List<string>();
}

[Serializable]
public class CompletedPlayerNameSaveData
{
    public List<string> playerInputNames = new List<string>();
}

[Serializable]
public class GameContextSaveData
{
    public int day = 1;
    public int conclave;
    public float remainingTime;
    public bool isTimeRunning;
    public bool isFirstStart = true;
    public bool isSushiOn;
    public bool showStartButton = true;
    public bool startButtonInteractable = true;
    public bool showInventoryPanel;
}

[Serializable]
public class CardinalSaveData
{
    public int index;
    public string objectName;
    public bool isPlayer;
    public bool isActive;
    public float hp;
    public float influence;
    public float piety;
    public float hpDrainMultiplier = 1f;
    public float prayDeltaHpEvent;
    public bool isKnockedOut;
    public bool isSchemer;
    public bool isConClaving;
    public int state;
    public SerializableVector3 position = new SerializableVector3();
    public float rotationZ;
}

[Serializable]
public class InventorySaveData
{
    public int maxSlots = 3;
    public List<ItemSaveData> inventoryItems = new List<ItemSaveData>();
    public List<ItemSaveData> activeBuffs = new List<ItemSaveData>();
}

[Serializable]
public class ItemSaveData
{
    public string itemId;
    public string runtimeStateJson;
}

[Serializable]
public class EventManagerSaveData
{
    public List<EventRecordSaveData> records = new List<EventRecordSaveData>();
}

[Serializable]
public class EventRecordSaveData
{
    public string eventId;
    public int appearCount;
}

[Serializable]
public class PlotManagerSaveData
{
    public List<PlotSetSaveData> plotSets = new List<PlotSetSaveData>();
}

[Serializable]
public class PlotSetSaveData
{
    public List<string> plotIds = new List<string>();
    public List<bool> usedSlots = new List<bool>();
}

[Serializable]
public class FieldItemSaveData
{
    public string itemId;
    public SerializableVector3 position = new SerializableVector3();
    public float rotationZ;
}

[Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3()
    {
    }

    public SerializableVector3(Vector3 value)
    {
        x = value.x;
        y = value.y;
        z = value.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public static SerializableVector3 FromVector3(Vector3 value)
    {
        return new SerializableVector3(value);
    }
}
