using UnityEngine;

/// <summary> YSW : 상호작용 가구 종류 </summary>
public enum FurnitureType
{
    Door,
    Drawer,
    Cabinet,
}

/// <summary>
/// YSW : 열고 닫는 상호작용 가구. 움직임은 애니메이션이 담당하고, 코드는 열기/닫기 트리거만 보낸다.
/// 방향이 있는 가구(문)는 상호작용한 쪽의 반대편으로 열리는 애니메이션(OpenFront / OpenBack)을 골라 재생한다.
/// </summary>
public class InteractionFurniture : MonoBehaviour, IInteractable, IOpenable
{
    #region ===== 설정 =====

    private static readonly int OpenFrontHash = Animator.StringToHash("OpenFront");
    private static readonly int OpenBackHash = Animator.StringToHash("OpenBack");
    private static readonly int CloseHash = Animator.StringToHash("Close");

    [Header("종류")]
    [SerializeField] private FurnitureType _furnitureType = FurnitureType.Door;

    [Header("애니메이션")]
    [Tooltip("열기/닫기를 재생할 Animator. 비워두면 자식에서 찾는다.")]
    [SerializeField] private Animator _animator;

    [Tooltip("켜면 상호작용한 쪽의 반대편으로 열리는 애니메이션을 고른다. (끄면 항상 OpenFront)")]
    [SerializeField] private bool _useDirectionalOpen = true;

    [Tooltip("앞/뒤를 판단할 기준. 파란 화살표(정면) 쪽이 앞이다. 비워두면 이 오브젝트를 사용한다.")]
    [SerializeField] private Transform _directionReference;

    [Header("안내")]
    [SerializeField] private string _openText = "열기";
    [SerializeField] private string _closeText = "닫기";

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    /// <summary> YSW : 가구 종류 </summary>
    public FurnitureType Type => _furnitureType;

    /// <summary> YSW : 열린 상태(또는 열리는 중)인지 여부 </summary>
    public bool IsOpen { get; private set; }

    public string InteractionText => IsOpen ? _closeText : _openText;

    private Transform DirectionReference => _directionReference != null ? _directionReference : transform;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : Animator가 지정되지 않았으면 자식에서 찾는다. </summary>
    private void InitializeComponents()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_animator == null)
        {
            CPrint.Warning($"{nameof(InteractionFurniture)} : [{name}]에 Animator가 없습니다.");
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 상호작용 =====

    /// <summary> YSW : 가구는 언제든 열고 닫을 수 있다. </summary>
    public bool CanInteract(GameObject interactor)
    {
        return _animator != null;
    }

    /// <summary> YSW : 닫혀 있으면 열고, 열려 있으면 닫는다. </summary>
    public void Interact(GameObject interactor)
    {
        if (IsOpen)
        {
            PlayClose();

            return;
        }

        PlayOpen(interactor);
    }

    #endregion ===== 상호작용 =====

    #region ===== 열기/닫기 =====

    /// <summary> YSW : 열기 애니메이션을 재생한다. 방향이 있으면 opener 반대쪽으로 여는 애니메이션을 고른다. </summary>
    public void PlayOpen(GameObject opener)
    {
        if (IsOpen || _animator == null)
        {
            return;
        }

        int openTrigger = GetOpenTrigger(opener);

        _animator.ResetTrigger(CloseHash);
        _animator.SetTrigger(openTrigger);

        IsOpen = true;
    }

    /// <summary> YSW : 닫기 애니메이션을 재생한다. (열렸던 방향에 맞는 닫기는 Animator가 이어서 재생한다) </summary>
    public void PlayClose()
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

    /// <summary> YSW : opener가 앞쪽에 있으면 뒤로(OpenBack), 뒤쪽에 있으면 앞으로(OpenFront) 여는 트리거를 반환한다. </summary>
    private int GetOpenTrigger(GameObject opener)
    {
        if (!_useDirectionalOpen || opener == null)
        {
            return OpenFrontHash;
        }

        Transform reference = DirectionReference;
        Vector3 toOpener = opener.transform.position - reference.position;

        bool isOpenerInFront = Vector3.Dot(toOpener, reference.forward) >= 0f;

        return isOpenerInFront ? OpenBackHash : OpenFrontHash;
    }

    #endregion ===== 열기/닫기 =====
}
