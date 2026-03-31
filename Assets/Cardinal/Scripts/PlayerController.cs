using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, ICardinalController
{
    [Header("Interaction Settings")]
    [Tooltip("기도 대기열에 등록할 키 설정")]
    [SerializeField] private Key interactKey = Key.F;

    [Tooltip("연설(Speech) 키 (기본 G)")]
    [SerializeField] private Key speechKey = Key.G;

    [Tooltip("상호작용 쿨다운 (초)")]
    [SerializeField] private float interactCooldown = 5.0f;

    [Tooltip("Gamsil 매니저 참조")]
    [SerializeField] private Gamsil gamsilManager;
    [SerializeField] private Lecture lectureManager;

    private float currentCooldownTimer = 0f;
    private Vector2? targetPos;
    private StateController myStateController;

    private void Awake()
    {
        myStateController = GetComponent<StateController>();
    }

    private void Start()
    {
        if (gamsilManager == null)
        {
            gamsilManager = FindAnyObjectByType<Gamsil>();
        }

        if (lectureManager == null)
        {
            lectureManager = FindAnyObjectByType<Lecture>();
        }
    }

    private void Update()
    {
        if (currentCooldownTimer > 0)
        {
            currentCooldownTimer -= Time.deltaTime;
        }

        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null || myStateController == null) return;

        bool canAcceptManualInteraction = myStateController.CanAcceptManualInteraction();
        bool canCancelActionMovement = myStateController.IsActionMovementInProgress;
        bool shouldCaptureMoveInput = canAcceptManualInteraction || canCancelActionMovement;
        bool isMovingInput = false;

        if (mouse.leftButton.wasPressedThisFrame && shouldCaptureMoveInput)
        {
            Vector2 screenPos = mouse.position.ReadValue();
            Vector3 world = Camera.main.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z));
            targetPos = new Vector2(world.x, world.y);
            isMovingInput = true;
        }

        if (shouldCaptureMoveInput &&
            (keyboard.wKey.isPressed || keyboard.sKey.isPressed || keyboard.aKey.isPressed || keyboard.dKey.isPressed ||
             keyboard.upArrowKey.isPressed || keyboard.downArrowKey.isPressed ||
             keyboard.leftArrowKey.isPressed || keyboard.rightArrowKey.isPressed))
        {
            isMovingInput = true;
        }

        if (canCancelActionMovement && isMovingInput)
        {
            if (myStateController.IsHeadingToQueue)
            {
                gamsilManager?.CancelPlayerRegistration(myStateController);
            }
            else if (myStateController.IsHeadingToSpeech)
            {
                lectureManager?.CancelPlayerRegistration(myStateController);
            }

            return;
        }

        if (!canAcceptManualInteraction || myStateController.CurrentState != CardinalState.Idle)
        {
            return;
        }

        if (keyboard[interactKey].wasPressedThisFrame)
        {
            if (!myStateController.IsHeadingToQueue)
            {
                if (currentCooldownTimer <= 0)
                {
                    RequestPrayerEntry();
                }
                else
                {
                    Debug.Log($"쿨다운 중입니다. {currentCooldownTimer:F1}초 남음");
                }
            }
        }

        if (keyboard[speechKey].wasPressedThisFrame)
        {
            if (!myStateController.IsHeadingToQueue && !myStateController.IsHeadingToSpeech)
            {
                if (currentCooldownTimer <= 0 && lectureManager != null)
                {
                    lectureManager.RegisterPlayerToQueue(myStateController);
                    currentCooldownTimer = interactCooldown;
                }
            }
        }
    }

    private void RequestPrayerEntry()
    {
        if (gamsilManager == null || myStateController == null) return;

        if (myStateController.CanAcceptManualInteraction())
        {
            gamsilManager.RegisterPlayerToQueue(myStateController);
            currentCooldownTimer = interactCooldown;
        }
    }

    public CardinalInputData GetInput()
    {
        CardinalInputData inputData = new CardinalInputData { targetPos = this.targetPos };
        targetPos = null;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            Vector2 moveDir = Vector2.zero;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveDir.y += 1;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveDir.y -= 1;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveDir.x -= 1;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveDir.x += 1;

            inputData.moveDirection = moveDir.normalized;
        }
        return inputData;
    }
}
