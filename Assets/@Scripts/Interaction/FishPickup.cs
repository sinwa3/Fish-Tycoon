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

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    /// <summary> YSW : 바라봤을 때 표시할 안내 문구 (생선 이름 포함) </summary>
    public string InteractionText => string.Format(_interactionFormat, GetFishName());

    /// <summary> KimYH : 이 생선의 ID </summary>
    public int FishID { get; private set; } = -1;

    /// <summary> KimYH : 이 자리에 놓인 생선 개수 </summary>
    public int Count { get; private set; }

    #endregion ===== 상태 =====

    /// <summary> KimYH : 생선 종류와 개수를 설정한다. </summary>
    public void Setup(int fishID, int count)
    {
        FishID = fishID;
        Count = count;
    }

    /// <summary> KDH : 생선 ID와 개수가 유효하면 상호작용할 수 있다. </summary>
    public bool CanInteract(GameObject interactor)
    {
        return FishID >= 0 && Count > 0;
    }

    /// <summary> KDH : 생선을 퀵슬롯에 추가하고 획득에 성공하면 오브젝트를 제거한다. YSW : 획득한 생선 이름과 마리 수를 안내한다. </summary>
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

        Destroy(gameObject);
    }

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
