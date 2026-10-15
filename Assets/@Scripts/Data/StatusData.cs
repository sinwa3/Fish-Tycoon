using System;

/// <summary> KDH : 게임 모드를 정의한다. </summary>
public enum GameMode
{
    Tycoon,  // 타이쿤 모드
    Stealth, // 잠입 모드
}

/// <summary> KDH : 게임 모드와 진행 상태 데이터를 저장한다. </summary>
[Serializable]
public class GameModeData
{
    /// <summary> KDH : 현재 게임 모드 </summary>
    public GameMode Mode;

    /// <summary> KDH : 현재 진행 일차 </summary>
    public int CurrentDay;
}