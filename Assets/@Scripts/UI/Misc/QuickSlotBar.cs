using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 소비 아이템 퀵슬롯을 생성하고 보유 정보에 따라 UI를 갱신한다. </summary>
public class QuickSlotBar : MonoBehaviour
{
    private readonly List<QuickSlot> _slots = new();

    private ItemTooltip _itemTooltip;

    private void Awake()
    {
        InitializeItemTooltip();
    }

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
            slot.SetTooltip(_itemTooltip);

            if (slot == null)
            {
                break;
            }

            slot.SetSlotIndex(_slots.Count);
            _slots.Add(slot);

            // 툴팁을 마지막에 위치시킨다.
            if (_slots.Count == slotCount)
            {
                _itemTooltip.transform.SetAsLastSibling();
            }
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
    
    /// <summary> KDH : 아이템 설명 툴팁을 초기화한다. </summary>
    private void InitializeItemTooltip()
    {
        if (_itemTooltip == null)
        {
            _itemTooltip = Managers.Resource.Spawn(ResourceKey.Path.TooltipUI + nameof(ItemTooltip),
                transform).gameObject.GetOrAddComponent<ItemTooltip>();
            
            LayoutElement layoutElement = _itemTooltip.gameObject.GetOrAddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
        }
        
        _itemTooltip.gameObject.SetActive(false);
    }
}