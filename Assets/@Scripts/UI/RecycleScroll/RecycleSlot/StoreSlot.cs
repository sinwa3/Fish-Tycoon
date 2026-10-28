using TMPro;
using UnityEngine.UI;

/// <summary> KDH : 상점 상품 정보와 구매 상태를 표시하고 구매 요청을 처리한다. </summary>
public class StoreSlot : BaseRecycleSlot<ShopData>
{
    public enum Texts
    {
        ItemNameText,
        ItemDescText,
        PriceText,
        DimText,
        PurchaseBtnText,
    }

    public enum Images
    {
        ItemImage,
        DimImage,
    }

    public enum Buttons
    {
        PurchaseBtn,
    }

    private Item _item;

    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        Get<Button>(Buttons.PurchaseBtn)?.onClick.AddListener(OnClickPurchase);
    }

    /// <summary> KDH : 재활용 슬롯에 상품 정보와 구매 상태를 표시한다. </summary>
    public override void UpdateData(ShopData shopData)
    {
        base.UpdateData(shopData);

        _item = null;

        if (shopData == null)
        {
            return;
        }

        _item = Managers.Table.Item.GetItem(shopData.ItemID);

        if (_item == null)
        {
            return;
        }

        Get<TMP_Text>(Texts.ItemNameText)?.SetText(_item.Name);
        Get<TMP_Text>(Texts.ItemDescText)?.SetText(_item.Description);
        Get<TMP_Text>(Texts.PriceText)?.SetText($"{shopData.SellPrice:N0} G");

        Image itemImage = Get<Image>(Images.ItemImage);

        if (itemImage)
        {
            Get<Image>(Images.ItemImage).sprite = Managers.Table.Item.GetSprite(_item.ID);
        }

        RefreshPurchaseStatus();
    }

    /// <summary> KDH : 현재 상품 구매를 요청하고 결과에 따라 UI를 갱신한다. </summary>
    private void TryPurchaseItem()
    {
        if (_item == null)
        {
            return;
        }

        PurchaseStatus result = Managers.Data.TryPurchaseItem(_item.ID);

        switch (result)
        {
            case PurchaseStatus.Available:
                Managers.UI.OpenToastMessage($"{_item.Name} 구매 완료");
                break;

            case PurchaseStatus.InsufficientGold:
                Managers.UI.OpenToastMessage("소지금이 부족합니다.");
                break;

            case PurchaseStatus.PreviousTierRequired:
                Managers.UI.OpenToastMessage(GetRequiredEquipmentText());
                break;

            case PurchaseStatus.AlreadyPurchased:
                Managers.UI.OpenToastMessage("이미 구매한 장비입니다.");
                break;

            case PurchaseStatus.QuantityLimit:
                Managers.UI.OpenToastMessage("더 이상 보유할 수 없습니다.");
                break;

            case PurchaseStatus.NoEmptySlot:
                Managers.UI.OpenToastMessage("빈 퀵슬롯이 없습니다.");
                break;

            default:
                Managers.UI.OpenToastMessage("구매할 수 없는 상품입니다.");
                break;
        }
    }

    /// <summary> KDH : 현재 구매 상태에 따라 버튼과 Dim UI를 갱신한다. </summary>
    public void RefreshPurchaseStatus()
    {
        if (_item == null)
        {
            return;
        }

        PurchaseStatus status = Managers.Data.GetPurchaseStatus(_item);

        Button purchaseButton = Get<Button>(Buttons.PurchaseBtn);
        if (purchaseButton != null)
        {
            purchaseButton.interactable = status == PurchaseStatus.Available;
        }

        Get<TMP_Text>(Texts.PurchaseBtnText)?.SetText(GetPurchaseButtonText(status));
        Get<TMP_Text>(Texts.DimText)?.SetText(GetDimText(status));

        bool showDim = status is PurchaseStatus.PreviousTierRequired or PurchaseStatus.NotForSale;
        Get<Image>(Images.DimImage)?.gameObject.SetActive(showDim);
    }

    /// <summary> KDH : 구매 상태에 따른 버튼 문구를 반환한다. </summary>
    private string GetPurchaseButtonText(PurchaseStatus status)
    {
        // 장비인 경우 이미 구매한 상태일 때는 장착 중 유무를 체크한다.
        if (_item?.Type == ItemType.Equipment && status == PurchaseStatus.AlreadyPurchased)
        {
            int equippedItemID = Managers.Data.GetEquippedItemID(_item.EquipType);
            return equippedItemID == _item.ID ? "장착 중" : "구매 완료";
        }
        
        return status switch
        {
            PurchaseStatus.Available => "구매",
            PurchaseStatus.AlreadyPurchased => "구매 완료",
            PurchaseStatus.InsufficientGold => "잔액 부족",
            PurchaseStatus.PreviousTierRequired => "구매",
            PurchaseStatus.QuantityLimit => "보유 한도",
            PurchaseStatus.NoEmptySlot => "슬롯 부족",
            PurchaseStatus.NotForSale => "판매 중지",
            _ => "구매"
        };
    }

    /// <summary> KDH : 구매 제한 상태에 따른 Dim 안내 문구를 반환한다. </summary>
    private string GetDimText(PurchaseStatus status)
    {
        return status switch
        {
            PurchaseStatus.PreviousTierRequired => GetRequiredEquipmentText(),
            PurchaseStatus.NotForSale => "판매 중지",
            _ => string.Empty
        };
    }

    /// <summary> KDH : 구매에 필요한 이전 단계 장비의 이름을 반환한다. </summary>
    private string GetRequiredEquipmentText()
    {
        if (_item == null || _item.Type != ItemType.Equipment || _item.Tier <= 1)
        {
            return string.Empty;
        }

        Item requiredItem = Managers.Table.Item.GetEquipmentItem(_item.EquipType, _item.Tier - 1);

        if (requiredItem == null)
        {
            return "이전 장비가 필요합니다.";
        }

        return $"{requiredItem.Name}을(를) 소지하고 있어야 합니다.";
    }

    #region ===== 이벤트 =====

    /// <summary> KDH : 현재 상품의 구매 가능 여부를 확인하고 구매 확인창을 표시한다. </summary>
    private void OnClickPurchase()
    {
        if (_item == null)
        {
            return;
        }

        Managers.UI.OpenPopup<AlertPopup>().Set("구매", "정말로 구매하시겠습니까?", true, TryPurchaseItem);
    }

    #endregion ===== 이벤트 =====
}