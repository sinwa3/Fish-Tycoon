/// <summary> KYH : 손님 임시 수치 모음 (데이터 테이블이 생기면 테이블 값으로 바꾼다) </summary>
public static class CustomerTempValue
{
    // TODO : 값은 테스트하면서 조정. 대기시간은 정하는 값이 아니라 재는 값이라 여기 없음
    public const float OrderDuration = 1.0f;
    public const float EatDuration = 3.0f;
    // 김초밥 메뉴 ID (음식 테이블 ID 0과 같음)
    public const int BasicSushiId = 0;
    public const float PayDuration = 1.0f;
    public const float SpawnInterval = 2.0f;
    public const int DailyCustomerCount = 10;
}
