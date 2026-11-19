/// <summary> KDH : 테이블 1행의 칼럼명과 일치확인, 그 값에 맞는 자료형 수정 필수 </summary>
public class LoadingTipData : TableData
{
    [CsvField("ID")]
    public int ID;

    [CsvField("내용")]
    public string Desc;
}

public class LoadingTipTable : TableLoader<LoadingTipData>
{
    protected override string TableName => "LoadingTipTable";
}
