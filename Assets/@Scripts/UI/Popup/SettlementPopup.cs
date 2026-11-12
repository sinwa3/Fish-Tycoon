using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary> KDH : 영업이 끝났을 때 소득 및 만족도 정산 팝업 </summary>
public class SettlementPopup : BasePopup
{
    public enum Buttons
    {
        ExitBtn,
    }

    public enum Texts
    {
        TitleText,

        StartingMoneyValueText,
        FoodSalesValueText,
        ReputationBonusText,
        ReputationBonusValueText,
        UnsoldFishSalesValueText,
        TotalMoneyValueText,
    }

    #region ===== 참조 =====

    private DailyMenu _dailyMenu;
    private FishListScroll _fishListScroll;

    #endregion ===== 참조 =====

    #region ===== 설정 =====

    [SerializeField] private float _totalMoneyAnimationDuration = 1.5f;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    // 음식 판매금
    private int _calculatedFoodSales;

    // 평판 보너스 적용 소득
    private float _bonusMultiplier;
    private int _calculatedReputationBonus;

    // 남은 생선 자동 매각 소득
    private int _calculatedUnsoldFishSales;

    private bool _isFinalDay;
    private int _targetGold;

    // 애니메이션
    private int _startingMoney;
    private Tween _totalMoneyTween;

    private Action _onClosed;

    #endregion ===== 상태 =====

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
        Get<Button>(Buttons.ExitBtn).onClick.AddListener(OnClickExit);

