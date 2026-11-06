using UnityEngine;

/// <summary>
/// YSW : 스텔스를 끝내고 나가는 현관문. 열리지 않고, 상호작용하면 정산 단계로 넘어간다. (정산 후 경영 파트로 이동)
/// 구조 : 루트(ExitDoor) / 문짝(콜라이더) / 문짝 자식 InteractTrigger(Interaction 레이어, 트리거)
/// </summary>
public class ExitDoor : MonoBehaviour, IInteractable
{
    #region ===== 설정 =====

    [Header("상호작용 판정")]
    [Tooltip("플레이어가 바라봤을 때 감지되는 트리거 박스 (Interaction 레이어). 시작할 때 문짝 크기에 맞춘다.")]
    [SerializeField] private BoxCollider _interactTrigger;

    [Tooltip("트리거 박스를 문짝보다 한쪽 면마다 키울 여유 크기")]
    [SerializeField] private float _triggerPadding = 0.05f;

    [Header("안내")]
    [SerializeField] private string _interactionText = "나가기";

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    // 이미 나가기를 눌렀는지 여부 (정산이 두 번 실행되지 않도록)
    private bool _hasExited;

    public string InteractionText => _interactionText;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeInteractTrigger();
    }

    #region ===== 초기화 =====

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

    /// <summary> YSW : 아직 나가지 않았으면 상호작용할 수 있다. </summary>
    public bool CanInteract(GameObject interactor)
    {
        return !_hasExited;
    }

    /// <summary> YSW : 탈출 이벤트를 발생시킨다. </summary>
    public void Interact(GameObject interactor)
    {
        if (_hasExited)
        {
            return;
        }

        _hasExited = true;

        if (Managers.Event != null)
        {
            Managers.Event.RaisePlayerEscaped();
        }
    }

    #endregion ===== 상호작용 =====
}
