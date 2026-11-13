using UnityEngine;

/// <summary> KYH : 열고 닫는 가구 파츠(서랍, 수납장 문) </summary>
public class InteractionFurniture : MonoBehaviour, IInteractable, IOpenable
{
    #region ===== 설정 =====

    [Header("안내")]
    [SerializeField] private string _openText = "열기";
    [SerializeField] private string _closeText = "닫기";
    [SerializeField] private string _lockText = "잠금 해제";

    private const string LockObjectName = "RotationLock_Interactable";

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private Animator _animator;
    private Transform _lock;

    /// <summary> KYH : 열린 상태(또는 열리는 중)인지 여부 </summary>
    public bool IsOpen { get; private set; }

    /// <summary>SJY : 자물쇠 활성화 여부</summary>
    public bool IsLocked => _lock != null && _lock.gameObject.activeInHierarchy;

    public string InteractionText =>IsLocked ? _lockText : IsOpen ? _closeText : _openText;

    /// <summary>SJY : 자물쇠 활성화여부에 따라 Type 변경</summary>
    public InteractionType Type => IsLocked ? InteractionType.Lock : InteractionType.Furniture;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeLock();
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

    /// <summary>SJY : 비활성화된 자식까지 검색하여 자물쇠 참조</summary>
    private void InitializeLock()
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name != LockObjectName)
            {
                continue;
            }

            _lock = child;
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

    /// <summary> KYH: Animator가 있으면 언제든 열고 닫을 수 있다. </summary>
    public bool CanInteract(GameObject interactor)
    {
        return _animator != null;
    }

    /// <summary> KYH, SJY : 잠긴 가구는 미니게임을 시작하고 닫혀 있으면 열고, 열려 있으면 닫는다. </summary>
    public void Interact(GameObject interactor)
    {
        if (IsLocked)
        {
            StartLockMinigame();

            return;
        }

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
        if (IsOpen || IsLocked)
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

    #region ==== 미니 게임 ====

    /// <summary> SJY : 핀 조절형과 회전형 중 하나를 무작위로 선택하여 미니게임 실행 </summary>
    private void StartLockMinigame()
    {
        if (!LockpickInventory.HasLockpick())
        {
            Managers.UI.OpenToastMessage("락픽이 부족합니다.");

            return;
        }

        if (Managers.UI == null || Managers.UI.CurrentScreen != null)
        {
            return;
        }

        SecurityMinigameScreen.MinigameType minigameType = Random.Range(0, 2) == 0 ? SecurityMinigameScreen.MinigameType.PinLock : SecurityMinigameScreen.MinigameType.RotationLock;

        SecurityMinigameScreen screen = Managers.UI.OpenScreen<SecurityMinigameScreen>();

        if (screen == null)
        {
            return;
        }

        screen.Set(minigameType, Camera.main, HandleLockUnlocked);
    }

    private void HandleLockUnlocked()
    {
        if (_lock == null)
        {
            return;
        }

        _lock.gameObject.SetActive(false);
    }

    #endregion ==== 미니 게임 ====
}
