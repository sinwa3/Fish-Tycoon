using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 생선 정보를 표시하는 툴팁을 관리한다. </summary>
public class FishTooltip : BaseTooltip
{
    public enum Texts
    {
        FishNameText,
        PriceValueText,
    }

    public enum Images
    {
        FoodImage,
    }

    #region ===== 상태 =====

    private FishData _fishData;

    #endregion ===== 상태 =====

    protected override void OnAwake()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }

    /// <summary> KDH : 표시할 생선 데이터를 설정한다. </summary>
    public void Show(FishData data, RectTransform trigger)
    {
        if (data == null)
        {
            return;
        }

        _fishData = data;

        base.Show(trigger);
    }

    /// <summary> KDH : 생선 정보를 UI에 적용한다. </summary>
    protected override void UpdateContent()
    {
        if (_fishData == null)
        {
            return;
        }

        Get<Image>(Images.FoodImage).sprite = Managers.Table.Food.GetSpriteByFishID(_fishData.ID);

        Get<TMP_Text>(Texts.FishNameText).text = _fishData.Name;
        Get<TMP_Text>(Texts.PriceValueText).text = _fishData.SellPrice.ToString("N0");
    }
}