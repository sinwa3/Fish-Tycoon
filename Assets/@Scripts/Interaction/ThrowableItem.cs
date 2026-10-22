using UnityEngine;

/// <summary> YSW : 플레이어가 주워서 던지거나 내려놓을 수 있는 물건 </summary>
[RequireComponent(typeof(Rigidbody))]
public class ThrowableItem : MonoBehaviour, IInteractable
{
    #region ===== 설정 =====

    [Header("안내")]
    [SerializeField] private string _interactionText = "줍기";

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private Rigidbody _rigidbody;
    private Collider[] _colliders;

    /// <summary> YSW : 플레이어가 들고 있는지 여부 </summary>
    public bool IsHeld { get; private set; }

    /// <summary> YSW : 이 물건의 충돌체 목록 </summary>
    public Collider[] Colliders => _colliders;

    public string InteractionText => _interactionText;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
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
}
