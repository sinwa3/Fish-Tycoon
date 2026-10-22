using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 음식 정보를 표시하는 툴팁을 관리한다. </summary>
public class FoodTooltip : BaseTooltip
{
    public enum Texts
    {
        FoodNameText,
        PriceValueText,
        SatisfactionValueText,
    }

    public enum Images
    {
        Required,
        FishImage,
    }

    #region ===== 상태 =====

    private FoodData _foodData;

    #endregion ===== 상태 =====

    protected override void OnAwake()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }

    /// <summary> KDH : 표시할 음식 데이터를 설정한다. </summary>
    public void Show(FoodData data, RectTransform trigger)
    {
        if (data == null)
        {
            return;
        }

        _foodData = data;

        base.Show(trigger);
    }

    /// <summary> KDH : 음식 정보를 UI에 적용한다. </summary>
    protected override void UpdateContent()
    {
        if (_foodData == null)
        {
            return;
        }

        // 기본 음식인 경우에는 생선 이미지를 표시하지 않는다.
        Get<Image>(Images.Required).gameObject.SetActive(_foodData.FishID != -1);
        Get<Image>(Images.FishImage).sprite = Managers.Table.Fish.GetSprite(_foodData.FishID);
        
        Get<TMP_Text>(Texts.FoodNameText).text = _foodData.Name;
        Get<TMP_Text>(Texts.PriceValueText).text = _foodData.Price.ToString("N0");
        Get<TMP_Text>(Texts.SatisfactionValueText).text = _foodData.Satisfaction.ToString();
    }
}