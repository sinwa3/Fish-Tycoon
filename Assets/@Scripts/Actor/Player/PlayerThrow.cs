using System.Collections;
using UnityEngine;

/// <summary> YSW : 던질 수 있는 물건을 하나 들고, 던지거나 내려놓는다. </summary>
public class PlayerThrow : MonoBehaviour
{
    #region ===== 설정 =====

    private const float DropRayStartHeight = 1f;
    private const float DropRayLength = 3f;
    private const float DropWallMargin = 0.2f;
    private const float DropFloorOffset = 0.05f;

    [Header("참조")]
    [Tooltip("던지는 방향의 기준이 될 카메라. 비워두면 Camera.main을 사용한다.")]
    [SerializeField] private Transform _cameraTransform;

    [Tooltip("물건을 들고 있을 위치. 비워두면 카메라 오른쪽 아래에 자동으로 만든다.")]
    [SerializeField] private Transform _holdPoint;

    [SerializeField] private Vector3 _defaultHoldOffset = new Vector3(0.3f, -0.3f, 0.6f);

    [Header("던지기")]
    [Tooltip("카메라 정면으로 던지는 속도")]
    [SerializeField] private float _throwSpeed = 10f;

    [Tooltip("포물선을 만들기 위해 위쪽으로 더하는 속도")]
    [SerializeField] private float _throwUpSpeed = 2f;

    [Tooltip("던진 직후 플레이어와의 충돌을 무시하는 시간(초)")]
    [SerializeField] private float _ignorePlayerCollisionTime = 0.3f;

    [Header("내려놓기")]
    [Tooltip("플레이어 앞 몇 m에 내려놓을지")]
    [SerializeField] private float _dropDistance = 0.8f;

    [Tooltip("바닥을 찾을 때 검사할 레이어")]
    [SerializeField] private LayerMask _dropLayers = ~0;

    [Header("디버그")]
    [Tooltip("UI가 없을 때 화면에 조작 안내를 임시로 표시한다.")]
    [SerializeField] private bool _showDebugPrompt = true;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private CharacterController _characterController;
    private PlayerHide _playerHide;
    private GUIStyle _debugPromptStyle;

    /// <summary> YSW : 현재 들고 있는 물건 (없으면 null) </summary>
    public ThrowableItem HeldItem { get; private set; }

    /// <summary> YSW : 물건을 들고 있는지 여부 </summary>
    public bool IsHolding => HeldItem != null;

