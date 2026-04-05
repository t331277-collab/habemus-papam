using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    [SerializeField] private GameObject startGameWarningPopup;
    [SerializeField] private GameObject loadWarningPopup;
    [SerializeField] private GameObject loadPopup;
    [SerializeField] private Component loadPlayerHpText;
    [SerializeField] private Component loadPlayerInfluenceText;
    [SerializeField] private Component loadPlayerPietyText;
    [SerializeField] private Component loadDayText;
    [SerializeField] private Component loadConclaveText;
    [SerializeField] private GameObject popeListPopup;
    [SerializeField] private Image popeListMiddleFrame;
    [SerializeField] private Image popeListLeftFrame;
    [SerializeField] private GameObject popeListCreditObject;
    [SerializeField] private Button popeListLeftArrowButton;
    [SerializeField] private Button popeListRightArrowButton;
    [SerializeField] private Button popeListBackButton;
    [SerializeField] private List<Selectable> popeListBlockedSelectables = new();
    [SerializeField] private int popeListInitialFrame = 1;
    [SerializeField] private int popeListMaxFrame = 100;
    [SerializeField] private string popeListFrameLabelFormat = "{0}frame";
    [SerializeField] private List<Button> navigationButtons = new();
    [SerializeField] private Color selectedOutlineColor = Color.black;
    [SerializeField] private Vector2 selectedOutlineDistance = new Vector2(8f, -8f);
    [SerializeField] private bool selectedOutlineUseGraphicAlpha = false;
    [SerializeField] private bool wrapNavigation = true;

    private int currentNavigationIndex = -1;
    private bool cachedSendNavigationEvents;
    private bool hasCachedSendNavigationEvents;
    private Button highlightedButton;
    private Outline highlightedOutline;
    private readonly Dictionary<Button, Outline> buttonOutlines = new();
    private readonly Dictionary<Selectable, bool> popeListSelectableInteractableStates = new();
    private Image popeListRightFrame;
    private int currentPopeFrame = 1;
    private bool popeListVisualsInitialized;

    private void Awake()
    {
        SetStartGameWarningPopup(false);
        SetLoadWarningPopup(false);
        SetLoadPopup(false);
        SetPopeListPopup(false);
    }

    private void Start()
    {
        EnsureEventSystemNavigationDisabled();
        RefreshNavigation(true);
    }

    private void Update()
    {
        EnsureEventSystemNavigationDisabled();
        RefreshNavigation(false);
        HandleNavigationInput();
    }

    private void OnDisable()
    {
        ClearSelectionHighlight();
        ApplyPopeListMouseOnlyMode(false);
        RestoreEventSystemNavigation();
    }

    public void OnClickStartGame()
    {
        if (SaveManager.Instance == null)
        {
            return;
        }

        if (SaveManager.Instance.HasSave())
        {
            SetStartGameWarningPopup(true);
            return;
        }

        SaveManager.Instance.StartNewGame();
    }

    public void OnClickConfirmStartGame()
    {
        SetStartGameWarningPopup(false);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.StartNewGame();
        }
    }

    public void OnClickCancelStartGame()
    {
        SetStartGameWarningPopup(false);
    }

    public void OnClickLoad()
    {
        if (SaveManager.Instance == null)
        {
            return;
        }

        SetLoadWarningPopup(false);
        SetLoadPopup(false);

        if (!SaveManager.Instance.HasSave())
        {
            SetLoadWarningPopup(true);
            return;
        }

        if (SaveManager.Instance.TryGetSavePreview(out SavePreviewData preview))
        {
            ApplyLoadPreview(preview);
            SetLoadPopup(true);
            return;
        }

        SetLoadWarningPopup(true);
    }

    public void OnClickPlayLoadGame()
    {
        SetLoadPopup(false);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
    }

    public void OnClickCloseLoadPopup()
    {
        SetLoadPopup(false);
    }

    public void OnClickCloseLoadWarningPopup()
    {
        SetLoadWarningPopup(false);
    }

    public void OnClickOpenPopeListPopup()
    {
        SetPopeListPopup(true);
    }

    public void OnClickClosePopeListPopup()
    {
        SetPopeListPopup(false);
    }

    public void OnClickPopeListLeftArrow()
    {
        MovePopeFrame(-1);
    }

    public void OnClickPopeListRightArrow()
    {
        MovePopeFrame(1);
    }

    private void SetStartGameWarningPopup(bool isActive)
    {
        if (startGameWarningPopup != null)
        {
            startGameWarningPopup.SetActive(isActive);
        }

        RefreshNavigation(false);
    }

    private void SetLoadWarningPopup(bool isActive)
    {
        if (loadWarningPopup != null)
        {
            loadWarningPopup.SetActive(isActive);
        }

        RefreshNavigation(false);
    }

    private void SetLoadPopup(bool isActive)
    {
        if (loadPopup != null)
        {
            loadPopup.SetActive(isActive);
        }

        RefreshNavigation(false);
    }

    private void SetPopeListPopup(bool isActive)
    {
        if (isActive)
        {
            InitializePopeListVisuals();
            currentPopeFrame = Mathf.Clamp(popeListInitialFrame, 1, Mathf.Max(1, popeListMaxFrame));
            ApplyPopeListMouseOnlyMode(true);
        }
        else
        {
            ApplyPopeListMouseOnlyMode(false);
            HidePopeListVisuals();
        }

        if (popeListPopup != null)
        {
            popeListPopup.SetActive(isActive);
        }

        if (isActive)
        {
            RefreshPopeListVisuals();
        }

        RefreshNavigation(false);
    }

    private void ApplyLoadPreview(SavePreviewData preview)
    {
        if (preview == null)
        {
            return;
        }

        SetText(loadPlayerHpText, FormatStatValue(preview.playerHp));
        SetText(loadPlayerInfluenceText, FormatStatValue(preview.playerInfluence));
        SetText(loadPlayerPietyText, FormatStatValue(preview.playerPiety));
        SetText(loadDayText, preview.day.ToString());
        SetText(loadConclaveText, preview.conclaveName);
    }

    private string FormatStatValue(float value)
    {
        return value.ToString("0.#", CultureInfo.InvariantCulture);
    }

    private void SetText(Component textComponent, string value)
    {
        Component resolvedComponent = ResolveTextComponent(textComponent);
        if (resolvedComponent == null)
        {
            return;
        }

        if (resolvedComponent is TMP_Text tmpText)
        {
            tmpText.text = value;
            return;
        }

        if (resolvedComponent is Text legacyText)
        {
            legacyText.text = value;
        }
    }

    private Component ResolveTextComponent(Component sourceComponent)
    {
        if (sourceComponent == null)
        {
            return null;
        }

        if (sourceComponent is TMP_Text || sourceComponent is Text)
        {
            return sourceComponent;
        }

        TMP_Text tmpText = sourceComponent.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            return tmpText;
        }

        Text legacyText = sourceComponent.GetComponent<Text>();
        if (legacyText != null)
        {
            return legacyText;
        }

        return sourceComponent.GetComponentInChildren<TMP_Text>(true) as Component
            ?? sourceComponent.GetComponentInChildren<Text>(true);
    }

    private void HandleNavigationInput()
    {
        if (IsNavigationBlocked())
        {
            return;
        }

        if (WasMoveLeftPressed())
        {
            MoveSelection(-1);
        }
        else if (WasMoveRightPressed())
        {
            MoveSelection(1);
        }

        if (WasSubmitPressed())
        {
            ActivateCurrentButton();
        }
    }

    private void MoveSelection(int direction)
    {
        if (direction == 0 || navigationButtons == null || navigationButtons.Count == 0)
        {
            return;
        }

        if (!IsNavigationIndexEligible(currentNavigationIndex))
        {
            RefreshNavigation(true);
            return;
        }

        int nextIndex = currentNavigationIndex;

        for (int i = 0; i < navigationButtons.Count; i++)
        {
            nextIndex += direction;

            if (wrapNavigation)
            {
                nextIndex = WrapIndex(nextIndex, navigationButtons.Count);
            }
            else if (nextIndex < 0 || nextIndex >= navigationButtons.Count)
            {
                return;
            }

            if (IsNavigationIndexEligible(nextIndex))
            {
                currentNavigationIndex = nextIndex;
                UpdateSelectionVisual();
                return;
            }
        }
    }

    private void ActivateCurrentButton()
    {
        Button currentButton = GetCurrentNavigationButton();
        if (currentButton == null)
        {
            return;
        }

        currentButton.onClick.Invoke();
        RefreshNavigation(false);
    }

    private void RefreshNavigation(bool forceSelectFirstButton)
    {
        if (IsNavigationBlocked())
        {
            ClearSelectionHighlight();
            return;
        }

        int firstEligibleIndex = FindFirstEligibleNavigationIndex();
        if (firstEligibleIndex < 0)
        {
            currentNavigationIndex = -1;
            ClearSelectionHighlight();
            return;
        }

        if (forceSelectFirstButton || !IsNavigationIndexEligible(currentNavigationIndex))
        {
            currentNavigationIndex = firstEligibleIndex;
        }

        UpdateSelectionVisual();
    }

    private int FindFirstEligibleNavigationIndex()
    {
        if (navigationButtons == null)
        {
            return -1;
        }

        for (int i = 0; i < navigationButtons.Count; i++)
        {
            if (IsNavigationIndexEligible(i))
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsNavigationIndexEligible(int index)
    {
        if (navigationButtons == null || index < 0 || index >= navigationButtons.Count)
        {
            return false;
        }

        Button button = navigationButtons[index];
        if (button == null || !button.gameObject.activeInHierarchy || !button.interactable)
        {
            return false;
        }

        return true;
    }

    private Button GetCurrentNavigationButton()
    {
        if (!IsNavigationIndexEligible(currentNavigationIndex))
        {
            return null;
        }

        return navigationButtons[currentNavigationIndex];
    }

    private void UpdateSelectionVisual()
    {
        Button currentButton = GetCurrentNavigationButton();
        if (currentButton == null)
        {
            ClearSelectionHighlight();
            return;
        }

        Outline targetOutline = GetOrCreateOutline(currentButton);
        if (targetOutline == null)
        {
            ClearSelectionHighlight();
            return;
        }

        if (highlightedButton == currentButton && highlightedOutline == targetOutline)
        {
            if (!targetOutline.enabled)
            {
                targetOutline.enabled = true;
            }

            ApplyOutlineStyle(targetOutline);
            return;
        }

        ClearSelectionHighlight();
        ApplyOutlineStyle(targetOutline);
        targetOutline.enabled = true;
        highlightedButton = currentButton;
        highlightedOutline = targetOutline;
    }

    private void ClearSelectionHighlight()
    {
        if (highlightedOutline != null)
        {
            highlightedOutline.enabled = false;
        }

        highlightedButton = null;
        highlightedOutline = null;
    }

    private bool IsNavigationBlocked()
    {
        return IsPopupOpen(startGameWarningPopup)
            || IsPopupOpen(loadWarningPopup)
            || IsPopupOpen(loadPopup)
            || IsPopupOpen(popeListPopup);
    }

    private static bool IsPopupOpen(GameObject popup)
    {
        return popup != null && popup.activeInHierarchy;
    }

    private Outline GetOrCreateOutline(Button button)
    {
        if (button == null)
        {
            return null;
        }

        if (buttonOutlines.TryGetValue(button, out Outline cachedOutline) && cachedOutline != null)
        {
            return cachedOutline;
        }

        Graphic targetGraphic = GetButtonGraphic(button);
        if (targetGraphic == null)
        {
            return null;
        }

        Outline outline = targetGraphic.GetComponent<Outline>();
        if (outline == null)
        {
            outline = targetGraphic.gameObject.AddComponent<Outline>();
        }

        ApplyOutlineStyle(outline);
        outline.enabled = false;
        buttonOutlines[button] = outline;
        return outline;
    }

    private void ApplyOutlineStyle(Outline outline)
    {
        if (outline == null)
        {
            return;
        }

        outline.effectColor = selectedOutlineColor;
        outline.effectDistance = selectedOutlineDistance;
        outline.useGraphicAlpha = selectedOutlineUseGraphicAlpha;
    }

    private static Graphic GetButtonGraphic(Button button)
    {
        if (button == null)
        {
            return null;
        }

        if (button.targetGraphic != null)
        {
            return button.targetGraphic;
        }

        return button.GetComponent<Graphic>();
    }

    private void MovePopeFrame(int direction)
    {
        if (direction == 0 || !IsPopupOpen(popeListPopup))
        {
            return;
        }

        int maxFrame = Mathf.Max(1, popeListMaxFrame);
        int nextFrame = Mathf.Clamp(currentPopeFrame + direction, 1, maxFrame);
        if (nextFrame == currentPopeFrame)
        {
            return;
        }

        currentPopeFrame = nextFrame;
        RefreshPopeListVisuals();
    }

    private void InitializePopeListVisuals()
    {
        if (popeListVisualsInitialized)
        {
            return;
        }

        if (popeListLeftFrame != null && popeListCreditObject != null)
        {
            Transform creditTransform = popeListCreditObject.transform;
            GameObject rightFrameObject = Instantiate(
                popeListLeftFrame.gameObject,
                creditTransform.parent);
            rightFrameObject.name = $"{popeListLeftFrame.gameObject.name}_RightRuntime";

            RectTransform sourceRect = popeListCreditObject.transform as RectTransform;
            RectTransform targetRect = rightFrameObject.transform as RectTransform;
            if (sourceRect != null && targetRect != null)
            {
                CopyRectTransform(sourceRect, targetRect);
            }

            rightFrameObject.transform.SetSiblingIndex(creditTransform.GetSiblingIndex());
            popeListRightFrame = rightFrameObject.GetComponent<Image>();
            if (popeListRightFrame != null)
            {
                popeListRightFrame.gameObject.SetActive(false);
            }
        }

        popeListVisualsInitialized = true;
    }

    private void RefreshPopeListVisuals()
    {
        int maxFrame = Mathf.Max(1, popeListMaxFrame);
        currentPopeFrame = Mathf.Clamp(currentPopeFrame, 1, maxFrame);

        SetFrameSlot(popeListMiddleFrame, currentPopeFrame, true);
        SetFrameSlot(
            popeListLeftFrame,
            currentPopeFrame + 1,
            currentPopeFrame < maxFrame);

        bool showCredit = currentPopeFrame <= 1;
        if (popeListCreditObject != null)
        {
            popeListCreditObject.SetActive(showCredit);
        }

        SetFrameSlot(
            popeListRightFrame,
            currentPopeFrame - 1,
            !showCredit);

        UpdatePopeListArrowState();
    }

    private void HidePopeListVisuals()
    {
        SetFrameSlot(popeListMiddleFrame, currentPopeFrame, false);
        SetFrameSlot(popeListLeftFrame, currentPopeFrame + 1, false);
        SetFrameSlot(popeListRightFrame, currentPopeFrame - 1, false);

        if (popeListCreditObject != null)
        {
            popeListCreditObject.SetActive(false);
        }
    }

    private void SetFrameSlot(Image frameImage, int frameNumber, bool isVisible)
    {
        if (frameImage == null)
        {
            return;
        }

        frameImage.gameObject.SetActive(isVisible);
        if (!isVisible)
        {
            return;
        }

        SetFrameLabel(frameImage.transform, frameNumber);
    }

    private void SetFrameLabel(Transform frameRoot, int frameNumber)
    {
        if (frameRoot == null)
        {
            return;
        }

        string format = string.IsNullOrWhiteSpace(popeListFrameLabelFormat)
            ? "{0}"
            : popeListFrameLabelFormat;
        string label = string.Format(format, frameNumber);

        TMP_Text tmpText = frameRoot.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null)
        {
            tmpText.text = label;
            return;
        }

        Text legacyText = frameRoot.GetComponentInChildren<Text>(true);
        if (legacyText != null)
        {
            legacyText.text = label;
        }
    }

    private void UpdatePopeListArrowState()
    {
        int maxFrame = Mathf.Max(1, popeListMaxFrame);

        if (popeListLeftArrowButton != null)
        {
            popeListLeftArrowButton.interactable = currentPopeFrame > 1;
        }

        if (popeListRightArrowButton != null)
        {
            popeListRightArrowButton.interactable = currentPopeFrame < maxFrame;
        }

        if (popeListBackButton != null)
        {
            popeListBackButton.interactable = true;
        }
    }

    private void ApplyPopeListMouseOnlyMode(bool isPopupOpen)
    {
        if (isPopupOpen)
        {
            popeListSelectableInteractableStates.Clear();

            foreach (Selectable selectable in GetPopeListBlockedSelectables())
            {
                if (selectable == null)
                {
                    continue;
                }

                if (!popeListSelectableInteractableStates.ContainsKey(selectable))
                {
                    popeListSelectableInteractableStates[selectable] = selectable.interactable;
                }

                selectable.interactable = false;
            }

            return;
        }

        foreach (KeyValuePair<Selectable, bool> entry in popeListSelectableInteractableStates)
        {
            if (entry.Key != null)
            {
                entry.Key.interactable = entry.Value;
            }
        }

        popeListSelectableInteractableStates.Clear();
    }

    private List<Selectable> GetPopeListBlockedSelectables()
    {
        List<Selectable> results = new();

        AddUniqueSelectable(results, popeListBlockedSelectables);

        if (navigationButtons != null)
        {
            foreach (Button navigationButton in navigationButtons)
            {
                AddUniqueSelectable(results, navigationButton);
            }
        }

        results.Remove(popeListLeftArrowButton);
        results.Remove(popeListRightArrowButton);
        results.Remove(popeListBackButton);

        return results;
    }

    private static void AddUniqueSelectable(List<Selectable> targets, IEnumerable<Selectable> source)
    {
        if (targets == null || source == null)
        {
            return;
        }

        foreach (Selectable selectable in source)
        {
            AddUniqueSelectable(targets, selectable);
        }
    }

    private static void AddUniqueSelectable(List<Selectable> targets, Selectable selectable, bool shouldAdd = true)
    {
        if (!shouldAdd || targets == null || selectable == null || targets.Contains(selectable))
        {
            return;
        }

        targets.Add(selectable);
    }

    private static void CopyRectTransform(RectTransform source, RectTransform destination)
    {
        if (source == null || destination == null)
        {
            return;
        }

        destination.anchorMin = source.anchorMin;
        destination.anchorMax = source.anchorMax;
        destination.anchoredPosition = source.anchoredPosition;
        destination.sizeDelta = source.sizeDelta;
        destination.pivot = source.pivot;
        destination.localRotation = source.localRotation;
        destination.localScale = source.localScale;
    }

    private void EnsureEventSystemNavigationDisabled()
    {
        if (hasCachedSendNavigationEvents || EventSystem.current == null)
        {
            return;
        }

        cachedSendNavigationEvents = EventSystem.current.sendNavigationEvents;
        hasCachedSendNavigationEvents = true;
        EventSystem.current.sendNavigationEvents = false;
    }

    private void RestoreEventSystemNavigation()
    {
        if (!hasCachedSendNavigationEvents || EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.sendNavigationEvents = cachedSendNavigationEvents;
        hasCachedSendNavigationEvents = false;
    }

    private static int WrapIndex(int index, int count)
    {
        if (count <= 0)
        {
            return -1;
        }

        int wrappedIndex = index % count;
        return wrappedIndex < 0 ? wrappedIndex + count : wrappedIndex;
    }

    private static bool WasMoveLeftPressed()
    {
        return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
    }

    private static bool WasMoveRightPressed()
    {
        return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
    }

    private static bool WasSubmitPressed()
    {
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    }
}
