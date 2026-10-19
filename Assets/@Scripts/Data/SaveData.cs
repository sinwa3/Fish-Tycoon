using System;
using System.Collections.Generic;

/// <summary> KDH : 게임 모드를 정의한다. </summary>
public enum GameMode
{
    Tycoon,  // 타이쿤 모드
    Stealth, // 잠입 모드
}

/// <summary> KDH : 게임의 로컬 세이브 데이터를 정의한다. </summary>
[Serializable]
public class SaveData
{
    public int Gold;
    public int Satisfaction;
    public int CurrentDay = 1;
    
    public List<InventoryFishData> InvenFish = new();

    /// <summary> KDH : 저장했을 당시의 게임모드 (Tycoon - 영업 후 시작, Stealth - 영업 전 시작) </summary>
    public GameMode SavedGameMode =  GameMode.Stealth;
}