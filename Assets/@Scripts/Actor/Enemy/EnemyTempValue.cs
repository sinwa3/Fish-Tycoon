/// <summary> KYH : 집주인 임시 수치 모음 (데이터 테이블이 생기면 테이블 값으로 바꾼다) </summary>
public static class EnemyTempValue
{
    // TODO : 값은 테스트하면서 조정
    public const float WalkSpeed = 1.5f;
    public const float RunSpeed = 4.0f;
    // 목표에서 이 거리 안에 바닥이 없으면 갈 수 없는 곳으로 본다
    public const float SampleMaxDistance = 1.0f;
    // 초당 회전 각도
    public const float TurnSpeed = 180.0f;
    // 두리번 때 처음 방향에서 좌우로 도는 각도
    public const float LookAroundAngle = 60.0f;

    // 시야 거리, 시야각(정면 기준 전체 각도)
    public const float SightDistance = 10.0f;
    public const float SightAngle = 110.0f;
    // 발에서 눈까지 높이
    public const float EyeHeight = 1.5f;
    // 플레이어 콜라이더 꼭대기에서 이만큼 아래를 본다
    public const float SightTargetTopOffset = 0.1f;
    // 웅크린 플레이어를 볼 때 시야 거리 배율
    public const float CrouchSightMultiplier = 0.6f;
    // 시야 검사 간격(초)
    public const float SightCheckInterval = 0.2f;
    // 시야 자극 세기
    public const float SightStimulusStrength = 1.0f;

    // 추격 중 목적지를 다시 잡는 간격(초)
    public const float ChaseRepathInterval = 0.2f;
    // 놓친 뒤 이 시간 동안은 실제 플레이어 위치로 쫓는다
    public const float ChaseTrackDuration = 1.0f;
    // 이 시간 동안 못 보면 놓친 것으로 본다
    public const float LoseSightDuration = 3.0f;
    // 이 거리 안에서 보이면 잡는다 (수평 거리)
    public const float CatchDistance = 1.2f;

    // 조사 목표에서 두리번거리는 시간(초)
    public const float InvestigateLookAroundDuration = 4.0f;

    // 의심 게이지 최대 (발각)
    public const float MaxGauge = 100.0f;
    // 보일 때 초당 상승량 (바로 앞, 시야 끝)
    public const float SightGaugeRiseNear = 80.0f;
    public const float SightGaugeRiseFar = 20.0f;
    // 안 보일 때 초당 감소량
    public const float GaugeDecayRate = 10.0f;
    // 소리로 오를 수 있는 최대 (발각 바로 아래)
    public const float SoundGaugeCap = 99.0f;
    // 단계마다 올라가는 값, 내려가는 값
    public const float SuspiciousEnterGauge = 30.0f;
    public const float SuspiciousExitGauge = 20.0f;
    public const float AlertEnterGauge = 60.0f;
    public const float AlertExitGauge = 50.0f;
    public const float DetectedEnterGauge = 100.0f;
    public const float DetectedExitGauge = 80.0f;

    // 크기 1당 들리는 반경(m)
    public const float SoundRangePerLoudness = 5.0f;
    // 벽, 닫힌 문 하나를 지날 때마다 곱하는 값
    public const float WallSoundMultiplier = 0.5f;
    public const float DoorSoundMultiplier = 0.7f;
    // 들린 세기가 이 값보다 작으면 못 들은 것으로 본다
    public const float SoundHearThreshold = 0.1f;
    // 들린 세기 1당 게이지 상승량
    public const float SoundGaugePerLoudness = 25.0f;
    // 소리 위치를 바닥에서 띄우는 높이 (바닥에 레이가 맞지 않게)
    public const float SoundSourceHeight = 0.1f;
}
