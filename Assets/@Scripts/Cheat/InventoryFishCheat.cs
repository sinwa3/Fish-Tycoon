#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary> 영업 전 가게에 보유한 생선을 넣고 빼고 생선, 요리 목록에 바로 반영한다 </summary>
public class InventoryFishCheat : BaseFishCheat
{
    #region ===== 리플렉션 =====

    // 보유 생선을 더하는 공개 메서드가 없다
    private static readonly MethodInfo AddFishMethod = GetPrivateMethod(typeof(DataManager), "TryAddFish");
    // 생선 목록은 씬이 시작될 때 보유한 종류로 한 번만 만들어져서 다시 만들게 한다
    private static readonly MethodInfo SetFishDataListMethod = GetPrivateMethod(typeof(FishListScroll), "SetDataList");
    private static readonly MethodInfo InitializeFishScrollMethod = GetPrivateMethod(typeof(BaseVerticalScroll<FishData>), "InitializeScroll");
    // 요리 슬롯은 생선이 있는지에 따라 어둡게 표시해서 다시 그리게 한다
    private static readonly MethodInfo RefreshFoodSlotsMethod = GetPrivateMethod(typeof(BaseVerticalScroll<FoodData>), "RefreshSlots");

    #endregion ===== 리플렉션 =====

    private FishListScroll _fishListScroll;
    private FoodListScroll _foodListScroll;

    public override string Title => "생선 (가게 영업 전 보유)";

    protected override bool IsTargetScene => IsTycoonPhase(TycoonScene.Phase.BeforeBusiness);

    protected override void OnInitialize()
    {
        if (AddFishMethod == null || SetFishDataListMethod == null || InitializeFishScrollMethod == null || RefreshFoodSlotsMethod == null)
        {
            CPrint.Warning("[InventoryFishCheat] 리플렉션할 메서드 없음");
        }
    }

    protected override int GetCount(int id)
    {
        return Managers.Data.GetFishCount(id);
    }

    protected override void Add(int id, int count)
    {
        AddFishMethod?.Invoke(Managers.Data, new object[] { id, count });
        RefreshLists();
    }

    /// <summary> 보유 수에서 빼되 요리에 쓰기로 고른 수보다 적게는 안 뺀다 </summary>
    protected override void Remove(int id, int count)
    {
        InventoryFishData fishData = FindInventoryFish(id);

        if (fishData == null)
        {
            return;
        }

        FishListScroll fishListScroll = FindFishListScroll();
        int minCount = fishListScroll != null ? fishListScroll.GetSelectedFishCount(id) : 0;

        fishData.Count = Mathf.Max(minCount, fishData.Count - count);

        if (fishData.Count == 0 && Managers.Data.GetInventoryFishList() is List<InventoryFishData> fishList)
        {
            fishList.Remove(fishData);
        }

        RefreshLists();
    }

    /// <summary> 생선 목록을 보유한 종류로 다시 만들고 요리 슬롯을 다시 그린다 (아직 한 번도 안 열린 목록은 열릴 때 알아서 만든다) </summary>
    private void RefreshLists()
    {
        FishListScroll fishListScroll = FindFishListScroll();

        // DataList는 목록의 Awake에서 처음 만들어진다
        if (fishListScroll != null && fishListScroll.DataList != null)
        {
            SetFishDataListMethod?.Invoke(fishListScroll, null);
            InitializeFishScrollMethod?.Invoke(fishListScroll, null);
        }

        FoodListScroll foodListScroll = FindFoodListScroll();

        if (foodListScroll != null && foodListScroll.DataList != null)
        {
            RefreshFoodSlotsMethod?.Invoke(foodListScroll, null);
        }
    }

    private FishListScroll FindFishListScroll()
    {
        if (_fishListScroll == null)
        {
            _fishListScroll = Object.FindObjectOfType<FishListScroll>(true);
        }

        return _fishListScroll;
    }

    private FoodListScroll FindFoodListScroll()
    {
        if (_foodListScroll == null)
        {
            _foodListScroll = Object.FindObjectOfType<FoodListScroll>(true);
        }

        return _foodListScroll;
    }

    private static InventoryFishData FindInventoryFish(int fishID)
    {
        IReadOnlyList<InventoryFishData> fishList = Managers.Data.GetInventoryFishList();

        if (fishList == null)
        {
            return null;
        }

        foreach (InventoryFishData fishData in fishList)
        {
            if (fishData.FishID == fishID)
            {
                return fishData;
            }
        }

        return null;
    }

    private static MethodInfo GetPrivateMethod(System.Type type, string methodName)
    {
        return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
    }
}
#endif