        // 마지막 날에는 HUD를 표시하지 않는다.
        if (_isFinalDay)
        {
            Managers.UI?.CurrentHUD.SetVisible(false);
            Get<TMP_Text>(Texts.TitleText)?.SetText($"마지막 정산");
        }
        else
        {
            Get<TMP_Text>(Texts.TitleText)?.SetText($"{Managers.Data?.GetCurrentDay()}일 차 정산");
        }
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Esc)
        {
            OnClickExit();
        }
    }

    protected override void DestroyOverride()
    {
    }

    /// <summary> KDH : 정산 팝업 호출할 때 반드시 Set을 호출하여 데이터를 설정한다. </summary>
    public void Set(DailyMenu dailyMenu, FishListScroll fishListScroll, bool isFinalDay, int targetGold,
        Action onClosed = null)
    {
        _dailyMenu = dailyMenu;
        _fishListScroll = fishListScroll;
        _isFinalDay = isFinalDay;
        _targetGold = targetGold;
        _onClosed = onClosed;

        // 정산 전 소지금 저장
        _startingMoney = Managers.Data.GetCurrentGold();

        SetStartingMoneyText();

        CalculateFoodSales();
        Managers.Data.AddGold(_calculatedFoodSales);
        SetFoodSalesText();

        CalculateReputationBonus();
        Managers.Data.AddGold(_calculatedReputationBonus);
        SetReputationBonusText();

        CalculateUnsoldFishSales();
        Managers.Data.AddGold(_calculatedUnsoldFishSales);
        Managers.Data.ClearFishInventory();
        SetUnsoldFishSalesText();

        // 최종 소지금 숫자 증가 애니메이션
        SetTotalMoneyText();

        if (!_isFinalDay)
        {
            Managers.Data.ChangeSavedGameMode(GameMode.Tycoon);
            Managers.Data.SaveData();
            Managers.UI.OpenToastMessage("저장되었습니다.");
        }
        else
        {
            Managers.UI.OpenToastMessage("마지막 날입니다.");
        }
    }

    /// <summary> KDH : 게임 클리어 조건에 달성했는지에 따라 종료 스크린을 달리한다. </summary>
    private bool IsGameOver()
    {
        if (Managers.Data.GetCurrentGold() < _targetGold)
        {
            return true;
        }

        return false;
    }

    #region ===== 계산 =====

    /// <summary> KDH : 음식 판매금을 계산한다. </summary>
    private void CalculateFoodSales()
    {
        if (_dailyMenu == null || Managers.Table.Food.Map == null)
        {
            return;
        }

        for (int i = 0; i < _dailyMenu.ShuffledCandidates.Count; i++)
        {
            foreach (var food in Managers.Table.Food.Map)
            {
                if (food.Key == _dailyMenu.ShuffledCandidates[i])
                {
                    _calculatedFoodSales += food.Value.Price;
                }
            }
        }
    }

    /// <summary> KDH : 평판 보너스 금액을 계산한다. </summary>
    private void CalculateReputationBonus()
    {
        _bonusMultiplier = Managers.Table.Reputation.GetSettlementBonusMultiplier(Managers.Data.GetSatisfaction());
        int bonusAmount = Mathf.RoundToInt(_calculatedFoodSales * _bonusMultiplier);

        _calculatedReputationBonus = Mathf.Max(0, bonusAmount - _calculatedFoodSales);
    }

    /// <summary> KDH : 남은 생선을 자동 매각하여 계산한다. </summary>
    private void CalculateUnsoldFishSales()
    {
        if (_fishListScroll == null || Managers.Table.Fish == null)
        {
            return;
        }

        foreach (var fish in _fishListScroll.GetRemainingFishCounts())
        {
            int fishID = fish.Key;
            int fishCount = fish.Value;

            _calculatedUnsoldFishSales += Managers.Table.Fish.GetSellPrice(fishID) * fishCount;
        }
    }

    #endregion ===== 계산 =====

    #region ===== UI =====

    /// <summary> KDH : 기본 소지금을 UI에 적용한다. </summary>
    private void SetStartingMoneyText()
    {
        Get<TMP_Text>(Texts.StartingMoneyValueText).SetText(Managers.Data.GetCurrentGold().ToString("N0") + " G");
    }

    /// <summary> KDH : 음식 판매금을 UI에 적용한다. </summary>
    private void SetFoodSalesText()
    {
        Get<TMP_Text>(Texts.FoodSalesValueText).SetText($"+ {_calculatedFoodSales:N0}" + " G");
    }

    /// <summary> KDH : 평판 보너스를 UI에 적용한다. </summary>
    private void SetReputationBonusText()
    {
        float multiplier = Mathf.Max(0, _bonusMultiplier - 1);
        Get<TMP_Text>(Texts.ReputationBonusText).SetText($"평판 보너스\n<size=20>(음식 판매금 x {multiplier})</size>");
        Get<TMP_Text>(Texts.ReputationBonusValueText).SetText($"+ {_calculatedReputationBonus:N0} G");
    }

    /// <summary> KDH : 남은 생선 자동 매각 금액을 UI에 적용한다. </summary>
    private void SetUnsoldFishSalesText()
    {
        Get<TMP_Text>(Texts.UnsoldFishSalesValueText).SetText($"+ {_calculatedUnsoldFishSales:N0} G");
    }

    /// <summary> KDH : 정산 전 소지금부터 최종 소지금까지 숫자가 증가하는 애니메이션을 재생한다. </summary>
    private void SetTotalMoneyText()
    {
        TMP_Text totalMoneyText = Get<TMP_Text>(Texts.TotalMoneyValueText);
        if (totalMoneyText == null)
        {
            return;
        }

        int totalMoney = Managers.Data.GetCurrentGold();

        if (_isFinalDay)
        {
            totalMoneyText.color = totalMoney < TycoonScene.TargetGold ? Color.red : ColorKey.DodgerBlue;
        }

        _totalMoneyTween?.Kill();

        _totalMoneyTween = DOVirtual.Int(_startingMoney, totalMoney, _totalMoneyAnimationDuration,
                value => { totalMoneyText.SetText($"{value:N0} G"); })
            .SetEase(Ease.OutCubic).OnComplete(() =>
            {
                totalMoneyText.SetText($"{totalMoney:N0} G");
                _totalMoneyTween = null;
            });
    }

    #endregion ===== UI =====

    #region ===== 이벤트 =====

    /// <summary> KDH : Exit 버튼을 선택했을 때 팝업을 닫고 지정된 동작을 실행한다. </summary>
    private void OnClickExit()
    {
        Action onClosed = _onClosed;
        _onClosed = null;

        Close(() =>
        {
            if (_isFinalDay)
            {
                Managers.UI?.OpenLoadingUI(2f, () =>
                {
                    Managers.UI.CloseLoadingUI();
                    Managers.UI.OpenScreen<EndScreen>().ShowResult(IsGameOver());
                });
            }
            else
            {
                onClosed?.Invoke();
            }
        });
        Managers.Sound.PlaySfx(SoundKey.Name.SfxType.ChimeConfirm);
    }

    #endregion ===== 이벤트 =====
}