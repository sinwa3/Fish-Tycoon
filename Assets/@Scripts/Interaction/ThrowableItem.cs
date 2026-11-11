using UnityEngine;

/// <summary> YSW : 플레이어가 주워서 던지거나 내려놓을 수 있는 물건 </summary>
[RequireComponent(typeof(Rigidbody))]
public class ThrowableItem : MonoBehaviour, IInteractable
{
    #region ===== 설정 =====

    [Header("안내")]
    [SerializeField] private string _interactionText = "줍기";

    [Header("구름 멈춤")]
    [Tooltip("바닥에 닿아 있을 때 초당 줄어드는 속도 비율 (클수록 빨리 멈춘다. 0이면 저항 없음)")]
    [SerializeField] private float _rollingResistance = 2f;

    [Tooltip("바닥에 닿은 채로 이 속도(m/s)보다 느려지면 완전히 멈춘다.")]
    [SerializeField] private float _stopSpeed = 0.15f;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private Rigidbody _rigidbody;
    private Collider[] _colliders;

    // 마지막 물리 단계에서 무언가에 닿아 있었는지
    private bool _isTouching;

    /// <summary> YSW : 플레이어가 들고 있는지 여부 </summary>
    public bool IsHeld { get; private set; }

    /// <summary> YSW : 이 물건의 충돌체 목록 </summary>
    public Collider[] Colliders => _colliders;

    public string InteractionText => _interactionText;

    public InteractionType Type => InteractionType.Throwable;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
    }

    private void FixedUpdate()
    {
        UpdateRollingResistance();
    }

    private void OnCollisionStay(Collision collision)
    {
        UpdateTouchState();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 물리 처리에 필요한 컴포넌트를 가져온다. </summary>
    private void InitializeComponents()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        _colliders = GetComponentsInChildren<Collider>();
    }

    #endregion ===== 초기화 =====

    #region ===== 상호작용 =====

    /// <summary> YSW : 누가 들고 있지 않을 때만 주울 수 있다. </summary>
    public bool CanInteract(GameObject interactor)
    {
        return !IsHeld;
    }

    /// <summary> YSW : 상호작용한 플레이어가 이 물건을 들게 한다. </summary>
    public void Interact(GameObject interactor)
    {
        if (interactor == null)
        {
            return;
        }

        PlayerThrow playerThrow = interactor.GetComponent<PlayerThrow>();

        if (playerThrow == null)
        {
            CPrint.Warning($"{nameof(ThrowableItem)} : [{interactor.name}]에 PlayerThrow가 없습니다.");

            return;
        }

        playerThrow.PickUp(this);
    }

    #endregion ===== 상호작용 =====

    #region ===== 들기/던지기/내려놓기 =====

    /// <summary> YSW : 물리를 끄고 지정한 손 위치에 붙는다. </summary>
    public void OnPickedUp(Transform holdPoint)
    {
        IsHeld = true;

        SetPhysicsEnabled(false);

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary> YSW : 손에서 떨어져 지정한 속도로 날아간다. </summary>
    public void OnThrown(Vector3 velocity)
    {
        Release();

        _rigidbody.velocity = velocity;
    }

    /// <summary> YSW : 손에서 떨어져 지정한 위치에 조용히 놓인다. </summary>
    public void OnDropped(Vector3 position)
    {
        Release();

        transform.position = position;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    /// <summary> YSW : 손에서 떼어내고 물리를 다시 켠다. </summary>
    private void Release()
    {
        transform.SetParent(null);

        IsHeld = false;

        SetPhysicsEnabled(true);
    }

    /// <summary> YSW : 물리 시뮬레이션과 충돌체를 켜거나 끈다. </summary>
    private void SetPhysicsEnabled(bool isEnabled)
    {
        _rigidbody.isKinematic = !isEnabled;

        foreach (Collider itemCollider in _colliders)
        {
            itemCollider.enabled = isEnabled;
        }
    }

    #endregion ===== 들기/던지기/내려놓기 =====

    #region ===== 구름 멈춤 =====

    /// <summary> YSW : 닿아 있는 동안 물리 단계마다 표시한다. (FixedUpdate에서 읽고 지운다) </summary>
    private void UpdateTouchState()
    {
        _isTouching = true;
    }

    /// <summary>
    /// YSW : 물리 엔진은 공처럼 구르는 물체를 거의 감속시키지 않아 계속 굴러간다.
    /// 바닥에 닿아 있을 때만 수평 속도와 회전을 조금씩 줄이고, 충분히 느려지면 완전히 멈춘다. (공중에서는 영향 없음)
    /// </summary>
    private void UpdateRollingResistance()
    {
        bool isTouching = _isTouching;
        _isTouching = false;

        if (IsHeld || _rigidbody.isKinematic || _rigidbody.IsSleeping() || !isTouching)
        {
            return;
        }

        Vector3 velocity = _rigidbody.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

        if (velocity.magnitude < _stopSpeed)
        {
            StopMoving();

            return;
        }

        float keepRatio = Mathf.Clamp01(1f - _rollingResistance * Time.fixedDeltaTime);

        _rigidbody.velocity = horizontalVelocity * keepRatio + Vector3.up * velocity.y;
        _rigidbody.angularVelocity *= keepRatio;
    }

    /// <summary> YSW : 속도와 회전을 없애고 물리 계산을 쉬게 한다. </summary>
    private void StopMoving()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.Sleep();
    }

    #endregion ===== 구름 멈춤 =====
}
