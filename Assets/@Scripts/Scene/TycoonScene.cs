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
    private float _customerSpawnInterval;

    #endregion ===== 상태 =====

    #region ===== 참조 =====

    private TycoonHUD _tycoonHUD;

    [SerializeField]
    private TycoonBgCtrl _tycoonBgCtrl;

    [Header("영업 전")]
    [SerializeField] private FoodListScroll _foodListScroll;

    [Header("영업 중")]
    [SerializeField] private CustomerSpawner _customerSpawner;
    [SerializeField] private CookingController _cookingController;
    public CookingController CookingController => _cookingController;

    private DailyMenu _dailyMenu;

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

        if (_currentPhase == Phase.BeforeBusiness) // 영업 전이면
        {
            // 영업을 준비한다.
            ReadyBusiness();
        }
        
        UpdatePhaseUI();

        SubscribeBusinessEvents();
        SubscribeBusinessClosed();
    }

    protected override void OnUpdate()
    {
    }

    private void OnDestroy()
    {
        UnsubscribeBusinessEvents();
        UnsubscribeBusinessClosed();
    }

    #region ===== 초기화 =====

    /// <summary> KDH : 저장되어 있는 게임모드를 기준으로 현재 페이즈를 초기화한다. </summary>
    private void InitializePhase()
    {
        GameMode gameMode = Managers.Data.GetSavedGameMode();

        _currentPhase = gameMode switch
        {
            GameMode.Tycoon => Phase.AfterBusiness,
            GameMode.Stealth => Phase.BeforeBusiness,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary> KDH : 영업을 위한 데이터를 설정한다. </summary>
    private void ReadyBusiness()
    {
        if (_foodListScroll == null)
        {
            CPrint.Error("[TycoonScene] _foodListScroll is null.");
            return;
        }

        if (_customerSpawner == null)
        {
            CPrint.Error("[TycoonScene] _customerSpawner is null.");
            return;
        }

        // 손님 수 세팅        
        _maxCustomerCount = Managers.Table.Reputation.GetCustomerCount(Managers.Data.GetSatisfaction());
        _foodListScroll.SetMaxSelectableFoodCount(_maxCustomerCount);

        // 손님 입장 인터버 세팅
        _customerSpawnInterval = Managers.Table.Reputation.GetCustomerSpawnInterval(Managers.Data.GetSatisfaction());
    }

    #endregion ===== 초기화 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 페이즈를 다음 단계로 진행시킨다. </summary>
    public void ChangeCurrentPhase()
    {
        if (_foodListScroll == null || _cookingController == null || _customerSpawner == null)
        {
            CPrint.Warning("[TycoonScene] null");
            return;
        }

        switch (_currentPhase)
        {
            case Phase.BeforeBusiness: // 영업 전 => 영업 중
                // 만족도, 소지금 UI를 숨긴다.
                _tycoonHUD.SetStoreInfoVisible(false);
                
                // 영업 진행도를 초기화 한다.
                _tycoonHUD.InitializeBusinessProgress(_maxCustomerCount);

                // DailyMenu를 선택한 음식 리스트로 초기화한다.
                _dailyMenu = new DailyMenu();
                foreach (var variables in _foodListScroll.SelectedFoodCounts)
                {
                    _dailyMenu.AddMenu(variables.Key, variables.Value);
                }

                // 쿠킹 컨트롤러를 초기화한다.
                _cookingController.Initialize(_dailyMenu.ShuffledCandidates);

                // 손님 스포너에 오늘 요리할 음식리스트, 최대 손님 수, 손님 인터버를 전달한다.
                _customerSpawner.StartBusiness(_maxCustomerCount, _customerSpawnInterval, _dailyMenu);

                break;
            case Phase.DuringBusiness: // 영업 중 => 영업 후
                // 영업 중 받은 만족도를 적용시킨다.
                Managers.Data.AddSatisfaction(_customerSpawner.DailySatisfaction);
                
                // 만족도, 소지금 UI를 보인다.
                _tycoonHUD.SetStoreInfoVisible(true);

                // 쿠킹 컨트롤러를 종료한다.
                _cookingController.Clear();

                // 손님 스포너를 종료한다.
                _customerSpawner.StopBusiness();

                // 마지막 날인 경우에는 게임을 끝낸다.
                if (CheckFinalDay())
                {
                    Managers.UI.OpenScreen<EndScreen>().ShowResult(CheckGameClear());
                }
                else
                {
                    // 정산 팝업
                    Managers.UI.OpenPopup<SettlementPopup>().Set(_dailyMenu, _foodListScroll.FishListScroll);
                }

                break;
            case Phase.AfterBusiness: // 영업 후 => 잠입 씬 전환
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
        if (_tycoonHUD == null)
        {
            return;
        }
        
        _tycoonHUD.UpdatePhaseText(_currentPhase);

        switch (_currentPhase)
        {
            case Phase.BeforeBusiness:
                _tycoonHUD.OpenFoodDiary();
                _tycoonHUD.SetNextBtnVisible(false);
                _tycoonHUD.SetBusinessProgressVisible(false);
                
                _tycoonBgCtrl.ShowBeforeOpening();
                break;
            case Phase.DuringBusiness:
                _tycoonBgCtrl.ShowOpened();
                _tycoonHUD.SetNextBtnVisible(false);
                _tycoonHUD.SetBusinessProgressVisible(true);

                break;
            case Phase.AfterBusiness:
                _tycoonHUD.DeactivateFoodDiary();
                _tycoonHUD.SetNextBtnVisible(true);
                _tycoonHUD.SetBusinessProgressVisible(false);
                
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

    #region ===== 이벤트 =====

    /// <summary> KDH : 영업이 시작했을시 이벤트를 구독한다. </summary>
    private void SubscribeBusinessEvents()
    {
        if (_cookingController == null || _customerSpawner == null)
        {
            return;
        }

        _cookingController.OnServingFinished += HandleServingFinished;
        _customerSpawner.OnOrderCountChanged += HandleOrderCountChanged;
    }

    /// <summary> KDH : 영업이 시작했을시 이벤트를 구독 해제한다. </summary>
    private void UnsubscribeBusinessEvents()
    {
        if (_cookingController == null || _customerSpawner == null)
        {
            return;
        }

        _cookingController.OnServingFinished -= HandleServingFinished;
        _customerSpawner.OnOrderCountChanged -= HandleOrderCountChanged;
    }

    /// <summary> KDH : 영업이 종료됐을시 이벤트를 구독한다. </summary>
    private void SubscribeBusinessClosed()
    {
        if (_customerSpawner == null)
        {
            return;
        }

        _customerSpawner.OnBusinessClosed += ChangeCurrentPhase;
    }

    /// <summary> KDH : 영업이 종료됐을시 이벤트를 구독해제한다. </summary>
    private void UnsubscribeBusinessClosed()
    {
        if (_customerSpawner == null)
        {
            return;
        }

        _customerSpawner.OnBusinessClosed -= ChangeCurrentPhase;
    }

    /// <summary> KDH : 주문 수가 변경되면 조리 컨트롤러를 갱신한다. </summary>
    private void HandleOrderCountChanged(int orderCount)
    {
        if (_cookingController == null)
        {
            return;
        }

        _cookingController.UpdateOrderedCount(orderCount);
        _cookingController.StartCooking();
    }

    /// <summary> KDH : 음식 서빙이 끝나면 손님에게 음식을 전달한다. </summary>
    private void HandleServingFinished()
    {
        _customerSpawner?.DeliverFood();
    }

    #endregion ===== 이벤트 =====
}