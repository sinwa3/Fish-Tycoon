using System.Collections.Generic;

/// <summary> KDH : 테이블 1행의 칼럼명과 일치확인, 그 값에 맞는 자료형 수정 필수 </summary>
public class ShopData : TableData
{
    [CsvField("아이템ID")]
    public int ItemID;

    [CsvField("가격")]
    public int SellPrice;

    [CsvField("판매 중")]
    public bool IsSell;

    [CsvField("반복 구매 가능")]
    public bool CanRepurchase;
}

public class ShopTable : TableLoader<ShopData>
{
    protected override string TableName => "ShopTable";

    private readonly Dictionary<ItemType, List<ShopData>> _sellingItemDataList = new();

    /// <summary> KDH : 현재 판매 중인 아이템을 타입별로 분류하여 초기화한다. </summary>
    private void InitializeSellingItemList()
    {
        foreach (ShopData data in TableMap.Values)
        {
            if (!data.IsSell)
            {
                continue;
            }

            Item item = Managers.Table.Item.GetItem(data.ItemID);

            if (item == null)
            {
                return;
            }
            
            if (!_sellingItemDataList.TryGetValue(item.Type, out List<ShopData> itemDataList))
            {
                itemDataList = new List<ShopData>();
                _sellingItemDataList.Add(item.Type, itemDataList);
            }

            itemDataList.Add(data);
        }
    }
    
    /// <summary> KDH : 아이템 타입에 해당하는 현재 판매 중인 아이템 목록을 가져온다. </summary>
    public IReadOnlyList<ShopData> GetSellingItemDataList(ItemType itemType)
    {
        if (_sellingItemDataList.Count == 0)
        {
            InitializeSellingItemList();
        }

        return _sellingItemDataList.GetValueOrDefault(itemType);
    }

    /// <summary> KDH : 아이템ID에 해당하는 판매가격을 획득한다.  </summary>
    public int GetItemSellPrice(int itemID)
    {
        return TableMap.TryGetValue(itemID, out var value) ? value.SellPrice : 0;
    }

    /// <summary> KDH : 현재 해당아이템이 판매중인지 반환한다. </summary>
    public bool IsSelling(int itemID)
    {
        return TableMap.TryGetValue(itemID, out var value) && value.IsSell;
    }
}