    private bool IsHiding => _playerHide != null && _playerHide.IsHiding;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeCamera();
        InitializeHoldPoint();
    }

    private void Update()
    {
        UpdateThrowInput();
    }

    private void OnGUI()
    {
        DrawDebugPrompt();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 던지기에 필요한 컴포넌트를 가져온다. </summary>
    private void InitializeComponents()
    {
        _characterController = GetComponent<CharacterController>();
        _playerHide = GetComponent<PlayerHide>();
    }

    /// <summary> YSW : 던지는 방향의 기준이 될 카메라를 설정한다. </summary>
    private void InitializeCamera()
    {
        if (_cameraTransform == null && Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        if (_cameraTransform == null)
        {
            CPrint.Warning($"{nameof(PlayerThrow)} : 카메라를 찾을 수 없습니다.");
        }
    }

    /// <summary> YSW : 손 위치가 없으면 카메라 자식으로 만든다. </summary>
    private void InitializeHoldPoint()
    {
        if (_holdPoint != null || _cameraTransform == null)
        {
            return;
        }

        GameObject holdPointObject = new GameObject("HoldPoint");

        _holdPoint = holdPointObject.transform;
        _holdPoint.SetParent(_cameraTransform, false);
        _holdPoint.localPosition = _defaultHoldOffset;
    }

    #endregion ===== 초기화 =====

    #region ===== 입력 처리 =====

    /// <summary> YSW : 좌클릭으로 던지고, 우클릭으로 내려놓는다. (숨어 있을 때는 불가) </summary>
    private void UpdateThrowInput()
    {
        InputManager input = Managers.Input;

        if (input == null || !input.CanReceivePlayer || !IsHolding || IsHiding)
        {
            return;
        }

        if (input.MouseDown_Left)
        {
            ThrowHeldItem();

            return;
        }

        if (input.MouseDown_Right)
        {
            DropHeldItem();
        }
    }

    #endregion ===== 입력 처리 =====

    #region ===== 들기/던지기/내려놓기 =====

    /// <summary> YSW : 지정한 물건을 든다. 이미 들고 있으면 들고 있던 물건을 내려놓고 바꿔 든다. </summary>
    public void PickUp(ThrowableItem item)
    {
        if (item == null || item == HeldItem || _holdPoint == null)
        {
            return;
        }

        if (IsHolding)
        {
            DropHeldItem();
        }

        HeldItem = item;
        HeldItem.OnPickedUp(_holdPoint);
    }

    /// <summary> YSW : 들고 있는 물건을 카메라 정면으로 던진다. </summary>
    private void ThrowHeldItem()
    {
        if (!IsHolding || _cameraTransform == null)
        {
            return;
        }

        ThrowableItem item = HeldItem;
        HeldItem = null;

        Vector3 velocity = _cameraTransform.forward * _throwSpeed + Vector3.up * _throwUpSpeed;
        item.OnThrown(velocity);

        IgnorePlayerCollision(item);
    }

    /// <summary> YSW : 들고 있는 물건을 플레이어 앞 바닥에 내려놓는다. </summary>
    private void DropHeldItem()
    {
        if (!IsHolding)
        {
            return;
        }

        ThrowableItem item = HeldItem;
        HeldItem = null;

        item.OnDropped(GetDropPosition());
    }

    /// <summary> YSW : 플레이어 앞의 벽을 피해 바닥 위 내려놓을 위치를 계산한다. </summary>
    private Vector3 GetDropPosition()
    {
        Vector3 chestPosition = transform.position + Vector3.up * DropRayStartHeight;
        float distance = _dropDistance;

        if (Physics.Raycast(chestPosition, transform.forward, out RaycastHit wallHit, _dropDistance, _dropLayers, QueryTriggerInteraction.Ignore))
        {
            distance = Mathf.Max(0f, wallHit.distance - DropWallMargin);
        }

        Vector3 dropOrigin = chestPosition + transform.forward * distance;

        if (Physics.Raycast(dropOrigin, Vector3.down, out RaycastHit floorHit, DropRayLength, _dropLayers, QueryTriggerInteraction.Ignore))
        {
            return floorHit.point + Vector3.up * DropFloorOffset;
        }

        return dropOrigin;
    }

    #endregion ===== 들기/던지기/내려놓기 =====

    #region ===== 충돌 처리 =====

    /// <summary> YSW : 던진 직후 잠시 플레이어와 물건의 충돌을 무시한다. </summary>
    private void IgnorePlayerCollision(ThrowableItem item)
    {
        if (_characterController == null)
        {
            return;
        }

        SetPlayerCollisionIgnored(item.Colliders, true);

        StartCoroutine(RestorePlayerCollisionRoutine(item.Colliders));
    }

    /// <summary> YSW : 일정 시간 뒤 플레이어와 물건의 충돌을 다시 켠다. </summary>
    private IEnumerator RestorePlayerCollisionRoutine(Collider[] itemColliders)
    {
        yield return new WaitForSeconds(_ignorePlayerCollisionTime);

        SetPlayerCollisionIgnored(itemColliders, false);
    }

    /// <summary> YSW : 플레이어와 지정한 충돌체들의 충돌 무시 여부를 설정한다. </summary>
    private void SetPlayerCollisionIgnored(Collider[] itemColliders, bool isIgnored)
    {
        if (itemColliders == null || _characterController == null)
        {
            return;
        }

        foreach (Collider itemCollider in itemColliders)
        {
            if (itemCollider == null)
            {
                continue;
            }

            Physics.IgnoreCollision(itemCollider, _characterController, isIgnored);
        }
    }

    #endregion ===== 충돌 처리 =====

    #region ===== 디버그 =====

    /// <summary> YSW : UI가 없을 때 화면 중앙 아래에 던지기 조작 안내를 임시로 표시한다. </summary>
    private void DrawDebugPrompt()
    {
        if (!_showDebugPrompt || !IsHolding)
        {
            return;
        }

        if (_debugPromptStyle == null)
        {
            _debugPromptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
            };
        }

        string promptText = IsHiding ? $"{HeldItem.name} 들고 있음" : $"[좌클릭] 던지기   [우클릭] 내려놓기   ({HeldItem.name})";

        Rect rect = new Rect(0f, Screen.height * 0.68f, Screen.width, 40f);
        GUI.Label(rect, promptText, _debugPromptStyle);
    }

    #endregion ===== 디버그 =====
}
