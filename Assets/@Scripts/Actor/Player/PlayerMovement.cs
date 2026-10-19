using UnityEngine;

/// <summary> YSW : WASD 입력으로 플레이어를 바라보는 방향 기준으로 이동시키고 걷기/달리기/웅크리기 상태를 관리한다. </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("이동 속도")]
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _runSpeed = 7f;
    [SerializeField] private float _crouchSpeed = 2f;

    [Header("웅크리기")]
    [Tooltip("웅크렸을 때 서 있을 때 대비 높이 비율")]
    [Range(0.3f, 1f)]
    [SerializeField] private float _crouchHeightRatio = 0.5f;

    [Tooltip("높이 비율이 초당 변하는 양 (클수록 빠르게 앉고 일어선다)")]
    [SerializeField] private float _crouchTransitionSpeed = 6f;

    [Tooltip("일어설 때 머리 위 장애물을 검사할 레이어")]
    [SerializeField] private LayerMask _ceilingLayers = ~0;

    [Header("중력")]
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _groundedStickForce = -2f;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private CharacterController _characterController;
    private PlayerStamina _playerStamina;

    private float _standHeight;
    private Vector3 _standCenter;

    private Vector3 _moveDirection;
    private float _moveSpeed;
    private float _verticalVelocity;

    /// <summary> YSW : 현재 수평 이동 속도 </summary>
    public float CurrentSpeed => _moveDirection.magnitude * _moveSpeed;

    /// <summary> YSW : 수평 이동 속도가 0보다 크면 이동 중으로 판단한다. </summary>
    public bool IsMoving => CurrentSpeed > 0f;

    /// <summary> YSW : 웅크리기 상태 여부 </summary>
    public bool IsCrouching { get; private set; }

    /// <summary> YSW : 실제로 달리고 있는 상태 여부 (이동 중 + Shift + 달리기 가능) </summary>
    public bool IsRunning { get; private set; }

    /// <summary> YSW : 서 있을 때 대비 현재 높이 비율 (서기 1, 웅크리기 _crouchHeightRatio) </summary>
    public float HeightRatio { get; private set; } = 1f;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeStandShape();
    }

    private void Update()
    {
        UpdateCrouchInput();
        UpdateMoveInput();
        UpdateCrouchHeight();
        UpdateGravity();
        UpdateMovement();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 이동에 필요한 컴포넌트를 가져온다. </summary>
    private void InitializeComponents()
    {
        _characterController = GetComponent<CharacterController>();
        _playerStamina = GetComponent<PlayerStamina>();
    }

    /// <summary> YSW : 서 있을 때의 충돌 높이와 중심을 저장한다. </summary>
    private void InitializeStandShape()
    {
        _standHeight = _characterController.height;
        _standCenter = _characterController.center;
    }

    #endregion ===== 초기화 =====

    #region ===== 이동 처리 =====

    /// <summary> YSW : 현재 입력으로 이동 방향과 속도를 갱신한다. </summary>
    private void UpdateMoveInput()
    {
        InputManager input = Managers.Input;

        if (input == null || !input.CanReceivePlayer)
        {
            _moveDirection = Vector3.zero;
            _moveSpeed = 0f;
            IsRunning = false;

            return;
        }

        _moveDirection = GetMoveDirection(input.KeyAxisX, input.KeyAxisY);

        IsRunning = !IsCrouching && input.Key_LeftShift && _moveDirection.sqrMagnitude > 0f && CanRun();
        _moveSpeed = GetMoveSpeed();
    }

    /// <summary> YSW : 입력 축 값을 플레이어가 바라보는 방향 기준으로 변환한다. </summary>
    private Vector3 GetMoveDirection(float axisX, float axisY)
    {
        Vector3 direction = transform.forward * axisY + transform.right * axisX;

        return Vector3.ClampMagnitude(direction, 1f);
    }

    /// <summary> YSW : 스테미너가 있으면 스테미너 상태로, 없으면 항상 달리기 가능으로 판단한다. </summary>
    private bool CanRun()
    {
        return _playerStamina == null || _playerStamina.CanRun;
    }

    /// <summary> YSW : 현재 이동 상태에 맞는 속도를 반환한다. </summary>
    private float GetMoveSpeed()
    {
        if (IsCrouching)
        {
            return _crouchSpeed;
        }

        if (IsRunning)
        {
            return _runSpeed;
        }

        return _walkSpeed;
    }

    /// <summary> YSW : 이동 방향과 중력을 합쳐 캐릭터를 이동시킨다. </summary>
    private void UpdateMovement()
    {
        Vector3 velocity = _moveDirection * _moveSpeed + Vector3.up * _verticalVelocity;

        _characterController.Move(velocity * Time.deltaTime);
    }

    #endregion ===== 이동 처리 =====

    #region ===== 웅크리기 처리 =====

    /// <summary> YSW : Ctrl 입력으로 웅크리기 상태를 토글한다. </summary>
    private void UpdateCrouchInput()
    {
        InputManager input = Managers.Input;

        if (input == null || !input.CanReceivePlayer || !input.KeyDown_LeftCtrl)
        {
            return;
        }

        if (!IsCrouching)
        {
            IsCrouching = true;

            return;
        }

        if (CanStandUp())
        {
            IsCrouching = false;
        }
    }

    /// <summary> YSW : 웅크리기 상태에 맞춰 높이 비율을 부드럽게 변경하고 충돌 크기에 반영한다. </summary>
    private void UpdateCrouchHeight()
    {
        float targetRatio = IsCrouching ? _crouchHeightRatio : 1f;

        if (Mathf.Approximately(HeightRatio, targetRatio))
        {
            return;
        }

        HeightRatio = Mathf.MoveTowards(HeightRatio, targetRatio, _crouchTransitionSpeed * Time.deltaTime);

        ApplyControllerHeight(HeightRatio);
    }

    /// <summary> YSW : 발 위치를 유지한 채 충돌 높이와 중심을 비율에 맞게 변경한다. </summary>
    private void ApplyControllerHeight(float ratio)
    {
        float minHeight = _characterController.radius * 2f;
        float height = Mathf.Max(_standHeight * ratio, minHeight);
        float bottom = _standCenter.y - _standHeight * 0.5f;

        _characterController.height = height;
        _characterController.center = new Vector3(_standCenter.x, bottom + height * 0.5f, _standCenter.z);
    }

    /// <summary> YSW : 머리 위에 일어설 공간이 있는지 검사한다. </summary>
    private bool CanStandUp()
    {
        float radius = _characterController.radius;
        float currentTop = _characterController.center.y + _characterController.height * 0.5f;
        float standTop = _standCenter.y + _standHeight * 0.5f;
        float checkDistance = standTop - currentTop;

        if (checkDistance <= 0f)
        {
            return true;
        }

        Vector3 origin = transform.position + Vector3.up * (currentTop - radius);

        return !Physics.SphereCast(origin, radius * 0.95f, Vector3.up, out _, checkDistance, _ceilingLayers, QueryTriggerInteraction.Ignore);
    }

    #endregion ===== 웅크리기 처리 =====

    #region ===== 중력 처리 =====

    /// <summary> YSW : 바닥에 붙어 있도록 수직 속도에 중력을 적용한다. </summary>
    private void UpdateGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = _groundedStickForce;
        }

        _verticalVelocity += _gravity * Time.deltaTime;
    }

    #endregion ===== 중력 처리 =====
}
