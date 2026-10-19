using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TycoonHUD : BaseHUD
{
    public enum Texts
    {
        GoldValueText,
        ReputationValueText,
        SatisfactionValueText,
        CurrentDayText,
        PhaseText,

        MainQuestValueText,
    }

    public enum Sliders
    {
        SatisfactionSlider
    }

    public enum Buttons
    {
        NextBtn,
        StartBusinessBtn,
    }

    public enum Images
    {
        GoldFrameImage,
    }
    
    #region ===== 참조 =====

    private TycoonScene _tycoonScene;

    private CanvasGroup _foodDiaryCG;

    private FoodListScroll _foodListScroll;

    #endregion ===== 참조 =====

    #region ===== 애니메이션 =====

    private RectTransform _diaryRectTransform;
    private Vector2 _diaryOriginPos;
    private Tween _diaryTween;

    #endregion ===== 애니메이션 =====

    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        _tycoonScene = gameObject.FindParent<TycoonScene>();

        ResolveFoodDiary();
    }

    protected override void OnStart()
    {
        InitializeBtnEvent();

        SetCurrentDayText();
        SetMainQuestValueText();

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
            Managers.Data.AddSatisfaction(100);
        }

        if (Managers.Input.KeyDown_2)
        {
            Managers.Data.SubtractSatisfaction(100);
        }

        if (Managers.Input.KeyDown_3)
        {
            Managers.Data.AddGold(1000);
        }

        if (Managers.Input.KeyDown_4)
        {
            Managers.Data.TrySpendGold(1000);
        }
    }

    private void OnDestroy()
    {
        _diaryTween?.Kill();

        UnsubscribeGoldChanged();
        UnsubscribeSatisfactionChanged();
    }

    #region ===== 초기화 =====

    /// <summary> KDH : 버튼 이벤트를 초기화한다. </summary>
    private void InitializeBtnEvent()
    {
        Get<Button>(Buttons.NextBtn).onClick.AddListener(() => _tycoonScene.ChangeCurrentPhase());
        Get<Button>(Buttons.StartBusinessBtn).onClick.AddListener(CloseFoodDiary);
    }

    /// <summary> KDH : 타이쿤 씬 첫 진입시 현재 영업 날짜를 세팅한다. </summary>
    private void SetCurrentDayText()
    {
        Get<TMP_Text>(Texts.CurrentDayText).SetText($"{Managers.Data.GetCurrentDay()}일 차");
    }

    /// <summary> KDH : 타이쿤 씬 첫 진입시 목표 금액을 설정한다. </summary>
    private void SetMainQuestValueText()
    {
        Get<TMP_Text>(Texts.MainQuestValueText).SetText($"{_tycoonScene.TargetGold:N0}");
    }

    /// <summary> KDH : 음식 메뉴 리스트를 참조한다. </summary>
    private void ResolveFoodDiary()
    {
        _foodDiaryCG = gameObject.FindChild<CanvasGroup>("FoodDiaryCG", true);
        if (_foodDiaryCG != null)
        {
            _diaryRectTransform = _foodDiaryCG.GetComponent<RectTransform>();
            _diaryOriginPos = _diaryRectTransform.anchoredPosition;
        }

        _foodListScroll = gameObject.FindChild<FoodListScroll>();
    }

    #endregion ===== 초기화 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 현재 골드 값을 UI에 업데이트 한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldValueText)?.SetText(Managers.Data.GetCurrentGold().ToString("N0"));
    }

    /// <summary> KDH : 현재 영업 상태를 UI에 업데이트 한다. </summary>
    public void UpdatePhaseText(TycoonScene.Phase phase)
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

    #region ===== 활성화 =====

    /// <summary> KDH : 음식 다이어리를 오픈한다. </summary>
    public void OpenFoodDiary()
    {
        if (_foodDiaryCG == null || _diaryRectTransform == null)
        {
            return;
        }

        _diaryTween?.Kill();

        _diaryRectTransform.anchoredPosition = _diaryOriginPos + Vector2.down * 100f;

        _foodDiaryCG.alpha = 0f;
        _foodDiaryCG.interactable = false;
        _foodDiaryCG.blocksRaycasts = true;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(_diaryRectTransform.DOAnchorPos(_diaryOriginPos, 0.45f).SetEase(Ease.OutCubic));
        sequence.Join(_foodDiaryCG.DOFade(1f, 0.45f));
        sequence.OnComplete(() => { _foodDiaryCG.interactable = true; });

        _diaryTween = sequence;
    }

    /// <summary> KDH : 음식 다이어리를 닫고 영업 중으로 전환한다. </summary>
    private void CloseFoodDiary()
    {
        if (_foodDiaryCG == null || _diaryRectTransform == null)
        {
            return;
        }

        _diaryTween?.Kill();

        _foodDiaryCG.interactable = false;
        _foodDiaryCG.blocksRaycasts = true;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(_diaryRectTransform.DOAnchorPos(_diaryOriginPos + Vector2.down * 100f, 0.45f)
            .SetEase(Ease.InCubic));
        sequence.Join(_foodDiaryCG.DOFade(0f, 0.45f));
        sequence.OnComplete(() =>
        {
            _foodDiaryCG.blocksRaycasts = false;
            _tycoonScene.ChangeCurrentPhase();
            DeactivateFoodDiary();
        });

        _diaryTween = sequence;
    }

    /// <summary> KDH : 가게 평판/만족도/소지금을 표시/비표시한다. </summary>
    public void SetStoreInfoVisible(bool isVisible)
    {
        Get<Slider>(Sliders.SatisfactionSlider)?.gameObject.SetActive(isVisible);
        Get<Image>(Images.GoldFrameImage)?.gameObject.SetActive(isVisible);
    }
    
    /// <summary> KDH : 다이어리를 비활성화한다. </summary>
    public void DeactivateFoodDiary()
    {
        if (_foodDiaryCG == null)
        {
            return;
        }

        _diaryTween?.Kill();
        _foodDiaryCG.transform.parent.gameObject.SetActive(false);
    }

    #endregion ===== 활성화 =====

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