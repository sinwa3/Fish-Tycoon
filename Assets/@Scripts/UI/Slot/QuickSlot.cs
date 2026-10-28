using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 퀵슬롯에 등록된 소비 아이템 또는 생선의 아이콘과 보유 개수를 표시한다. </summary>
public class QuickSlot : SlotUI
{
    private enum Images
    {
        ItemIcon,
    }

    private enum Texts
    {
        CountText,
    }

    private int _slotIndex = -1;

    private ItemTooltip _itemTooltip;
    
    private void Awake()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }
    
    /// <summary> KDH : 현재 슬롯의 아이템 종류에 맞춰 UI를 갱신한다. </summary>
    public override void UpdateUI()
    {
        if (_slotIndex < 0)
        {
            Clear();
            return;
        }

        QuickSlotData slotData = Managers.Data.GetQuickSlot(_slotIndex);

        if (slotData == null || slotData.Type == QuickSlotItemType.None || slotData.ItemID < 0 || slotData.Count <= 0)
        {
            Clear();
            return;
        }

        Sprite sprite = slotData.Type switch
        {
            QuickSlotItemType.Consumable => GetConsumableSprite(slotData.ItemID),
            QuickSlotItemType.Fish => GetFishSprite(slotData.ItemID),
            _ => null
        };

        if (sprite == null)
        {
            Clear();
            return;
        }

        Image itemIcon = Get<Image>(Images.ItemIcon);

        if (itemIcon != null)
        {
            itemIcon.sprite = sprite;
            itemIcon.enabled = true;
        }

        Get<TMP_Text>(Texts.CountText)?.SetText(slotData.Count.ToString());
        
        // 툴팁 표시 이벤트를 연결한다.
        SubscribeItemTooltip();
    }

    /// <summary> KDH : 툴팁을 연결한다. </summary>
    public void SetTooltip(ItemTooltip tooltip)
    {
        _itemTooltip = tooltip;
    }
    
    /// <summary> KDH : 퀵슬롯 인덱스를 설정하고 UI를 갱신한다. </summary>
    public void SetSlotIndex(int slotIndex)
    {
        _slotIndex = slotIndex;
        
        UpdateUI();
    }
    
    /// <summary> KDH : 소비 아이템의 아이콘을 반환한다. </summary>
    private Sprite GetConsumableSprite(int itemID)
    {
        Item item = Managers.Table.Item.GetItem(itemID);

        if (item is not { Type: ItemType.Consumable })
        {
            return null;
        }

        return Managers.Resource.LoadSprite(ResourceKey.Path.ItemSprite + ResourceKey.Name.ShopIcon, item.ResourceName);
    }

    /// <summary> KDH : 생선의 아이콘을 반환한다. </summary>
    private Sprite GetFishSprite(int fishID)
    {
        return Managers.Table.Fish.GetSprite(fishID);
    }

    /// <summary> KDH : 퀵슬롯의 아이템 표시를 초기화한다. </summary>
    private void Clear()
    {
        Image itemIcon = Get<Image>(Images.ItemIcon);

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        Get<TMP_Text>(Texts.CountText)?.SetText(string.Empty);
    }
    
    #region ===== 이벤트 =====
    
    /// <summary> KDH : 툴팁 표시 이벤트를 연결한다. </summary>
    private void SubscribeItemTooltip()
    {
        gameObject.ClearEvent();

        gameObject.BindEvent(UIEventType.PointerEnter, OnPointerEnterItem);
        gameObject.BindEvent(UIEventType.PointerExit, OnPointerExitItem);
    }
    
    /// <summary> KDH : 마우스가 아이템 아이콘에 올라오면 아이템 정보를 표시한다. </summary>
    private void OnPointerEnterItem()
    {
        _itemTooltip?.Show(Managers.Data.GetQuickSlot(_slotIndex), transform as RectTransform);
    }

    /// <summary> KDH : 마우스가 평판 아이콘에서 벗어나면 아이템 정보를 숨긴다. </summary>
    private void OnPointerExitItem()
    {
        _itemTooltip?.Hide();
    }
    
    #endregion ===== 이벤트 =====
}