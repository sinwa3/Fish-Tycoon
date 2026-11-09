/// <summary> KYH : 테이블 1행의 칼럼명과 일치확인, 일차별 집주인 감지, 행동 수치 </summary>
public class EnemyDetectionData : TableData
{
    [CsvField("일차")]
    public int Day;

    [CsvField("시야 거리")]
    public float SightDistance;

    [CsvField("시야각")]
    public float SightAngle;

    [CsvField("근거리 시야 게이지 상승")]
    public float SightGaugeRiseNear;

    [CsvField("원거리 시야 게이지 상승")]
    public float SightGaugeRiseFar;

    [CsvField("게이지 감소")]
    public float GaugeDecayRate;

    [CsvField("소리 게이지 환산")]
    public float SoundGaugePerLoudness;

    [CsvField("걷기 속도")]
    public float WalkSpeed;

    [CsvField("달리기 속도")]
    public float RunSpeed;

    [CsvField("놓침 시간")]
    public float LoseSightDuration;

    [CsvField("수색 시간")]
    public float InvestigateLookAroundDuration;

    [CsvField("웅크림 시야 배율")]
    public float CrouchSightMultiplier;

    [CsvField("잡는 거리")]
    public float CatchDistance;

    [CsvField("추적 유지 시간")]
    public float ChaseTrackDuration;

    [CsvField("소리 범위")]
    public float SoundRangePerLoudness;

    [CsvField("벽 감쇠")]
    public float WallSoundMultiplier;

    [CsvField("문 감쇠")]
    public float DoorSoundMultiplier;

    [CsvField("들리는 기준값")]
    public float SoundHearThreshold;

    [CsvField("의심 시작 게이지")]
    public float SuspiciousEnterGauge;

    [CsvField("의심 해제 게이지")]
    public float SuspiciousExitGauge;

    [CsvField("조사 시작 게이지")]
    public float AlertEnterGauge;

    [CsvField("조사 해제 게이지")]
    public float AlertExitGauge;

    [CsvField("발각 해제 게이지")]
    public float DetectedExitGauge;
}

/// <summary> KYH : 집주인 감지 수치 테이블을 관리한다 </summary>
public class EnemyDetectionTable : TableLoader<EnemyDetectionData>
{
    protected override string TableName => "EnemyDetectionTable";

    /// <summary> KYH : 일차에 해당하는 감지 수치를 돌려준다 </summary>
    public EnemyDetectionData GetDetectionByDay(int day)
    {
        foreach (EnemyDetectionData data in Map.Values)
        {
            if (data.Day == day)
            {
                return data;
            }
        }

        CPrint.Warning($"[EnemyDetectionTable] {day}일 차 없음");

        return null;
    }
}
