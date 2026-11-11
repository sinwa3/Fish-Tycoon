using System;
using UnityEngine;

/// <summary>
/// YSW : 카메라가 바라보는 상호작용 오브젝트를 찾고 E 키(또는 마우스 왼쪽)로 상호작용한다.
/// 홀드 대상은 키를 일정 시간 누르고 있어야 실행되고, 진행 상황은 HoldingSystem을 통해 UI에 전달한다.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("참조")]
    [Tooltip("레이를 쏠 카메라. 비워두면 Camera.main을 사용한다.")]
    [SerializeField] private Transform _cameraTransform;

    [Header("감지")]
    [SerializeField] private float _interactDistance = 2f;

    [Tooltip("상호작용 대상 레이어")]
    [SerializeField] private LayerMask _interactLayers = LayerKey.Mask.Interaction;
    [Tooltip("상호작용 차폐 레이어")]
    [SerializeField] private LayerMask _obstacleLayers = LayerKey.Mask.Wall | LayerKey.Mask.Door;

    [Header("홀드")]
    [Tooltip("일반 홀드 상호작용(문, 가구, 장롱, 현관문)에 필요한 누르는 시간(초)")]
    [SerializeField, Range(1f, 1.5f)] private float _holdDuration = 1.2f;

    [Tooltip("생선 획득에 필요한 누르는 시간(초)")]
    [SerializeField, Min(0.1f)] private float _fishHoldDuration = 2.5f;

    [Tooltip("장롱에서 나오기에 필요한 누르는 시간(초)")]
    [SerializeField, Min(0.1f)] private float _exitHidingHoldDuration = 0.5f;

    [Tooltip("마우스 왼쪽 버튼으로도 홀드할 수 있게 한다. (물건을 들고 있을 때는 던지기와 겹치므로 E 키만 사용)")]
    [SerializeField] private bool _allowMouseHold = true;

    [Header("안내")]
    [SerializeField] private string _exitHidingText = "나오기";

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private PlayerHide _playerHide;
    private PlayerThrow _playerThrow;
    private HoldingSystem _holdingSystem;

    // 홀드를 누르고 있었던 시간
    private float _holdTimer;

    // 홀드가 끝났거나 대상이 바뀐 뒤, 키를 한 번 뗄 때까지 다시 시작하지 않는다
    private bool _isWaitingRelease;

    /// <summary> YSW : 현재 바라보고 있는 상호작용 가능한 오브젝트 (없으면 null) </summary>
    public IInteractable CurrentTarget { get; private set; }

    /// <summary> YSW : 바라보는 상호작용 대상이 바뀔 때 새 대상을 전달한다. (없으면 null) </summary>
    public event Action<IInteractable> OnTargetChanged;

    private bool IsHiding => _playerHide != null && _playerHide.IsHiding;

    private bool IsHideTransitioning => _playerHide != null && _playerHide.IsTransitioning;

    private bool IsCarryingItem => _playerThrow != null && _playerThrow.IsHolding;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeCamera();
    }

    private void Update()
    {
        UpdateTarget();
        UpdateInteractInput();
        UpdateHoldingDisplay();
    }

    private void OnDisable()
    {
        ResetHold();
        _holdingSystem?.ClearTarget();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 상호작용에 필요한 컴포넌트를 가져온다. </summary>
    private void InitializeComponents()
    {
        _playerHide = GetComponent<PlayerHide>();
        _playerThrow = GetComponent<PlayerThrow>();
    }

    /// <summary> YSW : 레이를 쏠 카메라를 설정한다. </summary>
    private void InitializeCamera()
    {
        if (_cameraTransform == null && Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        if (_cameraTransform == null)
        {
            CPrint.Warning($"{nameof(PlayerInteractor)} : 카메라를 찾을 수 없습니다.");
        }
    }

    /// <summary> YSW : 상호작용 안내와 홀드 진행도를 전달할 홀딩 시스템을 연결한다. (StealthScene이 부른다) </summary>
    public void SetHoldingSystem(HoldingSystem holdingSystem)
    {
        _holdingSystem = holdingSystem;
    }

    #endregion ===== 초기화 =====

    #region ===== 대상 감지 =====

    /// <summary> YSW : 카메라 정면으로 레이를 쏴 상호작용 가능한 대상을 갱신한다. </summary>
    private void UpdateTarget()
    {
        if (IsHiding || _cameraTransform == null)
        {
            SetTarget(null);

            return;
        }

        SetTarget(FindTarget());
    }

    /// <summary> YSW : 레이에 맞은 오브젝트가 상호작용 가능하고 장애물에 가려지지 않았다면 반환한다. </summary>
    private IInteractable FindTarget()
    {
        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);

        // 1. 상호작용 대상 검사
        if (!Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactLayers, QueryTriggerInteraction.Collide))
        {
            return null;
        }

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

        if (interactable == null || !interactable.CanInteract(gameObject))
        {
            return null;
        }

        // 2. 대상까지의 거리에서 장애물 검사
        if (Physics.Raycast(ray, hit.distance, _obstacleLayers, QueryTriggerInteraction.Ignore))
        {
            return null;
        }

        return interactable;
    }

    /// <summary> YSW : 상호작용 대상을 변경하고 변경 이벤트를 알린다. 대상이 바뀌면 홀드를 처음부터 다시 해야 한다. </summary>
    private void SetTarget(IInteractable target)
    {
        if (CurrentTarget == target)
        {
            return;
        }

        CurrentTarget = target;

        ResetHold();
        _isWaitingRelease = IsHoldPressed();

        OnTargetChanged?.Invoke(CurrentTarget);
    }

    #endregion ===== 대상 감지 =====

    #region ===== 입력 처리 =====

    /// <summary> YSW : 숨어 있으면 짧게 홀드해서 나오고, 즉발 대상은 E로 바로 실행하고, 홀드 대상은 누르고 있는 시간을 잰다. </summary>
    private void UpdateInteractInput()
    {
        InputManager input = Managers.Input;

        if (input == null || !input.CanReceivePlayer)
        {
            ResetHold();

            return;
        }

        if (IsHiding)
        {
            if (IsHideTransitioning)
            {
                ResetHold();

                return;
            }

            if (UpdateHold(_exitHidingHoldDuration))
            {
                _playerHide.ExitHiding();
            }

            return;
        }

        if (CurrentTarget == null)
        {
            ResetHold();

            return;
        }

        if (!RequiresHold(CurrentTarget.Type))
        {
            if (input.KeyDown_E)
            {
                CurrentTarget.Interact(gameObject);
            }

            return;
        }

        if (UpdateHold(GetHoldDuration(CurrentTarget.Type)))
        {
            CurrentTarget.Interact(gameObject);
        }
    }

    /// <summary> YSW : 홀드 키를 누르고 있는 동안 시간을 채우고, 지정한 시간이 다 찬 프레임에 true를 반환한다. </summary>
    private bool UpdateHold(float holdDuration)
    {
        if (!IsHoldPressed())
        {
            _isWaitingRelease = false;
            ResetHold();

            return false;
        }

        if (_isWaitingRelease)
        {
            return false;
        }

        _holdTimer += Time.deltaTime;

        if (_holdTimer < holdDuration)
        {
            return false;
        }

        ResetHold();
        _isWaitingRelease = true;

        return true;
    }

    /// <summary> YSW : 홀드 키(E, 또는 물건을 들고 있지 않을 때 마우스 왼쪽)를 누르고 있는지 반환한다. </summary>
    private bool IsHoldPressed()
    {
        InputManager input = Managers.Input;

        if (input == null)
        {
            return false;
        }

        bool isMousePressed = _allowMouseHold && !IsCarryingItem && input.Mouse_Left;

        return input.Key_E || isMousePressed;
    }

    /// <summary> YSW : 홀드 진행 시간을 초기화한다. </summary>
    private void ResetHold()
    {
        _holdTimer = 0f;
    }

    #endregion ===== 입력 처리 =====

    #region ===== 홀드 규칙 =====

    /// <summary> YSW : 홀드로 실행하는 상호작용인지 반환한다. (던지기 물건은 즉발, 장롱에서 나오기는 따로 짧은 홀드) </summary>
    private static bool RequiresHold(InteractionType type)
    {
        switch (type)
        {
            case InteractionType.Door:
            case InteractionType.Furniture:
            case InteractionType.Wardrobe:
            case InteractionType.Fish:
            case InteractionType.ExitDoor:
                return true;

            default:
                return false;
        }
    }

    /// <summary> YSW : 상호작용 종류에 맞는 홀드 시간을 반환한다. (생선 획득만 길게) </summary>
    private float GetHoldDuration(InteractionType type)
    {
        return type == InteractionType.Fish ? _fishHoldDuration : _holdDuration;
    }

    #endregion ===== 홀드 규칙 =====

    #region ===== UI 전달 =====

    /// <summary> YSW : 현재 상호작용 대상의 종류, 안내 문구, 홀드 진행도를 홀딩 시스템에 전달한다. </summary>
    private void UpdateHoldingDisplay()
    {
        if (_holdingSystem == null)
        {
            return;
        }

        if (!CanShowPrompt())
        {
            _holdingSystem.ClearTarget();

            return;
        }

        if (IsHiding)
        {
            _holdingSystem.SetTarget(InteractionType.WardrobeExit, _exitHidingText);
            _holdingSystem.SetProgress(_holdTimer / _exitHidingHoldDuration);

            return;
        }

        if (CurrentTarget == null)
        {
            _holdingSystem.ClearTarget();

            return;
        }

        InteractionType type = CurrentTarget.Type;

        // 생선 아이콘은 넘기지 않는다. (획득 전에는 어떤 생선인지 모르게)
        _holdingSystem.SetTarget(type, CurrentTarget.InteractionText);

        float progress = RequiresHold(type) ? _holdTimer / GetHoldDuration(type) : 0f;
        _holdingSystem.SetProgress(progress);
    }

    /// <summary> YSW : 화면(Screen)이나 팝업이 열려 있지 않고 입력을 받을 수 있을 때만 안내를 보여준다. </summary>
    private bool CanShowPrompt()
    {
        if (Managers.Input == null || !Managers.Input.CanReceivePlayer)
        {
            return false;
        }

        UIManager ui = Managers.UI;

        return ui == null || (ui.CurrentScreen == null && ui.CurrentPopup == null);
    }

    #endregion ===== UI 전달 =====
}
