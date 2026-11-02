using UnityEngine;

/// <summary> KDH : 생선 데이터를 정의한다. </summary>
public class FishData : TableData
{
    [CsvField("이름")]
    public string Name;

    [CsvField("랜덤 획득 최소 개수")]
    public int MinRandomAcquireCount;

    [CsvField("랜덤 획득 최대 개수")]
    public int MaxRandomAcquireCount;

    [CsvField("생선 판매가")]
    public int SellPrice;

    // -1 (구매불가)
    [CsvField("생선 구매가")] 
    public int BuyPrice;

    [CsvField("습득 확률 가중치")]
    public int AcquireWeight;

    [CsvField("리소스명")]
    public string ResourceName;
}

/// <summary> KDH : 생선 테이블을 관리한다. </summary>
public class FishTable : TableLoader<FishData>
{
    protected override string TableName => "FishTable";

    /// <summary> KDH : 해당 생선의 스프라이트를 가져온다. </summary>
    public Sprite GetSprite(int fishID)
    {
        if (Map.ContainsKey(fishID))
        {
            string spriteName = Map[fishID].ResourceName;
            int lastIndex = spriteName.LastIndexOf('_');

            if (lastIndex >= 0)
            {
                spriteName = spriteName.Substring(0, lastIndex);
            }
            else
            {
                CPrint.Warning($"[FishTable] 올바르지 않은 리소스 명입니다. {Map[fishID].ResourceName}");
                return null;
            }
            
            return Managers.Resource.LoadSprite(ResourceKey.Path.FishSprite + spriteName, Map[fishID].ResourceName);
        }
        
        CPrint.Warning($"[FishTable] FishID({fishID})에 해당하는 Sprite가 존재하지 않는다.");
        return null;
    }

    /// <summary> KDH : 생선ID에 해당하는 가격정보를 가져온다. </summary>
    public int GetSellPrice(int fishID)
    {
        return Map[fishID]?.SellPrice ?? 0;
    }
    
    /// <summary> KDH : 생선ID에 해당하는 생선정보를 가져온다. </summary>
    public FishData GetFish(int fishID)
    {
        if (fishID < 0)
        {
            return null;
        }
        
        if (Map.TryGetValue(fishID, out FishData data))
        {
            return data;
        }

        CPrint.Warning($"[FishTable] ID({fishID}) not found!");
        return null;
    }
}