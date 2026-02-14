using UnityEngine;

public enum ItemGrade { Common, Rare }
public enum ItemExpirationType { Conclave, Day, Permanent, Special }
public enum ItemUsageType
{
    Passive, // 자동 (소지 시 효과)
    Active   // 수동 (사용 시 효과)
}

public abstract class Item : ScriptableObject
{
    [SerializeField] public string itemID;
    [SerializeField] public string itemName;
    [TextArea] public string itemDescription;
    [SerializeField] public Sprite itemImage;
    [SerializeField] public ItemGrade itemGrade;
    [SerializeField] public ItemExpirationType itemExpirationType;

    [Header("사용 타입")]
    [SerializeField] public ItemUsageType usageType;

    // 공통
    public virtual void OnAcquire() { }
    public virtual void OnRemove() { }

    // 사용
    public virtual void OnUse() { }

    // 이벤트
    public virtual void OnPray(Cardinal owner) { }
    public virtual void OnSpeech(Cardinal owner) { }

}
