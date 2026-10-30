/// <summary>각 핀의 상태 열거형</summary>
public enum PinLockState
{
    Idle,               // 기본 상태
    Falling,            // 핀이 내려가는 중
    BottomWindow,       // 바닥에서 고정 가능 상태
    Returning,          // 다시 위로 올라가는 중
    Fixed               // 고정된 상태
}
