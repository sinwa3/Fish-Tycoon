#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Reflection;

/// <summary> 치트에서 퀵슬롯을 찾고 비우는 기능 (DataManager에 빼는 공개 메서드가 없다) </summary>
public static class CheatQuickSlot
{
    // 퀵슬롯 UI는 이 이벤트로 갱신되는데 DataManager 밖에서 부를 수 없어 리플렉션으로 부른다
    private static readonly FieldInfo QuickSlotChangedField = typeof(DataManager).GetField(nameof(DataManager.OnQuickSlotChanged), BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo StoreChangedField = typeof(DataManager).GetField(nameof(DataManager.OnStoreChanged), BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary> 그 종류와 ID가 든 퀵슬롯을 돌려준다 (없으면 null) </summary>
    public static QuickSlotData Find(QuickSlotItemType type, int itemID)
    {
        int slotCount = Managers.Data.GetQuickSlotCount();

        for (int i = 0; i < slotCount; i++)
        {
            QuickSlotData slot = Managers.Data.GetQuickSlot(i);

            if (slot != null && slot.Type == type && slot.ItemID == itemID)
            {
                return slot;
            }
        }

        return null;
    }

    /// <summary> 그 퀵슬롯의 개수를 돌려준다 (없으면 0) </summary>
    public static int GetCount(QuickSlotItemType type, int itemID)
    {
        return Find(type, itemID)?.Count ?? 0;
    }

    /// <summary> 퀵슬롯에서 개수만큼 빼고 0개가 되면 칸을 비운 뒤 UI를 갱신한다 </summary>
    public static void Remove(QuickSlotItemType type, int itemID, int count)
    {
        QuickSlotData slot = Find(type, itemID);

        if (slot == null)
        {
            return;
        }

        slot.Count -= Math.Min(count, slot.Count);

        if (slot.Count == 0)
        {
            slot.Type = QuickSlotItemType.None;
            slot.ItemID = -1;
        }

        (QuickSlotChangedField?.GetValue(Managers.Data) as Action)?.Invoke();
        (StoreChangedField?.GetValue(Managers.Data) as Action)?.Invoke();
    }
}
#endif
