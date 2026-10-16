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
}