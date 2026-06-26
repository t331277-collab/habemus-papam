using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VoteButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Outline outline;

    [SerializeField] private Color normalColor = Color.clear;
    [SerializeField] private Color hoverColor = Color.yellow;

    [SerializeField] private Vector2 normalDistance = Vector2.zero;
    [SerializeField] private Vector2 hoverDistance = new Vector2(3f, -3f);

    private void Awake()
    {
        if (outline == null)
            outline = GetComponent<Outline>();

        HideOutline();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowOutline();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideOutline();
    }

    private void ShowOutline()
    {
        if (outline == null) return;

        outline.enabled = true;
        outline.effectColor = hoverColor;
        outline.effectDistance = hoverDistance;
    }

    private void HideOutline()
    {
        if (outline == null) return;

        outline.effectColor = normalColor;
        outline.effectDistance = normalDistance;
        outline.enabled = false;
    }
}