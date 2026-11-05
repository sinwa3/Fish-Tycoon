using UnityEngine;

/// <summary> KYH : 열고 닫는 가구 파츠(서랍, 수납장 문) </summary>
public class InteractionFurniture : MonoBehaviour, IInteractable, IOpenable
{
    #region ===== 설정 =====

    [Header("안내")]
    [SerializeField] private string _openText = "열기";
    [SerializeField] private string _closeText = "닫기";

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private Animator _animator;

    /// <summary> KYH : 열린 상태(또는 열리는 중)인지 여부 </summary>
    public bool IsOpen { get; private set; }

    public string InteractionText => IsOpen ? _closeText : _openText;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeLayer();
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 같은 오브젝트의 Animator를 가져온다. </summary>
    private void InitializeComponents()
    {
        _animator = GetComponent<Animator>();

        if (_animator == null)
        {
            CPrint.Warning($"{nameof(InteractionFurniture)} : [{name}]에 Animator가 없습니다.");

            return;
        }
    }

    /// <summary> KYH : 플레이어 레이에 맞도록 상호작용 레이어로 바꾼다. </summary>
    private void InitializeLayer()
    {
        gameObject.layer = LayerKey.Idx.Interaction;
    }

    #endregion ===== 초기화 =====

    #region ===== 상호작용 =====

    /// <summary> KYH : Animator가 있으면 언제든 열고 닫을 수 있다. </summary>
    public bool CanInteract(GameObject interactor)
    {
        return _animator != null;
    }

    /// <summary> KYH : 닫혀 있으면 열고, 열려 있으면 닫는다. </summary>
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

    #region ===== 열기, 닫기 =====

    /// <summary> KYH : 열기 애니메이션을 재생한다. </summary>
    public void PlayOpen(GameObject opener)
    {
        if (IsOpen)
        {
            return;
        }

        if (_animator == null)
        {
            return;
        }

        _animator.ResetTrigger(AnimatorKey.Hash.DoClose);
        _animator.SetTrigger(AnimatorKey.Hash.DoOpen);

        IsOpen = true;
    }

    /// <summary> KYH : 닫기 애니메이션을 재생한다. </summary>
    public void PlayClose()
    {
        if (!IsOpen)
        {
            return;
        }

        if (_animator == null)
        {
            return;
        }

        _animator.ResetTrigger(AnimatorKey.Hash.DoOpen);
        _animator.SetTrigger(AnimatorKey.Hash.DoClose);

        IsOpen = false;
    }

    #endregion ===== 열기, 닫기 =====
}
