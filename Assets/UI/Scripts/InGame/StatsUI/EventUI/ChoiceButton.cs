using TMPro;
using UnityEngine;
using UnityEngine.UI;

class ChoiceButton : MonoBehaviour
{
    [SerializeField] Sprite ButtonWithReq;
    [SerializeField] Sprite ButtonWithoutReq;
    Button ButtonKoraji;
    [TextArea] TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI req;

    public void SetButton(string desc, string req = "")
    {
        if(req != "")
        {
            ButtonKoraji.image.sprite = ButtonWithoutReq;
            SetReq(req);
            SetText(desc);
            return;
        }
        ButtonKoraji.image.sprite = ButtonWithReq;
        SetReq("");
        SetText(desc);
    }
    public void DisableButton()
    {
        ButtonKoraji.image.color = new Color(0.5f, 0.5f, 0.5f);
        ButtonKoraji.interactable = false;
    }
    public void SetReq(string eventreq)
    {
        string replaced = eventreq.Replace("정치력", "<sprite name=\"pol\">정치력").
            Replace("경건함", "<sprite name=\"piety\">경건함").
            Replace("체력", "<sprite name=\"hp\">체력").
            Replace("이상", "<sprite name=\"upArrow\">").
            Replace("이하", "<sprite name=\"downArrow\">");
        req.text=replaced;
    }
    public void SetText(string eventdesc)
    {
        text.text = eventdesc;
    }
    public void Clear()
    {
        ButtonKoraji = null;
        text.text = req.text = "";
    }
}