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

    [Header("가속 / 감속")]
    [Tooltip("이동 키를 누르고 있을 때 초당 속도 변화량 (클수록 빠르게 최고 속도에 도달한다)")]
    [SerializeField] private float _acceleration = 40f;

    [Tooltip("이동 키를 뗐을 때 초당 속도 감소량 (클수록 덜 미끄러진다. 아주 크게 하면 바로 멈춘다)")]
    [SerializeField] private float _deceleration = 60f;

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

    private float _previousAxisX;
    private float _previousAxisY;

    private Vector3 _moveDirection;
    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;

    /// <summary> YSW : 현재 수평 이동 속도 </summary>
    public float CurrentSpeed => _horizontalVelocity.magnitude;

    /// <summary> YSW : 수평 이동 속도가 0보다 크면 이동 중으로 판단한다. </summary>
    public bool IsMoving => CurrentSpeed > 0f;

    /// <summary> YSW : 웅크리기 상태 여부 </summary>
    public bool IsCrouching { get; private set; }

    /// <summary> YSW : 실제로 달리고 있는 상태 여부 (이동 키 입력 중 + Shift + 달리기 가능) </summary>
    public bool IsRunning { get; private set; }

    /// <summary> YSW : 서 있을 때 대비 현재 높이 비율 (서기 1, 웅크리기 _crouchHeightRatio) </summary>
    public float HeightRatio { get; private set; } = 1f;

    /// <summary> YSW : 웅크리면서 플레이어 기준점(transform)이 서 있을 때보다 내려간 거리. 카메라 높이 보정에 사용한다. </summary>
    public float CrouchPivotOffset => (_standHeight - _characterController.height) * 0.5f;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeStandShape();
    }

    private void OnDisable()
    {
        ResetMoveState();
    }

    private void Update()
    {
        UpdateCrouchInput();
        UpdateMoveInput();
        UpdateHorizontalVelocity();
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

    /// <summary> YSW : 서 있을 때의 충돌 높이를 저장한다. </summary>
    private void InitializeStandShape()
    {
        _standHeight = _characterController.height;
    }

    #endregion ===== 초기화 =====

    #region ===== 이동 처리 =====

    /// <summary> YSW : 현재 입력으로 이동하려는 방향과 달리기 상태를 갱신한다. </summary>
    private void UpdateMoveInput()
    {
        InputManager input = Managers.Input;

        if (input == null || !input.CanReceivePlayer)
        {
            ResetMoveState();

            return;
        }

        float axisX = GetPressedAxis(input.KeyAxisX, _previousAxisX);
        float axisY = GetPressedAxis(input.KeyAxisY, _previousAxisY);

        _previousAxisX = input.KeyAxisX;
        _previousAxisY = input.KeyAxisY;

        _moveDirection = GetMoveDirection(axisX, axisY);

        IsRunning = !IsCrouching && input.Key_LeftShift && _moveDirection.sqrMagnitude > 0f && CanRun();
    }

    /// <summary>
    /// YSW : InputManager의 축 값은 키를 떼도 서서히 0으로 줄어든다.
    /// 값이 줄어드는 중이면 키를 뗀 것으로 보고 0을, 유지되거나 커지는 중이면 누르고 있는 방향(-1 또는 1)을 반환한다.
    /// </summary>
    private float GetPressedAxis(float currentAxis, float previousAxis)
    {
        float currentAbs = Mathf.Abs(currentAxis);

        if (currentAbs <= 0f || currentAbs < Mathf.Abs(previousAxis))
        {
            return 0f;
        }

        return Mathf.Sign(currentAxis);
    }

    /// <summary> YSW : 이동 방향, 속도, 달리기 상태를 초기화한다. </summary>
    private void ResetMoveState()
    {
        _moveDirection = Vector3.zero;
        _horizontalVelocity = Vector3.zero;
        _previousAxisX = 0f;
        _previousAxisY = 0f;
        IsRunning = false;
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

    /// <summary> YSW : 키를 누르고 있으면 가속 값으로, 뗐으면 감속 값으로 현재 수평 속도를 목표 속도에 맞춘다. </summary>
    private void UpdateHorizontalVelocity()
    {
        bool hasInput = _moveDirection.sqrMagnitude > 0f;

        Vector3 targetVelocity = _moveDirection * GetMoveSpeed();
        float changeRate = hasInput ? _acceleration : _deceleration;

        _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetVelocity, changeRate * Time.deltaTime);
    }

    /// <summary> YSW : 수평 속도와 중력을 합쳐 캐릭터를 이동시킨다. </summary>
    private void UpdateMovement()
    {
        Vector3 velocity = _horizontalVelocity + Vector3.up * _verticalVelocity;

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
        float minRatio = _characterController.radius * 2f / _standHeight;
        float targetRatio = IsCrouching ? Mathf.Max(_crouchHeightRatio, minRatio) : 1f;

        if (Mathf.Approximately(HeightRatio, targetRatio))
        {
            return;
        }

        float nextRatio = Mathf.MoveTowards(HeightRatio, targetRatio, _crouchTransitionSpeed * Time.deltaTime);

        ResizeController(nextRatio);
    }

    /// <summary>
    /// YSW : 충돌 판정이 있는 Move로 몸을 옮기면서 캡슐 높이를 바꿔, 발이 바닥에 붙은 채로 앉고 일어서게 한다.
    /// (center를 직접 바꾸면 캡슐이 지형과 겹쳤다가 밀려나면서 낮은 턱 위로 올라가는 문제가 생길 수 있다)
    /// </summary>
    private void ResizeController(float ratio)
    {
        const float MoveTolerance = 0.001f;

        float currentHeight = _characterController.height;
        float targetHeight = _standHeight * ratio;
        float heightDelta = targetHeight - currentHeight;

        if (heightDelta < 0f)
        {
            // 앉기 : 캡슐을 줄이면 발이 뜨므로, 줄어든 만큼 아래로 내려 바닥에 다시 붙인다.
            _characterController.height = targetHeight;
            _characterController.Move(Vector3.down * (-heightDelta * 0.5f));
        }
        else if (heightDelta > 0f)
        {
            // 일어서기 : 먼저 위로 올라갈 수 있는 만큼만 올라간 뒤, 실제로 올라간 만큼만 캡슐을 늘린다.
            float beforeY = transform.position.y;

            _characterController.Move(Vector3.up * (heightDelta * 0.5f));

            float requestedUp = heightDelta * 0.5f;
            float movedUp = Mathf.Max(0f, transform.position.y - beforeY);

            if (movedUp < requestedUp - MoveTolerance)
            {
                // 머리 위가 막혀 다 못 올라갔으면 올라간 만큼만 늘리고, 다음 프레임에 다시 시도한다.
                _characterController.height = currentHeight + movedUp * 2f;
                HeightRatio = _characterController.height / _standHeight;

                return;
            }

            _characterController.height = targetHeight;
        }

        HeightRatio = ratio;
    }

    /// <summary> YSW : 머리 위에 일어설 공간이 있는지 검사한다. </summary>
    private bool CanStandUp()
    {
        float radius = _characterController.radius;
        float currentTop = _characterController.center.y + _characterController.height * 0.5f;
        float checkDistance = _standHeight - _characterController.height;

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
