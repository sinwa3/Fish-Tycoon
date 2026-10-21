using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 영업이 끝났을 때 소득 및 만족도 정산 팝업 </summary>
public class SettlementPopup : BasePopup
{
    public enum Buttons
    {
        ExitBtn,
    }

    public enum Texts
    {
        StartingMoneyValueText,
        FoodSalesValueText,
        ReputationBonusText,
        ReputationBonusValueText,
        UnsoldFishSalesValueText,
        TotalMoneyValueText,
    }

    private DailyMenu _dailyMenu;
    private FishListScroll _fishListScroll;

    // 음식 판매금
    private int _calculatedFoodSales;

    // 평판 보너스 적용 소득
    private float _bonusMultiplier;
    private int _calculatedReputationBonus;

    // 남은 생선 자동 매각 소득
    private int _calculatedUnsoldFishSales;
    
    private bool _isFinalDay;
    private int _targetGold;

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
        Get<Button>(Buttons.ExitBtn).onClick.AddListener(OnClickExit);
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
    public void Set(DailyMenu dailyMenu, FishListScroll fishListScroll, bool isFinalDay, int targetGold)
    {
        _dailyMenu = dailyMenu;
        _fishListScroll = fishListScroll;
        _isFinalDay = isFinalDay;
        _targetGold = targetGold;
        
        CalculateFoodSales();
        Managers.Data.AddGold(_calculatedFoodSales);

        CalculateReputationBonus();
        Managers.Data.AddGold(_calculatedReputationBonus);

        CalculateUnsoldFishSales();
        Managers.Data.ClearFishInventory();

        SetStartingMoneyText();
        SetFoodSalesText();
        SetReputationBonusText();
        SetUnsoldFishSalesText();
        SetTotalMoneyText();

        Managers.Data.AddGold(_calculatedFoodSales + _calculatedReputationBonus + _calculatedUnsoldFishSales);

        if (!_isFinalDay)
        {
            Managers.Data.ChangeSavedGameMode(GameMode.Tycoon);
            Managers.Data.SaveData();
            Managers.UI.OpenToastMessage("저장되었습니다.");
        }
        else
        {
            Managers.UI.OpenToastMessage("마지막 날입니다. 다음으로 진행하시면 결과를 확인할 수 있습니다.");
        }
    }

    /// <summary> KDH : 게임 클리어 조건에 달성했는지에 따라 종료 스크린을 달리한다. </summary>
    private bool IsGameClear()
    {
        if (Managers.Data.GetCurrentGold() >= _targetGold)
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
        Get<TMP_Text>(Texts.StartingMoneyValueText).SetText(Managers.Data.GetCurrentGold().ToString("N0"));
    }

    /// <summary> KDH : 음식 판매금을 UI에 적용한다. </summary>
    private void SetFoodSalesText()
    {
        Get<TMP_Text>(Texts.FoodSalesValueText).SetText($"+ {_calculatedFoodSales:N0}");
    }

    /// <summary> KDH : 평판 보너스를 UI에 적용한다. </summary>
    private void SetReputationBonusText()
    {
        float multiplier = Mathf.Max(0, _bonusMultiplier - 1);
        Get<TMP_Text>(Texts.ReputationBonusText).SetText($"평판 보너스\n<size=20>(음식 판매금 x {multiplier})</size>");
        Get<TMP_Text>(Texts.ReputationBonusValueText).SetText($"+ {_calculatedReputationBonus:N0}");
    }

    /// <summary> KDH : 남은 생선 자동 매각 금액을 UI에 적용한다. </summary>
    private void SetUnsoldFishSalesText()
    {
        Get<TMP_Text>(Texts.UnsoldFishSalesValueText).SetText($"+ {_calculatedUnsoldFishSales:N0}");
    }

    /// <summary> KDH : 소득 적용 후 현재 소지금을 UI에 적용한다. </summary>
    private void SetTotalMoneyText()
    {
        int totalMoney = Managers.Data.GetCurrentGold() + _calculatedFoodSales + _calculatedReputationBonus +
                         _calculatedUnsoldFishSales;

        Get<TMP_Text>(Texts.TotalMoneyValueText).SetText(totalMoney.ToString("N0"));
    }

    #endregion ===== UI =====

    #region ===== 이벤트 =====

    /// <summary> KDH : Exit 버튼을 선택했을 때 팝업을 닫고 지정된 동작을 실행한다. </summary>
    private void OnClickExit()
    {
        Close(() =>
        {
            if (_isFinalDay)
            {
                Managers.UI.OpenScreen<EndScreen>().ShowResult(IsGameClear());
            }
        });
        Managers.Sound.PlaySfx(SoundKey.Name.SfxType.ChimeConfirm);
    }

    #endregion ===== 이벤트 =====
}