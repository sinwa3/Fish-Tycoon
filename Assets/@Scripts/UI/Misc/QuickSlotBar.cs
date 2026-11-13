using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 소비 아이템 퀵슬롯을 생성하고 보유 정보에 따라 UI를 갱신한다. </summary>
public class QuickSlotBar : MonoBehaviour
{
    private const int RedButtonItemID = 15;

    private readonly List<QuickSlot> _slots = new();

    private ItemTooltip _itemTooltip;

    private int _activeSlotCount;
    private int _selectedSlotIndex = -1;
    private bool _isSelectionEnabled;

    /// <summary> KDH : 현재 선택한 퀵슬롯에 사용 가능한 소비 아이템이 있는지 확인한다. </summary>
    public bool HasUsableConsumable
    {
        get
        {
            if (!_isSelectionEnabled || _selectedSlotIndex < 0 || Managers.Data == null)
            {
                return false;
            }

            QuickSlotData slot = Managers.Data.GetQuickSlot(_selectedSlotIndex);

            return slot is { Type: QuickSlotItemType.Consumable, Count: > 0 };
        }
    }
    
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

    /// <summary> KDH : 잠입 HUD에서 전달한 입력으로 퀵슬롯을 선택하거나 소비 아이템을 사용한다. </summary>
    public void OnInputKey()
    {
        InputManager input = Managers.Input;

        if (!_isSelectionEnabled || !isActiveAndEnabled || input is not { CanReceivePlayer: true } || _activeSlotCount == 0)
        {
            return;
        }

        // 키보드 입력으로 슬롯 전환
        int slotIndex = input.KeyDown_1 ? 0
            : input.KeyDown_2 ? 1
            : input.KeyDown_3 ? 2
            : input.KeyDown_4 ? 3
            : input.KeyDown_5 ? 4
            : input.KeyDown_6 ? 5
            : input.KeyDown_7 ? 6
            : input.KeyDown_8 ? 7
            : input.KeyDown_9 ? 8
            : input.KeyDown_0 ? 9
            : -1;

        if (slotIndex >= 0)
        {
            SelectSlot(slotIndex);
            return;
        }

        if (input.MouseDown_Right)
        {
            UseSelectedConsumable();
            return;
        }
        
        // 마우스 휠로 선택 슬롯 전환
        if (Cursor.lockState != CursorLockMode.Locked || Mathf.Approximately(input.MouseWheel, 0f))
        {
            return;
        }

        MoveSelection(input.MouseWheel > 0f ? -1 : 1);
    }

    /// <summary> KDH : 선택한 소비 아이템을 한 개 차감하고 사용 안내를 표시한다. </summary>
    private void UseSelectedConsumable()
    {
        if (!HasUsableConsumable)
        {
            return;
        }

        QuickSlotData slot = Managers.Data.GetQuickSlot(_selectedSlotIndex);

        string itemName = Managers.Table?.Item?.GetItem(slot.ItemID)?.Name ?? "소비 아이템";

        if (slot.ItemID == RedButtonItemID)
        {
            OpenUseConfirmPopup(_selectedSlotIndex, itemName);
            return;
        }

        UseConsumable(_selectedSlotIndex, itemName);
    }

    /// <summary> KYH : 사용 확인 팝업을 띄우고 Yes를 누르면 아이템을 사용한다 </summary>
    private void OpenUseConfirmPopup(int slotIndex, string itemName)
    {
        Managers.UI.OpenPopup<AlertPopup>().Set(itemName, "정말로 사용하시겠습니까?", true,
            () => UseConsumable(slotIndex, itemName), true);
    }

    /// <summary> KYH : 지정한 칸의 소비 아이템을 한 개 차감하고 사용 안내를 표시한다 </summary>
    private void UseConsumable(int slotIndex, string itemName)
    {
        if (Managers.Data.TryUseConsumable(slotIndex))
        {
            Managers.UI?.OpenToastMessage($"{itemName} 아이템을 사용했습니다.");
        }
    }

    /// <summary> KDH : 지정한 퀵슬롯을 선택하고 이전 슬롯의 선택 표시를 해제한다. </summary>
    public void SelectSlot(int slotIndex)
    {
        if (!_isSelectionEnabled || slotIndex < 0 || slotIndex >= _activeSlotCount || _selectedSlotIndex == slotIndex)
        {
            return;
        }

        if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _slots.Count)
        {
            _slots[_selectedSlotIndex].SetSelected(false);
        }

        _selectedSlotIndex = slotIndex;
        _slots[_selectedSlotIndex].SetSelected(true);
        _itemTooltip?.Hide();
    }

    /// <summary> KDH : 처음과 마지막 슬롯을 순환하며 선택 위치를 이동한다. </summary>
    private void MoveSelection(int direction)
    {
        if (_activeSlotCount == 0 || direction == 0)
        {
            return;
        }

        int nextIndex = (_selectedSlotIndex + (direction > 0 ? 1 : -1) + _activeSlotCount) % _activeSlotCount;
        SelectSlot(nextIndex);
    }

    /// <summary> KDH : 잠입 HUD에서만 퀵슬롯 선택 기능과 선택 표시를 활성화한다. </summary>
    public void SetSelectionEnabled(bool isEnabled)
    {
        _isSelectionEnabled = isEnabled;

        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].SetSelected(isEnabled && i == _selectedSlotIndex);
        }
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

            slot.SetTooltip(_itemTooltip);

            slot.SetSlotIndex(_slots.Count);
            _slots.Add(slot);

            // 툴팁을 마지막에 위치시킨다.
            if (_slots.Count == slotCount)
            {
                _itemTooltip.transform.SetAsLastSibling();
            }
        }

        _activeSlotCount = Mathf.Min(slotCount, _slots.Count);
        _selectedSlotIndex = _activeSlotCount > 0 ? Mathf.Clamp(_selectedSlotIndex, 0, _activeSlotCount - 1) : -1;

        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].gameObject.SetActive(i < _activeSlotCount);
            _slots[i].SetSelected(_isSelectionEnabled && i == _selectedSlotIndex);

            if (i < _activeSlotCount)
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
