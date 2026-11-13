using Unity.VisualScripting;
using UnityEngine;

/// <summary> SJY : 퀵슬롯에 보관된 락픽의 수량 조회 및 소모 기능 </summary>
public static class LockpickInventory
{
    private const int LockpickItemID = 11;

    /// <summary> SJY : 현재 보유한 락픽 총개수 반환 </summary>
    public static int GetCount()
    {
        if (Managers.Data == null)
        {
            return 0;
        }

        int count = 0;

        for (int i = 0; i < Managers.Data.GetQuickSlotCount(); i++)
        {
            QuickSlotData slot = Managers.Data.GetQuickSlot(i);

            if (slot == null || slot.Type != QuickSlotItemType.Consumable || slot.ItemID != LockpickItemID)
            {
                continue;
            }

            count += slot.Count;
        }

        return count;
    }

    /// <summary> SJY : 락픽을 하나 이상 소지하고 있는지 확인 </summary>
    public static bool HasLockpick()
    {
        return GetCount() > 0;
    }

    /// <summary> SJY : 락픽이 들어 있는 슬롯에서 1개 소모 </summary>
    public static bool TryConsume()
    {
        if (Managers.Data == null)
        {
            return false;
        }

        for (int i = 0; i < Managers.Data.GetQuickSlotCount(); i++)
        {
            QuickSlotData slot = Managers.Data.GetQuickSlot(i);

            if (slot == null || slot.Type != QuickSlotItemType.Consumable || slot.ItemID != LockpickItemID || slot.Count <= 0)
            {
                continue;
            }

            return Managers.Data.TryUseConsumable(i);
        }

        return false;
    }
}
