using System;

/// <summary> KDH : 퀵슬롯에 보관된 아이템 정보를 관리한다. </summary>
[Serializable]
public class QuickSlotData
{
    public QuickSlotItemType Type;
    public int ItemID = -1;
    public int Count;
}