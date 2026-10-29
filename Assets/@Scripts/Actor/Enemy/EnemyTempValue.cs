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
    public const float EyeHeight = 1.6f;
    // 플레이어 콜라이더 꼭대기에서 이만큼 아래를 본다
    public const float SightTargetTopOffset = 0.1f;
    // 웅크린 플레이어를 볼 때 시야 거리 배율
    public const float CrouchSightMultiplier = 0.6f;
    // 시야 검사 간격(초)
    public const float SightCheckInterval = 0.2f;
    // 시야 자극 세기
    public const float SightStimulusStrength = 1.0f;
}
