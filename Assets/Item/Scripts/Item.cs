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
    [TextArea] public string itemEffectDescription;
    [SerializeField] public Sprite itemImage;
    

    [Header("사용 타입")]
    [SerializeField] public ItemGrade itemGrade;
    [SerializeField] public ItemExpirationType itemExpirationType;
    [SerializeField] public ItemUsageType usageType;

    // 공통
    public virtual void OnAcquire() { }
    public virtual void OnRemove() { }

    // 사용
    public virtual void OnUse() { }

    // 이벤트
    public virtual void OnPray(Cardinal owner) { }
    public virtual void OnSpeech(Cardinal owner) { }

    public string GetColoredGrade()
    {
        string colorHex = "";
        string gradeText = "";

        switch (itemGrade)
        {
            case ItemGrade.Common:
                colorHex = "#4488FF"; 
                gradeText = "일반";
                break;
            case ItemGrade.Rare:
                colorHex = "#9B30FF"; 
                gradeText = "레어";
                break;
            default:
                colorHex = "#FFFFFF"; 
                gradeText = "기타";
                break;
        }

        return $"<color={colorHex}>{gradeText}</color>";
    }

}
