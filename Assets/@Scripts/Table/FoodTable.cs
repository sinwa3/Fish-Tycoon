/// <summary> KDH : 음식 데이터를 정의한다. </summary>
public class FoodData : TableData
{
    [CsvField("생선 ID")]
    public int FishID;
    
    [CsvField("이름")]
    public string Name;

    [CsvField("판매 가격")]
    public int Price;

    [CsvField("손님 만족도")]
    public int Satisfaction;

    [CsvField("커맨드")]
    public string Command;
    
    [CsvField("리소스명")]
    public string ResourceName;
}

/// <summary> KDH : 음식 테이블을 관리한다. </summary>
public class FoodTable : TableLoader<FoodData>
{
    protected override string TableName => "FoodTable";
}