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

    /// <summary> 현재 보유 중인 생선 정보 </summary>
    public List<InventoryFishData> InvenFish = new();

    /// <summary> 현재 장착 중인 장비 정보 </summary>
    public EquipmentData Equipment = new();

    /// <summary> 잠입 중 사용하는 퀵슬롯 정보 </summary>
    public List<QuickSlotData> QuickSlots = new();

    /// <summary> 저장 당시 게임 모드 </summary>
    public GameMode SavedGameMode = GameMode.Stealth;
}