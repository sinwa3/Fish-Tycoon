using TMPro;
using UnityEngine;

/// <summary> KDH : 아이템의 이름과 설명을 표시하는 공용 툴팁이다. </summary>
public class ItemTooltip : BaseTooltip
{
    private enum Texts
    {
        NameText,
        DescText,
    }

    private QuickSlotData _quickSlotData;

    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
    }

    /// <summary> KDH : 표시할 생선 데이터를 설정한다. </summary>
    public void Show(QuickSlotData data, RectTransform trigger)
    {
        if (data == null)
        {
            return;
        }

        _quickSlotData = data;

        base.Show(trigger);
    }

    /// <summary> KDH : 아이템 이름과 설명을 갱신한다. </summary>
    protected override void UpdateContent()
    {
        if (_quickSlotData == null)
        {
            return;
        }

        if (_quickSlotData.Type == QuickSlotItemType.None || _quickSlotData.ItemID < 0 || _quickSlotData.Count <= 0)
        {
            return;
        }

        switch (_quickSlotData.Type)
        {
            case QuickSlotItemType.Consumable:
                Item item = Managers.Table.Item?.GetItem(_quickSlotData.ItemID);
                if (item != null)
                {
                    SetItemInfoUI(item.Name, item.Description);
                }

                break;
            case QuickSlotItemType.Fish:
                FishData fishData = Managers.Table.Fish?.GetFish(_quickSlotData.ItemID);
                if (fishData != null)
                {
                    SetItemInfoUI(fishData.Name);
                }

                break;
        }
    }

    /// <summary> KDH : 툴팁에 표시할 아이템 정보를 UI에 세팅한다. </summary>
    public void SetItemInfoUI(string itemName, string description = null)
    {
        Get<TMP_Text>(Texts.NameText)?.SetText(itemName);

        if (!string.IsNullOrEmpty(description))
        {
            Get<TMP_Text>(Texts.DescText)?.gameObject.SetActive(true);
            Get<TMP_Text>(Texts.DescText)?.SetText(description);
        }
        else
        {
            Get<TMP_Text>(Texts.DescText)?.gameObject.SetActive(false);
        }
    }
}