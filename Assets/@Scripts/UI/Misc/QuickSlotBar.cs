using System.Collections.Generic;
using UnityEngine;

/// <summary> KDH : 소비 아이템 퀵슬롯을 생성하고 보유 정보에 따라 UI를 갱신한다. </summary>
public class QuickSlotBar : MonoBehaviour
{
    private readonly List<QuickSlot> _slots = new();

    private void OnEnable()
    {
        if (Managers.Data == null)
        {
            return;
        }
        
        Managers.Data.OnQuickSlotChanged += RefreshSlots;
        RefreshSlots();
    }

    private void OnDisable()
    {
        if (Managers.Data == null)
        {
            return;
        }
        
        Managers.Data.OnQuickSlotChanged -= RefreshSlots;
    }

    /// <summary> KDH : 현재 소비 아이템 슬롯 개수에 맞춰 UI를 생성하고 갱신한다. </summary>
    private void RefreshSlots()
    {
        int slotCount = Managers.Data.GetQuickSlotCount();

        while (_slots.Count < slotCount)
        {
            QuickSlot slot = Managers.UI.MakeSlot<QuickSlot>(transform);

            if (slot == null)
            {
                break;
            }

            slot.SetSlotIndex(_slots.Count);
            _slots.Add(slot);
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].gameObject.SetActive(i < slotCount);

            if (i < slotCount)
            {
                _slots[i].UpdateUI();
            }
        }
    }
}