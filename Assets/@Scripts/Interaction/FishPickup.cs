using System.Collections;
using UnityEngine;

/// <summary> KimYH : 맵에 배치된 생선의 정보를 관리하고 상호작용 시 획득 처리한다. </summary>
public class FishPickup : MonoBehaviour, IInteractable
{
    #region ===== 설정 =====

    private const string DefaultFishName = "생선";

    [Header("안내")]
    [Tooltip("상호작용 안내 문구. {0}에 생선 이름이 들어간다.")]
    [SerializeField] private string _interactionFormat = "{0} 줍기";

    [Tooltip("획득 안내 문구. {0}에 생선 이름, {1}에 개수가 들어간다.")]
    [SerializeField] private string _acquireMessageFormat = "{0} {1}마리를 획득했습니다.";

    [Header("디졸브")]
    [Tooltip("획득 후 완전히 사라지기까지 걸리는 시간(초)")]
    [SerializeField, Min(0.01f)] private float _dissolveDuration = 1.0f;

    /// <summary> KimYH : DespawnEffect 쉐이더의 Split Value (1 : 전부 보임, 0 : 전부 사라짐) </summary>
    private static readonly int SplitValueID = Shader.PropertyToID("_Split_Value");

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    /// <summary> YSW : 바라봤을 때 표시할 안내 문구 (생선 이름 포함) </summary>
    public string InteractionText => string.Format(_interactionFormat, GetFishName());

    public InteractionType Type => InteractionType.Fish;

    /// <summary> KimYH : 이 생선의 ID </summary>
    public int FishID { get; private set; } = -1;

    /// <summary> KimYH : 이 자리에 놓인 생선 개수 </summary>
    public int Count { get; private set; }

    /// <summary> KimYH : 디졸브 연출 중에 상호작용 방지용 </summary>
    private bool _isDissolving;

    private Renderer[] _renderers;
    private Collider[] _colliders;
    private MaterialPropertyBlock _propertyBlock;

    #endregion ===== 상태 =====

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _colliders = GetComponentsInChildren<Collider>();
    }

    /// <summary> KimYH : 생선 종류와 개수를 설정한다. </summary>
    public void Setup(int fishID, int count)
    {
        FishID = fishID;
        Count = count;
    }

    /// <summary> KDH : 생선 ID와 개수가 유효하면 상호작용할 수 있다. </summary>
    public bool CanInteract(GameObject interactor)
    {
        return !_isDissolving && FishID >= 0 && Count > 0;
    }

    /// <summary> KDH : 생선을 퀵슬롯에 추가하고 획득에 성공하면 오브젝트를 제거한다. YSW : 획득한 생선 이름과 마리 수를 안내한다. KimYH : 제거는 디졸브 연출 후에 한다. </summary>
    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        if (!Managers.Data.TryAddFishToQuickSlot(FishID, Count))
        {
            CPrint.Warning($"[FishPickup] 생선 획득 실패. FishID: {FishID}");
            Managers.UI?.OpenToastMessage("인벤토리가 가득 찼습니다.");
            return;
        }

        Managers.UI?.OpenToastMessage(string.Format(_acquireMessageFormat, GetFishName(), Count));

        StartCoroutine(DissolveAndDestroy());
    }

    #region ===== 디졸브 =====

    /// <summary> KimYH : Split Value를 1에서 0으로 줄여 생선을 사라지게 한 뒤 오브젝트를 제거한다. </summary>
    private IEnumerator DissolveAndDestroy()
    {
        _isDissolving = true;

        foreach (Collider fishCollider in _colliders)
        {
            fishCollider.enabled = false;
        }

        // 아웃라인에서 몸체 하나를 더 덮어서 그리므로 디졸브가 진행될 때는 레이어를 바꾸어 아웃라인용 몸체를 없앤다.
        foreach (Renderer fishRenderer in _renderers)
        {
            fishRenderer.gameObject.layer = LayerKey.Idx.Default;
        }

        // 머티리얼을 복제하지 않고 이 생선의 렌더러에만 값을 덮어쓴다 (MPB)
        _propertyBlock ??= new MaterialPropertyBlock();

        float elapsed = 0.0f;

        while (elapsed < _dissolveDuration)
        {
            elapsed += Time.deltaTime;
            SetSplitValue(1.0f - Mathf.Clamp01(elapsed / _dissolveDuration));

            yield return null;
        }

        Destroy(gameObject);
    }

    /// <summary> KimYH : 모든 렌더러의 Split Value를 설정한다. </summary>
    private void SetSplitValue(float value)
    {
        foreach (Renderer fishRenderer in _renderers)
        {
            fishRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(SplitValueID, value);
            fishRenderer.SetPropertyBlock(_propertyBlock);
        }
    }

    #endregion ===== 디졸브 =====

    /// <summary> YSW : 생선 테이블에서 이름을 가져온다. (없으면 기본 이름) </summary>
    private string GetFishName()
    {
        FishTable fishTable = Managers.Table?.Fish;

        if (fishTable == null || !fishTable.Map.ContainsKey(FishID))
        {
            return DefaultFishName;
        }

        return fishTable.Map[FishID].Name;
    }
}
