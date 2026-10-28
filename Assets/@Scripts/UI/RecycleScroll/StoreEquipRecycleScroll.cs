using System;
using System.Collections.Generic;

/// <summary> KDH : 장비 판매 목록을 표시한다. </summary>
public class StoreEquipRecycleScroll : BaseVerticalScroll<ShopData>
{
    protected override void OnAwake()
    {
        SetDataList();
    }

    protected override void OnStart()
    {
    }
    
    private void OnEnable()
    {
        if (Managers.Data == null)
        {
            return;
        }
        
        Managers.Data.OnStoreChanged += RefreshStoreSlots;
        
        ScrollToIndex(0);
    }

    private void OnDisable()
    {
        if (Managers.Data == null)
        {
            return;
        }
        
        Managers.Data.OnStoreChanged -= RefreshStoreSlots;
    }

    /// <summary> KDH : 판매 중인 장비만 기존 DataList에 구성한다. </summary>
    private void SetDataList()
    {
        DataList = new List<ShopData>();
        DataList.Clear();
        var shopDataList = Managers.Table.Shop.GetSellingItemDataList(ItemType.Equipment);

        if (shopDataList is { Count: > 0 })
        {
            DataList.AddRange(shopDataList);
        }
    }
    
    #region ===== 업데이트 =====

    /// <summary> KDH : 구매한 경우 UI를 갱신한다. </summary>
    public void RefreshStoreSlots()
    {
        foreach (var slot in Slots)
        {
            if (!slot.gameObject.activeSelf)
            {
                continue;
            }
            
            if (slot is not StoreSlot storeSlot || slot.Data == null)
            {
                continue;
            }

            storeSlot.RefreshPurchaseStatus();
        }
    }

    #endregion ===== 업데이트 =====
}
