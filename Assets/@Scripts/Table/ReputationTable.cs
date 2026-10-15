using System.Collections.Generic;

/// <summary> KDH : 테이블 1행의 칼럼명과 일치확인, 그 값에 맞는 자료형 수정 필수 </summary>
public class ReputationData : TableData
{
    [CsvField("평판 등급")]
    public int ReputationGrade;

    [CsvField("평판 이름")]
    public string ReputationName;

    [CsvField("평판 설명")]
    public string ReputationDesc;

    [CsvField("요구 만족도")] // 누적 만족도
    public int RequiredSatisfaction;

    [CsvField("손님 입장 간격")]
    public int CustomerSpawnInterval;

    [CsvField("방문 손님 수")]
    public int CustomerCount;

    [CsvField("정산 보너스 배율")]
    public float SettlementBonusMultiplier;
}

/// <summary> KDH : 평판 테이블을 관리한다. </summary>
public class ReputationTable : TableLoader<ReputationData>
{
    protected override string TableName => "ReputationTable";
    
}