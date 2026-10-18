using System.Collections.Generic;
using UnityEngine;

/// <summary> KDH : 영업 전 요리리스트를 관리하는 스크롤이다. </summary>
public class FoodListScroll : BaseVerticalScroll<FoodData>
{
    #region ===== 참조 =====

    [Header("참조")]
    [SerializeField]
    private FoodTooltip _foodTooltip;

    [SerializeField]
    private FishListScroll _fishListScroll;

    #endregion ===== 참조 =====

    #region ===== 상태 =====

    /// <summary> KDH : 요리별 선택 수량을 임시 저장한다. </summary>
    private readonly Dictionary<int, int> _selectedFoodCounts = new();

    /// <summary> KDH : 영업 시 선택할 수 있는 최대 요리 수량 </summary>
    private int _maxSelectableFoodCount;
    /// <summary> KDH : 현재 선택한 요리 수량 </summary>
    private int _curSelectedFoodCount;

    /// <summary> KDH : 요리를 추가로 선택할 수 있는지 반환한다. </summary>
    private bool CanAddFood => _curSelectedFoodCount < _maxSelectableFoodCount;

    #endregion ===== 상태 =====

    protected override void OnAwake()
    {
        SetDataList();
    }

    protected override void OnStart()
    {
    }

    /// <summary> KDH : 요리 슬롯에 툴팁과 스크롤을 연결한다. </summary>
    protected override void OnSlotCreated(BaseRecycleSlot<FoodData> slot)
    {
        if (slot is not FoodListSlot foodSlot)
        {
            return;
        }

        foodSlot.SetTooltip(_foodTooltip);
        foodSlot.SetFoodListScroll(this);
    }

    /// <summary> KDH : 음식 데이터 리스트를 세팅한다. </summary>
    private void SetDataList()
    {
        DataList = new List<FoodData>();

        if (Managers.Table == null)
        {
            return;
        }

        foreach (var foodData in Managers.Table.Food.Map)
        {
            // 생선을 사용하지 않는 음식은 목록에서 제외한다.
            if (foodData.Value.FishID == -1)
            {
                continue;
            }

            DataList.Add(foodData.Value);
        }
    }

    /// <summary> KDH : 요리에 사용할 수 있는 생선 수량을 반환한다. </summary>
    public int GetAvailableFishCount(int fishID)
    {
        return _fishListScroll != null ? _fishListScroll.GetAvailableFishCount(fishID) : 0;
    }
    
    #region ===== 음식 수량 =====

    /// <summary> KDH : 최대 선택 가능 요리 수량을 설정한다. </summary>
    public void SetMaxSelectableFoodCount(int count)
    {
        _maxSelectableFoodCount = Mathf.Max(0, count);
    }

    /// <summary> KDH : 선택한 요리 수량을 증가시킨다. </summary>
    public void AddFood(FoodData foodData)
    {
        if (foodData == null)
        {
            return;
        }

        if (!CanAddFood)
        {
            Managers.UI.OpenToastMessage("평판 레벨이 부족하여 음식을 추가할 수 없습니다.");
            return;
        }

        // 생선 사용 수량을 먼저 예약한다.
        if (_fishListScroll == null || !_fishListScroll.TryAddSelectedFish(foodData.FishID))
        {
            return;
        }

        int foodID = foodData.ID;
        _selectedFoodCounts[foodID] = GetSelectedFoodCount(foodID) + 1;

        _curSelectedFoodCount++;

        // 요리 목록 UI를 갱신한다.
        RefreshFoodSlots();
    }

    /// <summary> KDH : 선택한 요리 수량을 감소시킨다. </summary>
    public void RemoveFood(FoodData foodData)
    {
        if (foodData == null)
        {
            return;
        }

        int foodID = foodData.ID;
        int selectedCount = GetSelectedFoodCount(foodID);

        // 생선 사용 수량을 먼저 해제한다.
        if (_fishListScroll == null || !_fishListScroll.TryRemoveSelectedFish(foodData.FishID))
        {
            return;
        }

        if (selectedCount == 1)
        {
            _selectedFoodCounts.Remove(foodID);
        }
        else
        {
            _selectedFoodCounts[foodID] = selectedCount - 1;
        }

        _curSelectedFoodCount--;

        // 요리 목록 UI를 갱신한다.
        RefreshFoodSlots();
    }

    /// <summary> KDH : 특정 요리의 선택 수량을 반환한다. </summary>
    public int GetSelectedFoodCount(int foodID)
    {
        return _selectedFoodCounts.GetValueOrDefault(foodID, 0);
    }

    #endregion ===== 음식 수량 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 요리 슬롯 UI를 갱신한다. </summary>
    private void RefreshFoodSlots()
    {
        foreach (BaseRecycleSlot<FoodData> slot in Slots)
        {
            if (slot is not FoodListSlot foodSlot || slot.Data == null)
            {
                continue;
            }

            foodSlot.RefreshCount();
        }
    }

    #endregion ===== 업데이트 =====
}