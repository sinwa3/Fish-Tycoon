using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 요리 목록의 생선 슬롯을 관리한다. </summary>
public class FoodListSlot : BaseRecycleSlot<FoodData>
{
    public enum Images
    {
        FoodImage,
        FoodCountFrame,
        TooltipTrigger,
        Dim,
    }

    public enum Buttons
    {
        FoodCountUpBtn,
        FoodCountDownBtn,
    }

    public enum Texts
    {
        FoodCountText
    }

    #region ===== 참조 =====

    private FoodTooltip _foodTooltip;
    private FoodListScroll _foodListScroll;

    #endregion ===== 참조 =====

    protected override void OnAwake()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        Get<Button>(Buttons.FoodCountUpBtn).onClick.AddListener(OnClickAddFood);
        Get<Button>(Buttons.FoodCountDownBtn).onClick.AddListener(OnClickRemoveFood);

        Get<TMP_Text>(Texts.FoodCountText).text = "0";
    }

    /// <summary> KDH : 툴팁을 연결한다. </summary>
    public void SetTooltip(FoodTooltip tooltip)
    {
        _foodTooltip = tooltip;
    }

    /// <summary> KDH : 요리 목록 스크롤을 연결한다. </summary>
    public void SetFoodListScroll(FoodListScroll scroll)
    {
        _foodListScroll = scroll;
    }

    #region ===== 업데이트 =====

    /// <summary> KDH : 요리 데이터에 맞춰 슬롯 UI를 갱신한다. </summary>
    public override void UpdateData(FoodData data)
    {
        base.UpdateData(data);

        // 요리 이미지를 표시한다.
        Get<Image>(Images.FoodImage).sprite = Managers.Table.Food.GetSpriteByFishID(data.FishID);

        // 생선 보유 여부에 따라 UI를 갱신한다.
        bool hasFish = Managers.Data.GetFishCount(data.FishID) > 0;

        // 기본 음식이거나 생선을 가지고 있는 경우에는 딤을 표시하지 않는다.
        Get<Image>(Images.Dim).gameObject.SetActive(!(data.FishID == -1 || hasFish));
        // 기본 음식이거나 생선을 가지고 있는 경우에는 수량 표시
        Get<Image>(Images.FoodCountFrame).gameObject.SetActive(data.FishID == -1 || hasFish);
        Get<Button>(Buttons.FoodCountUpBtn).gameObject.SetActive(hasFish);
        Get<Button>(Buttons.FoodCountDownBtn).gameObject.SetActive(hasFish);

        SubscribeDesc();

        // 선택 수량과 버튼 상태를 갱신한다.
        RefreshCount();
    }

    /// <summary> KDH : 현재 선택한 요리 수량과 버튼 상태를 갱신한다. </summary>
    public void RefreshCount()
    {
        if (Data == null || _foodListScroll == null)
        {
            return;
        }

        // 기본음식인 경우
        int selectableCount = _foodListScroll.MaxSelectableFoodCount - _foodListScroll.CurSelectedFoodCount;
        int selectedCount = Data.FishID == -1 ? Mathf.Max(0, selectableCount) : _foodListScroll.GetSelectedFoodCount(Data.ID);

        Get<TMP_Text>(Texts.FoodCountText).SetText(selectedCount.ToString("N0"));

        // 해당 생선을 추가로 사용할 수 있는지 확인한다.
        bool hasAvailableFish = _foodListScroll.GetAvailableFishCount(Data.FishID) > 0;

        Get<Button>(Buttons.FoodCountUpBtn).interactable = hasAvailableFish;
        Get<Button>(Buttons.FoodCountDownBtn).interactable = selectedCount > 0;
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

    /// <summary> KDH : 마우스가 올라오면 요리 정보를 표시한다. </summary>
    private void OnPointerEnterDescription()
    {
        _foodTooltip?.Show(Data, Get<Image>(Images.TooltipTrigger).rectTransform);
    }

    /// <summary> KDH : 마우스가 벗어나면 요리 정보를 숨긴다. </summary>
    private void OnPointerExitDescription()
    {
        _foodTooltip?.Hide();
    }

    /// <summary> KDH : 요리 수량 증가를 요청한다. </summary>
    private void OnClickAddFood()
    {
        _foodListScroll?.AddFood(Data);
    }

    /// <summary> KDH : 요리 수량 감소를 요청한다. </summary>
    private void OnClickRemoveFood()
    {
        _foodListScroll?.RemoveFood(Data);
    }

    #endregion ===== 이벤트 =====
}