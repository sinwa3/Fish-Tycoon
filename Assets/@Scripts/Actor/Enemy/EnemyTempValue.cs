/// <summary> KYH : 테이블에 넣지 않는 집주인 수치 모음 </summary>
public static class EnemyTempValue
{
    // TODO : 값은 테스트하면서 조정
    // 목표에서 이 거리 안에 바닥이 없으면 갈 수 없는 곳으로 본다
    public const float SampleMaxDistance = 1.0f;
    // 초당 회전 각도
    public const float TurnSpeed = 180.0f;
    // 두리번 때 처음 방향에서 좌우로 도는 각도
    public const float LookAroundAngle = 60.0f;

    // 발에서 눈까지 높이
    public const float EyeHeight = 1.5f;
    // 플레이어 콜라이더 꼭대기에서 이만큼 아래를 본다
    public const float SightTargetTopOffset = 0.1f;
    // 시야 검사 간격(초)
    public const float SightCheckInterval = 0.2f;
    // 시야 자극 세기
    public const float SightStimulusStrength = 1.0f;

    // 추격 중 목적지를 다시 잡는 간격(초)
    public const float ChaseRepathInterval = 0.2f;
    // 숨기 직전 이 시간 안에 봤으면 숨는 걸 본 것으로 친다 (시야 검사 간격보다 길어야 한다)
    public const float HideWitnessWindow = SightCheckInterval * 1.5f;

    // 이 거리 안의 닫힌 문을 보면 멈춰서 연다
    public const float DoorDetectDistance = 1.0f;
    // 문을 열고 지나가기 전까지 기다리는 시간(초)
    public const float DoorOpenWaitDuration = 0.5f;

    // 의심 게이지 최대 (발각)
    public const float MaxGauge = 100.0f;
    // 소리로 오를 수 있는 최대 (발각 바로 아래)
    public const float SoundGaugeCap = 99.0f;
    // 발각 단계로 올라가는 값
    public const float DetectedEnterGauge = 100.0f;

    // 소리 위치를 바닥에서 띄우는 높이 (바닥에 레이가 맞지 않게)
    public const float SoundSourceHeight = 0.1f;
}
