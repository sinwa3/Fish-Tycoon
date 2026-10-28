using System.Collections.Generic;
using UnityEngine;

/// <summary> KDH : 테이블 1행의 칼럼명과 일치확인, 그 값에 맞는 자료형 수정 필수 </summary>
public class ItemData : TableData
{
    [CsvField("이름")]
    public string Name;

    [CsvField("설명")]
    public string Description;

    [CsvField("적용 값")]
    public int BuffValue;

    [CsvField("최대 소지 개수")]
    public int LimitCount;

    [CsvField("타입")]
    public string Type;

    [CsvField("장비타입")]
    public string EquipmentType;

    [CsvField("장비 단계")]
    public int Tier;

    [CsvField("리소스명")]
    public string ResourceName;
}

/// <summary> KDH : 아이템 테이블 데이터를 관리한다. </summary>
public class ItemTable : TableLoader<ItemData>
{
    private readonly Dictionary<int, Item> _itemMap = new();

    protected override string TableName => "ItemTable";

    /// <summary> KDH : CSV 로딩 후 게임에서 사용할 아이템 객체를 생성한다. </summary>
    protected override void OnLoaded()
    {
        _itemMap.Clear();

        foreach (KeyValuePair<int, ItemData> pair in TableMap)
        {
            _itemMap.Add(pair.Key, new Item(pair.Key, pair.Value));
        }
    }

    /// <summary> KDH : 아이템 ID에 해당하는 아이템을 반환한다. </summary>
    public Item GetItem(int itemID)
    {
        if (itemID < 0)
        {
            return null;
        }
        
        if (_itemMap.TryGetValue(itemID, out Item item))
        {
            return item;
        }

        CPrint.Warning($"[ItemTable] ID({itemID}) not found!");
        return null;
    }

    /// <summary> KDH : 실제 아이템 시트에서 ID에 해당하는 아이콘을 가져온다. </summary>
    public Sprite GetSprite(int itemID)
    {
        if (itemID < 0)
        {
            return null;
        }
        
        Item item = GetItem(itemID);

        if (item == null)
        {
            return null;
        }

        return Managers.Resource.LoadSprite(ResourceKey.Path.ItemSprite + ResourceKey.Name.ShopIcon,
            item.ResourceName);
    }
    
    /// <summary> KDH : 장비 종류와 Tier에 해당하는 아이템을 반환한다. </summary>
    public Item GetEquipmentItem(EquipmentType equipmentType, int tier)
    {
        foreach (Item item in _itemMap.Values)
        {
            if (item.Type != ItemType.Equipment)
            {
                continue;
            }

            if (item.EquipType == equipmentType && item.Tier == tier)
            {
                return item;
            }
        }

        return null;
    }
}