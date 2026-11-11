/// <summary> YSW, KDH : 플레이어가 상호작용하는 오브젝트의 종류. 홀딩 UI 아이콘 표시와 홀드 여부 판단에 사용한다. </summary>
public enum InteractionType
{
    None,
    Door,         // 방 문 열기/닫기 (홀드)
    Furniture,    // 서랍, 수납장 열기/닫기 (홀드)
    Wardrobe,     // 장롱에 숨기 (홀드)
    WardrobeExit, // 장롱에서 나오기 (즉발)
    Fish,         // 생선 줍기 (홀드)
    ExitDoor,     // 현관문 나가기 (홀드)
    Throwable,    // 던지기 물건 줍기 (즉발)
    Lock,         // 미니게임 실행 (홀드)
}
