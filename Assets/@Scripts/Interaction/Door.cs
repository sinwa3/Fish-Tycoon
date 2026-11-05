using UnityEngine;

/// <summary>
/// YSW : 플레이어와 적이 열고 닫는 문. 움직임은 애니메이션이 담당하고, 코드는 열기/닫기 트리거만 보낸다.
/// 항상 여는 쪽의 반대편으로 열리는 애니메이션(OpenFront / OpenBack)을 골라 재생한다.
/// 구조 : 루트(Door + Animator) / 문짝(Door 레이어, 콜라이더) / 문짝 자식 InteractTrigger(Interaction 레이어, 트리거)
/// </summary>
public class Door : MonoBehaviour, IInteractable, IOpenable
{
    #region ===== 설정 =====

    private static readonly int OpenFrontHash = Animator.StringToHash("OpenFront");
    private static readonly int OpenBackHash = Animator.StringToHash("OpenBack");
    private static readonly int CloseHash = Animator.StringToHash("Close");

    [Header("애니메이션")]
    [Tooltip("열기/닫기를 재생할 Animator. 비워두면 자신 또는 자식에서 찾는다.")]
    [SerializeField] private Animator _animator;

    [Tooltip("앞/뒤를 판단할 기준. 파란 화살표(정면) 쪽이 앞이다. 비워두면 이 오브젝트를 사용한다.")]
    [SerializeField] private Transform _directionReference;

    [Header("상호작용 판정")]
    [Tooltip("플레이어가 바라봤을 때 감지되는 트리거 박스 (Interaction 레이어). 시작할 때 문짝 크기에 맞춘다.")]
    [SerializeField] private BoxCollider _interactTrigger;

    [Tooltip("트리거 박스를 문짝보다 한쪽 면마다 키울 여유 크기")]
    [SerializeField] private float _triggerPadding = 0.05f;

    [Header("안내")]
    [SerializeField] private string _openText = "열기";
    [SerializeField] private string _closeText = "닫기";

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    /// <summary> YSW : 문이 열린 상태(또는 열리는 중)인지 여부 </summary>
    public bool IsOpen { get; private set; }

    public string InteractionText => IsOpen ? _closeText : _openText;

    private Transform DirectionReference => _directionReference != null ? _directionReference : transform;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeInteractTrigger();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : Animator가 지정되지 않았으면 자신 또는 자식에서 찾는다. </summary>
    private void InitializeComponents()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_animator == null)
        {
            CPrint.Warning($"{nameof(Door)} : [{name}]에 Animator가 없습니다.");
        }
    }

    /// <summary> YSW : 트리거 박스를 부모(문짝) 메쉬 크기에 여유를 더한 크기로 맞춘다. </summary>
    private void InitializeInteractTrigger()
    {
        if (_interactTrigger == null)
        {
            return;
        }

        _interactTrigger.isTrigger = true;

        Transform leaf = _interactTrigger.transform.parent;
        MeshFilter leafMeshFilter = leaf != null ? leaf.GetComponent<MeshFilter>() : null;

        if (leafMeshFilter == null || leafMeshFilter.sharedMesh == null)
        {
            return;
        }

        Bounds leafBounds = leafMeshFilter.sharedMesh.bounds;

        _interactTrigger.center = leafBounds.center;
        _interactTrigger.size = leafBounds.size + Vector3.one * (_triggerPadding * 2f);
    }

    #endregion ===== 초기화 =====

    #region ===== 상호작용 =====

    /// <summary> YSW : 문은 언제든 열고 닫을 수 있다. </summary>
    public bool CanInteract(GameObject interactor)
    {
        return _animator != null;
    }

    /// <summary> YSW : 닫혀 있으면 상호작용한 쪽의 반대편으로 열고, 열려 있으면 닫는다. </summary>
    public void Interact(GameObject interactor)
    {
        if (IsOpen)
        {
            Close();

            return;
        }

        PlayOpen(interactor);
    }

    #endregion ===== 상호작용 =====

    #region ===== 열기/닫기 =====

    /// <summary> YSW : 지정한 위치(플레이어, 적)에서 멀어지는 방향으로 문을 연다. </summary>
    public void Open(Vector3 awayFromPosition)
    {
        if (IsOpen || _animator == null)
        {
            return;
        }

        _animator.ResetTrigger(CloseHash);
        _animator.SetTrigger(GetOpenTrigger(awayFromPosition));

        IsOpen = true;
    }

    /// <summary> YSW : 문을 닫는다. (열렸던 방향에 맞는 닫기는 Animator가 이어서 재생한다) </summary>
    public void Close()
    {
        if (!IsOpen || _animator == null)
        {
            return;
        }

        _animator.ResetTrigger(OpenFrontHash);
        _animator.ResetTrigger(OpenBackHash);
        _animator.SetTrigger(CloseHash);

        IsOpen = false;
    }

    /// <summary> YSW : opener 반대쪽으로 문을 연다. (IOpenable) </summary>
    public void PlayOpen(GameObject opener)
    {
        Vector3 openerPosition = opener != null ? opener.transform.position : DirectionReference.position + DirectionReference.forward;

        Open(openerPosition);
    }

    /// <summary> YSW : 문을 닫는다. (IOpenable) </summary>
    public void PlayClose()
    {
        Close();
    }

    /// <summary> YSW : 지정한 위치가 앞쪽이면 뒤로(OpenBack), 뒤쪽이면 앞으로(OpenFront) 여는 트리거를 반환한다. </summary>
    private int GetOpenTrigger(Vector3 awayFromPosition)
    {
        Transform reference = DirectionReference;
        Vector3 toPosition = awayFromPosition - reference.position;

        bool isInFront = Vector3.Dot(toPosition, reference.forward) >= 0f;

        return isInFront ? OpenBackHash : OpenFrontHash;
    }

    #endregion ===== 열기/닫기 =====
}
