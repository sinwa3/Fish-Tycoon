using System;
using UnityEngine;

public class TycoonScene : BaseScene
{
    /// <summary> KDH : 순서 중요 </summary>
    public enum Phase
    {
        BeforeBusiness, // 영업 전
        DuringBusiness, // 영업 중
        AfterBusiness, // 영업 후
    }

    #region ===== 상태 =====

    private Phase _currentPhase;

    public readonly int TargetGold = 100000;
    public readonly int FinalDay = 7;

    private int _maxCustomerCount;

    #endregion ===== 상태 =====

    #region ===== 참조 =====

    private TycoonHUD _tycoonHUD;

    [SerializeField]
    private TycoonBgCtrl _tycoonBgCtrl;
    
    [Header("영업 전")]
    [SerializeField] private FoodListScroll _foodListScroll;

    #endregion ===== 참조 =====

    protected override void OnAwake()
    {
        _tycoonHUD = gameObject.GetComponentInChildren<TycoonHUD>(true);
        if (_tycoonHUD == null)
        {
            CPrint.Error("[TycoonScene] _tycoonHUD not found.");
        }
    }

    protected override void OnStart()
    {
        InitializePhase();
        
        SetMaxSelectableFoodCount();
    }

    protected override void OnUpdate()
    {
    }

    #region ===== 초기화 =====
    
    /// <summary> KDH : 저장되어 있는 게임모드를 기준으로 현재 페이즈를 초기화하고 해당 정보를 HUD에 업데이트한다. </summary>
    private void InitializePhase()
    {
        GameMode gameMode = Managers.Data.GetSavedGameMode();

        _currentPhase = gameMode switch
        {
            GameMode.Tycoon => Phase.AfterBusiness,
            GameMode.Stealth => Phase.BeforeBusiness,
            _ => throw new ArgumentOutOfRangeException()
        };

        UpdatePhaseUI();
    }

    /// <summary> KDH : 영업 시 최대 선택 가능 요리 수량을 설정한다. </summary>
    private void SetMaxSelectableFoodCount()
    {
        if (_foodListScroll == null)
        {
            CPrint.Error("[TycoonScene] _foodListScroll is null.");
            return;
        }

        _maxCustomerCount = Managers.Table.Reputation.GetCustomerCount(Managers.Data.GetSatisfaction());
        
        _foodListScroll.SetMaxSelectableFoodCount(_maxCustomerCount);
    }
    
    #endregion ===== 초기화 =====
    
    #region ===== 업데이트 =====
    
    /// <summary> KDH : 페이즈를 다음 단계로 진행시킨다. </summary>
    public void ChangeCurrentPhase()
    {
        switch (_currentPhase)
        {
            case Phase.DuringBusiness: // 영업 중 => 영업 후
                if (CheckFinalDay())
                {
                    Managers.UI.OpenScreen<EndScreen>().ShowResult(CheckGameClear());
                    return;
                }

                Managers.Data.ChangeSavedGameMode(GameMode.Tycoon);
                Managers.Data.SaveData();
                Managers.UI.OpenToastMessage("저장되었습니다.");
                break;
            case Phase.AfterBusiness: // 잠입 씬 전환
                Managers.Scene.LoadSceneWithLoading(SceneType.StealthScene);
                break;
        }

        if (_currentPhase < Phase.AfterBusiness)
        {
            _currentPhase++;
        }

        UpdatePhaseUI();
    }
    
    /// <summary> KDH : 현재 페이즈에 맞춰 UI를 업데이트한다. </summary>
    private void UpdatePhaseUI()
    {
        _tycoonHUD.UpdatePhaseText(_currentPhase);

        switch (_currentPhase)
        {
            case Phase.BeforeBusiness:
                _tycoonHUD.OpenFoodDiary();
                _tycoonBgCtrl.ShowBeforeOpening();
                break;
            case Phase.DuringBusiness:
                _tycoonBgCtrl.ShowOpened();
                break;
            case Phase.AfterBusiness:
                _tycoonHUD.DeactivateFoodDiary();
                _tycoonBgCtrl.ShowAfter();
                break;
            
        }
    }

    #endregion ===== 업데이트 =====
    
    #region ===== 체크 =====
    
    /// <summary> KDH : 마지막 날인지 체크한다.  </summary>
    private bool CheckFinalDay()
    {
        if (Managers.Data.GetCurrentDay() >= FinalDay)
        {
            return true;
        }

        return false;
    }

    /// <summary> KDH : 게임 클리어 조건에 달성했는지에 따라 종료 스크린을 달리하여 오픈한다. </summary>
    private bool CheckGameClear()
    {
        if (Managers.Data.GetCurrentGold() >= TargetGold)
        {
            return true;
        }

        return false;
    }
    
    #endregion ===== 체크 =====
}