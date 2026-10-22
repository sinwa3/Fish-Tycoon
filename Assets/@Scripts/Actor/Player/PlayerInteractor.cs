using System;
using UnityEngine;

/// <summary> YSW : 카메라가 바라보는 상호작용 오브젝트를 찾고 E 키로 상호작용한다. </summary>
public class PlayerInteractor : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("참조")]
    [Tooltip("레이를 쏠 카메라. 비워두면 Camera.main을 사용한다.")]
    [SerializeField] private Transform _cameraTransform;

    [Header("감지")]
    [SerializeField] private float _interactDistance = 2f;

    [Tooltip("레이가 검사할 레이어 (벽도 포함해야 벽 너머 오브젝트가 잡히지 않는다)")]
    [SerializeField] private LayerMask _interactLayers = ~0;

    [Header("디버그")]
    [Tooltip("UI가 없을 때 화면에 상호작용 안내를 임시로 표시한다.")]
    [SerializeField] private bool _showDebugPrompt = true;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private PlayerHide _playerHide;
    private GUIStyle _debugPromptStyle;

    /// <summary> YSW : 현재 바라보고 있는 상호작용 가능한 오브젝트 (없으면 null) </summary>
    public IInteractable CurrentTarget { get; private set; }

    /// <summary> YSW : 바라보는 상호작용 대상이 바뀔 때 새 대상을 전달한다. (없으면 null) </summary>
    public event Action<IInteractable> OnTargetChanged;

    private bool IsHiding => _playerHide != null && _playerHide.IsHiding;

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
    }

    private void OnGUI()
    {
        DrawDebugPrompt();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 상호작용에 필요한 컴포넌트를 가져온다. </summary>
    private void InitializeComponents()
    {
        _playerHide = GetComponent<PlayerHide>();
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

    #endregion ===== 초기화 =====

    #region ===== 상호작용 처리 =====

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

    /// <summary> YSW : 레이에 맞은 오브젝트가 상호작용 가능하면 반환한다. </summary>
    private IInteractable FindTarget()
    {
        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactLayers, QueryTriggerInteraction.Ignore))
        {
            return null;
        }

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

        if (interactable == null || !interactable.CanInteract(gameObject))
        {
            return null;
        }

        return interactable;
    }

    /// <summary> YSW : E 키 입력 시 숨어 있으면 나오고, 아니면 바라보는 대상과 상호작용한다. </summary>
    private void UpdateInteractInput()
    {
        InputManager input = Managers.Input;

        if (input == null || !input.CanReceivePlayer || !input.KeyDown_E)
        {
            return;
        }

        if (IsHiding)
        {
            _playerHide.ExitHiding();

            return;
        }

        if (CurrentTarget == null)
        {
            return;
        }

        CurrentTarget.Interact(gameObject);
    }

    /// <summary> YSW : 상호작용 대상을 변경하고 변경 이벤트를 알린다. </summary>
    private void SetTarget(IInteractable target)
    {
        if (CurrentTarget == target)
        {
            return;
        }

        CurrentTarget = target;

        OnTargetChanged?.Invoke(CurrentTarget);
    }

    #endregion ===== 상호작용 처리 =====

    #region ===== 디버그 =====

    /// <summary> YSW : UI가 없을 때 화면 중앙 아래에 상호작용 안내를 임시로 표시한다. </summary>
    private void DrawDebugPrompt()
    {
        if (!_showDebugPrompt)
        {
            return;
        }

        string promptText = GetPromptText();

        if (string.IsNullOrEmpty(promptText))
        {
            return;
        }

        if (_debugPromptStyle == null)
        {
            _debugPromptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
            };
        }

        Rect rect = new Rect(0f, Screen.height * 0.6f, Screen.width, 40f);
        GUI.Label(rect, promptText, _debugPromptStyle);
    }

    /// <summary> YSW : 현재 상태에 맞는 안내 문구를 반환한다. </summary>
    private string GetPromptText()
    {
        if (IsHiding)
        {
            return "[E] 나오기";
        }

        if (CurrentTarget == null)
        {
            return null;
        }

        return $"[E] {CurrentTarget.InteractionText}";
    }

    #endregion ===== 디버그 =====
}
