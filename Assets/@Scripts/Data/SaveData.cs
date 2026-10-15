using System;
using System.Collections.Generic;

/// <summary> KDH : 게임의 로컬 세이브 데이터를 정의한다. </summary>
[Serializable]
public class SaveData
{
    public int Gold;
    public int Satisfaction;
    public List<InventoryItemData> Inventory = new();
    public GameMode CurrentGameMode;
}