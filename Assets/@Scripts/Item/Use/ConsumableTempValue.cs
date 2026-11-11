/// <summary> KYH : 소비 아이템 임시 수치 모음 (데이터 테이블에 넣으면 테이블 값으로 바꾼다) </summary>
public static class ConsumableTempValue
{
    // 병이 깨질 때 소음 크기 (들리는 반경 = 크기 x 5m)
    public const float BottleNoiseLoudness = 2.0f;
    // 법선 벡터 최소 y
    public const float FloorNormalMinY = 0.9f;
    // 바닥에 못 닿은 병이 사라지기까지 시간
    public const float BottleAutoDestroyTime = 10.0f;
}
