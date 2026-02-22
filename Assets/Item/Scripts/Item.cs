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
    public virtual bool IsDurationBuff => false;
    public virtual bool ConsumeOnUse => true;


    // 공통
    public virtual void OnAcquire() { }
    public virtual void OnRemove() { }

    // 사용
    public virtual void OnUse() { }

    // 이벤트
    public virtual void OnPray(Cardinal owner) { }
    public virtual void OnSpeech(Cardinal owner) { }

    public virtual void OnReapply(Cardinal owner) { } //아이템 효과 다시 발동(패시브 아이템으 경우 콘클라베 다시 시작하면 다시 효과적용)

    public virtual bool OnHpReachedZero(Cardinal owner) { return false; } // 부활 관련 함수
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

    //아이템에서 정치력을 고정으로 올릴때 참조하는 함수
    public virtual float ModifySpeechInfluence(float originalDelta, GameBalance balance, bool isSuccess)
    {
        return originalDelta;
    }

}
