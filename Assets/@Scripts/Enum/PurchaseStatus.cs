/// <summary> KDH : 아이템 구매 가능 상태 및 구매 처리 결과를 정의한다. </summary>
public enum PurchaseStatus
{
    Available,
    NotForSale,
    InsufficientGold,
    PreviousTierRequired,
    AlreadyPurchased,
    QuantityLimit,
    NoEmptySlot,
}