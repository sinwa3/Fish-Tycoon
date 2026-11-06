using UnityEngine;

/// <summary> KimYH : 맵에 배치된 생선의 정보를 관리하고 상호작용 시 획득 처리한다. </summary>
public class FishPickup : MonoBehaviour, IInteractable
{
    public string InteractionText { get; } = "획득";

    /// <summary> KimYH : 이 생선의 ID </summary>
    public int FishID { get; private set; } = -1;

    /// <summary> KimYH : 이 자리에 놓인 생선 개수 </summary>
    public int Count { get; private set; }

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

    /// <summary> KDH : 생선을 퀵슬롯에 추가하고 획득에 성공하면 오브젝트를 제거한다. </summary>
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

        Destroy(gameObject);
    }
}