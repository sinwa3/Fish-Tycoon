using System;

/// <summary> KDH : 인벤토리에 저장되는 개별 아이템 정보를 관리한다. </summary>
[Serializable]
public class InventoryItemData
{
    public int ItemID;
    public int Count;

    /// <summary> KDH : 인벤토리에서 아이템이 위치한 슬롯 인덱스다. </summary>
    public int InvenSlotIndex = -1;

    /// <summary> KDH : QuickSlot에 등록된 슬롯 인덱스다. </summary>
    public int QuickSlotIndex = -1;
}