using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TycoonHUD : BaseHUD
{
    public enum Texts
    {
        GoldValueText,
        ReputationValueText,
        SatisfactionValueText
    }

    public enum Sliders
    {
        SatisfactionSlider
    }

    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
    }

    protected override void OnStart()
    {
        UpdateGoldText();
        UpdateSatisfactionInfo();

        SubscribeGoldChanged();
        SubscribeSatisfactionChanged();
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        base.OnInputKey();

        if (Managers.Input.KeyDown_1)
        {
            Managers.Data.AddSatisfaction(10);
        }
        if (Managers.Input.KeyDown_2)
        {
            Managers.Data.SubtractSatisfaction(10);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeGoldChanged();
        UnsubscribeSatisfactionChanged();
    }

    #region ===== 업데이트 =====

    /// <summary> KDH : 현재 골드 값을 UI에 업데이트 한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldValueText)?.SetText(Managers.Data.GetCurrentGold().ToString("N0"));
    }

    /// <summary> KDH : 현재 만족도와 평판 레벨을 업데이트한다. </summary>
    private void UpdateSatisfactionInfo()
    {
        var info = Managers.Data.GetReputationInfo();

        Get<TMP_Text>(Texts.ReputationValueText)?.SetText(info.reputationGrade.ToString("N0"));
        Get<TMP_Text>(Texts.SatisfactionValueText)?.SetText($"{info.currentSatisfaction:N0} / {info.requiredSatisfaction:N0}");

        bool isMaxGrade = Managers.Table.Reputation.IsMaxReputationGrade(info.reputationGrade);

        if (isMaxGrade) // 만렙인경우
        {
            Get<Slider>(Sliders.SatisfactionSlider)?.SetValueWithoutNotify(1f);
            Get<TMP_Text>(Texts.SatisfactionValueText)?.SetText("MAX");

            return;
        }
        
        float progress = info.requiredSatisfaction > 0 ? (float)info.currentSatisfaction / info.requiredSatisfaction : 0f;
        Get<Slider>(Sliders.SatisfactionSlider)?.SetValueWithoutNotify(progress);
    }

    #endregion ===== 업데이트 =====

    #region ===== 이벤트 =====

    /// <summary> KDH : 골드 변화시 UI 업데이트를 구독 </summary>
    private void SubscribeGoldChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnGoldChanged += UpdateGoldText;
        }
    }

    /// <summary> KDH : 골드 변화시 UI 업데이트를 구독해제 </summary>
    private void UnsubscribeGoldChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnGoldChanged -= UpdateGoldText;
        }
    }

    /// <summary> KDH : 만족도 변화시 UI 업데이트를 구독 </summary>
    private void SubscribeSatisfactionChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnSatisfactionChanged += UpdateSatisfactionInfo;
        }
    }

    /// <summary> KDH : 만족도 변화시 UI 업데이트를 구독해제 </summary>
    private void UnsubscribeSatisfactionChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnSatisfactionChanged -= UpdateSatisfactionInfo;
        }
    }
    
    #endregion ===== 이벤트 =====
}