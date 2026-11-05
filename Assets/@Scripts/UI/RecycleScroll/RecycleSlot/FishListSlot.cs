using TMPro;
using UnityEngine.UI;

/// <summary> KDH : 생선 목록의 슬롯을 관리한다. </summary>
public class FishListSlot : BaseRecycleSlot<FishData>
{
    public enum Images
    {
        FishImage,
        TooltipTrigger
    }

    public enum Texts
    {
        FishCountText
    }

    #region ===== 참조 =====

    private FishTooltip _fishTooltip;
    private FishListScroll _fishListScroll;

    #endregion ===== 참조 =====

    protected override void OnAwake()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }

    #region ===== 참조 =====

    /// <summary> KDH : 툴팁을 연결한다. </summary>
    public void SetTooltip(FishTooltip tooltip)
    {
        _fishTooltip = tooltip;
    }

    /// <summary> KDH : 생선 목록 스크롤을 연결한다. </summary>
    public void SetFishListScroll(FishListScroll fishListScroll)
    {
        _fishListScroll = fishListScroll;
    }

    #endregion ===== 참조 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 생선 데이터에 맞춰 슬롯 UI를 갱신한다. </summary>
    public override void UpdateData(FishData data)
    {
        base.UpdateData(data);

        // 생선 이미지를 표시한다.
        Get<Image>(Images.FishImage).sprite = Managers.Table.Fish.GetSprite(data.ID);

        // 요리에 사용할 수 있는 생선 수량을 표시한다.
        RefreshCount();

        // 툴팁 표시 이벤트를 연결한다.
        SubscribeDesc();
    }

    /// <summary> KDH : 요리에 사용할 수 있는 생선 수량을 표시한다. </summary>
    public void RefreshCount()
    {
        if (Data == null)
        {
            return;
        }

        // 요리에 사용할 수 있는 생선 수량을 가져온다.
        int fishCount = _fishListScroll?.GetAvailableFishCount(Data.ID) ?? 0;

        // 생선 수량을 표시한다.
        Get<TMP_Text>(Texts.FishCountText).text = fishCount.ToString("N0");
    }

    #endregion ===== 업데이트 =====

    #region ===== 이벤트 =====

    /// <summary> KDH : 툴팁 표시 이벤트를 연결한다. </summary>
    private void SubscribeDesc()
    {
        var trigger = Get<Image>(Images.TooltipTrigger).gameObject;

        trigger.ClearEvent();

        trigger.BindEvent(UIEventType.PointerEnter, OnPointerEnterDescription);
        trigger.BindEvent(UIEventType.PointerExit, OnPointerExitDescription);
    }

    /// <summary> KDH : 마우스가 올라오면 생선 정보를 표시한다. </summary>
    private void OnPointerEnterDescription()
    {
        _fishTooltip?.Show(Data, Get<Image>(Images.TooltipTrigger).rectTransform);
    }

    /// <summary> KDH : 마우스가 벗어나면 생선 정보를 숨긴다. </summary>
    private void OnPointerExitDescription()
    {
        _fishTooltip?.Hide();
    }

    #endregion ===== 이벤트 =====
}