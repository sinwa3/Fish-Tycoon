using System.Collections.Generic;
using UnityEngine;

/// <summary> KDH : 영업 전 생선리스트를 관리하는 스크롤이다. </summary>
public class FishListScroll : BaseVerticalScroll<FishData>
{
    #region ===== 참조 =====

    [Header("참조")]
    [SerializeField]
    private FishTooltip _fishTooltip;

    [SerializeField]
    private FoodListScroll _foodListScroll;

    #endregion ===== 참조 =====

    #region ===== 상태 =====

    /// <summary> KDH : 생선별 요리 사용 예정 수량을 저장한다. </summary>
    private readonly Dictionary<int, int> _selectedFishCounts = new();

    #endregion ===== 상태 =====

    protected override void OnAwake()
    {
        SetDataList();
    }

    protected override void OnStart()
    {
    }

    /// <summary> KDH : 생선 슬롯에 툴팁과 스크롤을 연결한다. </summary>
    protected override void OnSlotCreated(BaseRecycleSlot<FishData> slot)
    {
        if (slot is not FishListSlot fishSlot)
        {
            return;
        }

        fishSlot.SetTooltip(_fishTooltip);
        fishSlot.SetFishListScroll(this);
    }

    /// <summary> KDH : 생선 데이터 리스트를 세팅한다. </summary>
    private void SetDataList()
    {
        DataList = new List<FishData>();

        if (Managers.Table == null || Managers.Table.Fish == null)
        {
            return;
        }

        foreach (var fishData in Managers.Table.Fish.Map)
        {
            // 소지 중인 생선이 아닌 경우에는 인벤에 표시하지 않는다.
            if (Managers.Data.GetFishCount(fishData.Value.ID) == 0)
            {
                continue;
            }

            DataList.Add(fishData.Value);
        }
    }

    #region ===== 생선 수량 =====

    /// <summary> KDH : 생선별 요리 사용 예정 수량을 반환한다. </summary>
    public int GetSelectedFishCount(int fishID)
    {
        return _selectedFishCounts.GetValueOrDefault(fishID, 0);
    }

    /// <summary> KDH : 요리에 사용할 수 있는 생선 수량을 반환한다. </summary>
    public int GetAvailableFishCount(int fishID)
    {
        int inventoryCount = Managers.Data.GetFishCount(fishID);
        int selectedCount = GetSelectedFishCount(fishID);

        return Mathf.Max(0, inventoryCount - selectedCount);
    }

    /// <summary> KDH : 요리에 사용할 생선 수량을 증가시킨다. </summary>
    public bool TryAddSelectedFish(int fishID)
    {
        if (GetAvailableFishCount(fishID) <= 0)
        {
            return false;
        }

        _selectedFishCounts[fishID] = GetSelectedFishCount(fishID) + 1;

        RefreshFishSlots();

        return true;
    }

    /// <summary> KDH : 요리에 사용할 생선 수량을 감소시킨다. </summary>
    public bool TryRemoveSelectedFish(int fishID)
    {
        int selectedCount = GetSelectedFishCount(fishID);

        if (selectedCount <= 0)
        {
            return false;
        }

        if (selectedCount == 1)
        {
            _selectedFishCounts.Remove(fishID);
        }
        else
        {
            _selectedFishCounts[fishID] = selectedCount - 1;
        }

        RefreshFishSlots();

        return true;
    }

    #endregion ===== 생선 수량 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 요리 선택 수량 변경에 맞춰 생선 수량 UI를 갱신한다. </summary>
    public void RefreshFishSlots()
    {
        foreach (BaseRecycleSlot<FishData> slot in Slots)
        {
            if (slot is not FishListSlot fishSlot || slot.Data == null)
            {
                continue;
            }

            fishSlot.RefreshCount();
        }
    }

    #endregion ===== 업데이트 =====
}