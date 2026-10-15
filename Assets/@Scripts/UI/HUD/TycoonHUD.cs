using TMPro;
using UnityEngine.UI;

public class TycoonHUD : BaseHUD
{
    public enum Texts
    {
        GoldValueText,
        ReputationValueText,
        SatisfactionValueText,
        CurrentDayText,
        PhaseText
    }

    public enum Sliders
    {
        SatisfactionSlider
    }

    #region ===== 참조 =====

    private TycoonScene _tycoonScene;

    #endregion ===== 참조 =====

    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));

        _tycoonScene = gameObject.FindParent<TycoonScene>();
        if (_tycoonScene == null)
        {
            CPrint.Error("[TycoonHUD] : no _tycoonScene found.");
        }
    }

    protected override void OnStart()
    {
        SetCurrentDayText();

        UpdateGoldText();
        UpdateSatisfactionInfo();

        SubscribeGoldChanged();
        SubscribeSatisfactionChanged();
        SubscribePhaseChanged();
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

        if (Managers.Input.KeyDown_3)
        {
            Managers.Data.AddGold(10);
        }

        if (Managers.Input.KeyDown_4)
        {
            Managers.Data.TrySpendGold(10);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeGoldChanged();
        UnsubscribeSatisfactionChanged();
        UnsubscribePhaseChanged();
    }

    #region ===== 초기화 =====

    /// <summary> 타이쿤 씬 첫 진입시 현재 영업 날짜를 세팅한다. </summary>
    private void SetCurrentDayText()
    {
        Get<TMP_Text>(Texts.CurrentDayText).SetText($"{Managers.Data.GetCurrentDay()}일 차");
    }

    #endregion ===== 초기화 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 현재 골드 값을 UI에 업데이트 한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldValueText)?.SetText(Managers.Data.GetCurrentGold().ToString("N0"));
    }

    /// <summary> KDH : 현재 영업 상태를 UI에 업데이트 한다. </summary>
    private void UpdatePhaseText(TycoonScene.Phase phase)
    {
        string message = phase switch
        {
            TycoonScene.Phase.BeforeBusiness => "영업 전",
            TycoonScene.Phase.DuringBusiness => "영업 중",
            TycoonScene.Phase.AfterBusiness => "영업 후",
            _ => string.Empty
        };

        Get<TMP_Text>(Texts.PhaseText)?.SetText(message);
    }

    /// <summary> KDH : 현재 만족도와 평판 레벨을 업데이트한다. </summary>
    private void UpdateSatisfactionInfo()
    {
        if (Managers.Table.Reputation == null)
        {
            CPrint.Error("[TycoonHUD] : 현재 테이블 데이터가 존재하지 않습니다.");
            return;
        }

        var info = Managers.Data.GetReputationInfo();

        Get<TMP_Text>(Texts.ReputationValueText)?.SetText(info.reputationGrade.ToString("N0"));
        Get<TMP_Text>(Texts.SatisfactionValueText)
            ?.SetText($"{info.currentSatisfaction:N0} / {info.requiredSatisfaction:N0}");

        bool isMaxGrade = Managers.Table.Reputation.IsMaxReputationGrade(info.reputationGrade);

        if (isMaxGrade) // 만렙인경우
        {
            Get<Slider>(Sliders.SatisfactionSlider)?.SetValueWithoutNotify(1f);
            Get<TMP_Text>(Texts.SatisfactionValueText)?.SetText("MAX");

            return;
        }

        float progress = info.requiredSatisfaction > 0
            ? (float)info.currentSatisfaction / info.requiredSatisfaction
            : 0f;
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

    /// <summary> 페이즈 변경되었을 때 영업 상태 텍스트를 변경을 구독 </summary>
    private void SubscribePhaseChanged()
    {
        if (_tycoonScene != null)
        {
            _tycoonScene.OnPhaseChanged += UpdatePhaseText;
        }
    }

    /// <summary> 페이즈 변경되었을 때 영업 상태 텍스트를 변경을 구독해제 </summary>
    private void UnsubscribePhaseChanged()
    {
        if (_tycoonScene != null)
        {
            _tycoonScene.OnPhaseChanged -= UpdatePhaseText;
        }
    }

    #endregion ===== 이벤트 =====
}