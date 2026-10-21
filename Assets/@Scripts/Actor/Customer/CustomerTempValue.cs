/// <summary> KYH : 손님 임시 수치 모음 (데이터 테이블이 생기면 테이블 값으로 바꾼다) </summary>
public static class CustomerTempValue
{
    // TODO : 값은 테스트하면서 조정
    public const float OrderDuration = 1.0f;
    // 대기시간 100%. 기다린 시간은 재는 값이고 이건 그 최대값
    public const float MaxWaitDuration = 20.0f;
    // 남은 대기시간 비율이 이 값 이하면 절반 배율
    public const float HalfMultiplierRatio = 0.5f;
    public const float ZeroMultiplier = 0.0f;
    public const float HalfMultiplier = 0.5f;
    public const float FullMultiplier = 1.0f;
    public const float EatDuration = 3.0f;
    // 김초밥 메뉴 ID (음식 테이블 ID 0과 같음)
    public const int BasicSushiId = 0;
    public const float PayDuration = 1.0f;
    public const float SpawnInterval = 2.0f;
    public const int DailyCustomerCount = 10;
}
