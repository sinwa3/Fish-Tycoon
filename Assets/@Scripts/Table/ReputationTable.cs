using System;

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
    private ReputationData[] _reputationCache;

    protected override string TableName => "ReputationTable";

    /// <summary> KDH : 평판 데이터를 요구 만족도 오름차순으로 캐싱한다. </summary>
    protected override void OnLoaded()
    {
        _reputationCache = new ReputationData[TableMap.Count];

        int index = 0;

        foreach (ReputationData data in TableMap.Values)
        {
            _reputationCache[index++] = data;
        }

        Array.Sort(_reputationCache, (a, b) =>
            a.RequiredSatisfaction.CompareTo(b.RequiredSatisfaction));
    }

    /// <summary> KDH : 평판 등급에 해당하는 평판 데이터를 반환한다. </summary>
    public ReputationData GetReputationByGrade(int grade)
    {
        foreach (ReputationData data in _reputationCache)
        {
            if (data.ReputationGrade == grade)
            {
                return data;
            }
        }

        return null;
    }

    /// <summary> KDH : 현재 평판의 다음 평판 데이터를 반환한다. </summary>
    public ReputationData GetNextReputation(int currentGrade)
    {
        return GetReputationByGrade(currentGrade + 1);
    }

    /// <summary> KDH : 누적 만족도에 해당하는 평판 등급을 반환한다. </summary>
    public int GetReputationGrade(int satisfaction)
    {
        ReputationData data = GetReputationBySatisfaction(satisfaction);

        return data?.ReputationGrade ?? -1;
    }

    /// <summary> KDH : 현재 누적 만족도에 해당하는 손님 수를 반환한다. </summary>
    public int GetCustomerCount(int satisfaction)
    {
        ReputationData data = GetReputationBySatisfaction(satisfaction);
        
        return data?.CustomerCount ?? 0;
    }
    
    /// <summary> KDH : 현재 평판이 최대 등급인지 반환한다. </summary>
    public bool IsMaxReputationGrade(int currentGrade)
    {
        if (_reputationCache == null || _reputationCache.Length == 0)
        {
            return false;
        }

        return currentGrade >= _reputationCache[^1].ReputationGrade;
    }
    
    /// <summary> KDH : 누적 만족도에 해당하는 현재 평판 데이터를 반환한다. </summary>
    private ReputationData GetReputationBySatisfaction(int satisfaction)
    {
        ReputationData currentReputation = null;
        
        foreach (ReputationData data in _reputationCache)
        {
            if (data.RequiredSatisfaction > satisfaction)
            {
                break;
            }

            // 현재 만족도에 해당하는 가장 높은 평판 등급을 찾는다.
            currentReputation = data;
        }

        return currentReputation;
    }
}