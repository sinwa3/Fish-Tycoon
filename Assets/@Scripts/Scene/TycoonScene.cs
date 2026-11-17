using System;
using UnityEngine;
using UnityEngine.UI;

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

    public static readonly int TargetGold = 100000;
    public static readonly int FinalDay = 7;

    private int _maxCustomerCount;
    private float _customerSpawnInterval;

    public int ServedCount { get; set; }

    #endregion ===== 상태 =====

    #region ===== 참조 =====

    private TycoonHUD _tycoonHUD;
    private Camera _playCamera;
    private RectTransform _firstBar;
    private RectTransform _secondBar;
    private Vector2Int _viewportSize;
    
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
        InitializeLetterbox();
        InitializePhase();

        if (_currentPhase == Phase.BeforeBusiness) // 영업 전이면
        {
            // 영업을 준비한다.
            ReadyBusiness();
        }

        UpdatePhaseUI();

        SubscribeBusinessEvents();
        SubscribeBusinessClosed();
        
        if (_currentPhase == Phase.BeforeBusiness)
        {
            Managers.Tutorial.Show(TutorialTrigger.BeforeBusiness);
        }
    }

    protected override void OnUpdate()
    {
        UpdateViewport();
    }

    private void OnDestroy()
    {
        UnsubscribeBusinessEvents();
        UnsubscribeBusinessClosed();
    }

    #region ===== 초기화 =====

    /// <summary> KDH : 레터박스와 관련된 값들을 생성하거나 초기화한다. </summary>
    private void InitializeLetterbox()
    {
        _playCamera = Camera.main;
        
        if (_playCamera != null)
        {
            // 레터박스 전용 Canvas를 생성하고 현재 씬 오브젝트의 자식으로 설정
            GameObject letterboxObj = new GameObject("Tycoon Letterbox", typeof(Canvas));
            letterboxObj.transform.SetParent(transform, false);
            
            Canvas canvas = letterboxObj.GetComponent<Canvas>();
            
            // 카메라와 관계없이 화면에 직접 렌더링하고, 다른 UI보다 낮은 정렬 순서 지정
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -1;
            
            // 화면의 남는 영역을 가릴 검은색 바 2개 생성
            _firstBar = CreateBar(letterboxObj.transform, "First Bar");
            _secondBar = CreateBar(letterboxObj.transform, "Second Bar");
            
            // 현재 화면 비율에 맞게 카메라 영역과 레터박스 갱신
            UpdateViewport();
        }
    }
    
    /// <summary> KDH : 레터박스에 사용할 검은색 Image를 생성한다. </summary>
    private static RectTransform CreateBar(Transform parent, string objectName)
    {
        // RectTransform과 Image 컴포넌트를 가진 UI 오브젝트 생성
        Image image = new GameObject(objectName, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        
        // 부모 Canvas에 연결하고 로컬 Transform 유지
        image.transform.SetParent(parent, false);
        
        // 검은색 여백으로 표시하고 UI 입력을 가로채지 않도록 설정
        image.color = Color.black;
        image.raycastTarget = false;
        
        return image.rectTransform;
    }
    
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

    /// <summary> KDH : 창 중앙에 16:9 플레이 영역을 만들고 남는 공간을 검정으로 가린다. </summary>
    private void UpdateViewport()
    {
        // 현재 게임 창의 해상도 가져오기
        Vector2Int size = new(Screen.width, Screen.height);
        
        // 필요한 오브젝트가 없거나 해상도가 변경되지 않았다면 갱신하지 않음
        if (_playCamera == null || _firstBar == null || size.x <= 0 || size.y <= 0 || size == _viewportSize)
        {
            return;
        }

        _viewportSize = size;
        
        // 목표 화면 비율(16:9)과 현재 게임 창의 비율 계산
        const float referenceAspect = 16f / 9f;
        float aspect = (float)size.x / size.y;
        
        // 16:9 비율을 유지하면서 화면에 표시할 가로/세로 영역 계산 (0~1)
        float width = Mathf.Min(1f, referenceAspect / aspect);
        float height = Mathf.Min(1f, aspect / referenceAspect);
        // 남는 공간을 양쪽에 균등하게 배분하여 플레이 영역을 화면 중앙에 배치
        Rect viewport = new((1f - width) * 0.5f, (1f - height) * 0.5f, width, height);

        // 월드 카메라의 렌더링 영역과 화면 비율 설정
        _playCamera.rect = viewport;
        _playCamera.aspect = referenceAspect;
        
        // UI 카메라도 월드 카메라와 동일한 렌더링 영역을 사용하도록 동기화
        Managers.UI.SyncSceneUIViewport(_playCamera);

        if (width < 1f)
        {
            // 창이 16:9보다 넓으면 좌우에 검은색 바 배치 (필러박스)
            SetBar(_firstBar, Vector2.zero, new Vector2(viewport.xMin, 1f));
            SetBar(_secondBar, new Vector2(viewport.xMax, 0f), Vector2.one);
        }
        else
        {
            // 창이 16:9보다 좁으면 상하에 검은색 바 배치 (레터박스)
            SetBar(_firstBar, Vector2.zero, new Vector2(1f, viewport.yMin));
            SetBar(_secondBar, new Vector2(0f, viewport.yMax), Vector2.one);
        }
    }

    /// <summary> KDH : 지정한 Anchor 영역에 맞춰 레터박스 바의 크기와 위치를 설정한다. </summary>
    private static void SetBar(RectTransform bar, Vector2 min, Vector2 max)
    {
        // 부모 RectTransform 기준으로 검은색 바가 차지할 영역 지정
        bar.anchorMin = min;
        bar.anchorMax = max;
        
        // Anchor 경계에 맞춰 여백 없이 영역을 채우도록 설정
        bar.offsetMin = Vector2.zero;
        bar.offsetMax = Vector2.zero;
    }
    
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
                Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.StartBusiness);
                
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
                int satisfactionBefore = Managers.Data.GetSatisfaction();
                Managers.Data.AddSatisfaction(_customerSpawner.DailySatisfaction);
                int satisfactionAfter = Managers.Data.GetSatisfaction();

                // 쿠킹 컨트롤러를 종료한다.
                _cookingController.Clear();

                // 손님 스포너를 종료한다.
                _customerSpawner.StopBusiness();

                // 정산 팝업 (마지막 날인 경우에는 게임을 끝낸다.)
                ReputationProgressPopup progressPopup = Managers.UI.OpenPopup<ReputationProgressPopup>();
                if (progressPopup != null)
                {
                    progressPopup.Play(satisfactionBefore, satisfactionAfter, OpenSettlementPopup);
                }
                else
                {
                    OpenSettlementPopup();
                }

                break;
            case Phase.AfterBusiness: // 영업 후 => 잠입 씬 전환
                Managers.UI.OpenLoadingUI(0.5f, () => { Managers.Scene.LoadSceneWithLoading(SceneType.StealthScene); });
                break;
        }

        if (_currentPhase < Phase.AfterBusiness)
        {
            _currentPhase++;
            UpdatePhaseUI();
        }
    }

    private void OpenSettlementPopup()
    {
        Managers.UI.OpenPopup<SettlementPopup>().Set(_dailyMenu, _foodListScroll.FishListScroll, IsFinalDay(), TargetGold,
            () => Managers.Tutorial.Show(TutorialTrigger.AfterBusiness));
    }

    /// <summary> KDH : 현재 페이즈에 맞춰 UI를 업데이트한다. </summary>
    private void UpdatePhaseUI()
    {
        if (_tycoonHUD == null)
        {
            return;
        }

        switch (_currentPhase)
        {
            case Phase.BeforeBusiness:
                _tycoonHUD.SetStoreInfoVisible(false); // 만족도, 소지금 UI를 숨긴다.
                _tycoonHUD.SetQuestVisible(false);
                _tycoonHUD.SetFoodSaleLimit(_foodListScroll != null ? _foodListScroll.MaxSelectableFoodCount : 0);
                _tycoonHUD.OpenFoodDiary();
                _tycoonHUD.SetNextBtnVisible(false);
                _tycoonHUD.SetStoreBtnVisible(false);
                _tycoonHUD.SetBusinessProgressVisible(false);
                _tycoonHUD.SetQuickSlotVisible(false);

                _tycoonBgCtrl.ShowBeforeOpening();
                break;
            case Phase.DuringBusiness:
                _tycoonBgCtrl.ShowOpened(() =>
                {
                    if (_currentPhase == Phase.DuringBusiness)
                    {
                        Managers.Tutorial.Show(TutorialTrigger.DuringBusiness);
                    }
                });

                _tycoonHUD.SetQuestVisible(Managers.Data != null &&
                                           Managers.Data.GetCurrentDay() > 1); // 1일차 영업전은 퀘스트 숨긴다.
                _tycoonHUD.SetBusinessProgressVisible(true);
                break;
            case Phase.AfterBusiness:
                _tycoonHUD.SetStoreInfoVisible(true); // 만족도, 소지금 UI를 보인다.
                _tycoonHUD.SetQuestVisible(true);
                _tycoonHUD.DeactivateFoodDiary();
                _tycoonHUD.SetNextBtnVisible(true);
                _tycoonHUD.SetStoreBtnVisible(Managers.Data?.GetCurrentDay() > 1); // 1일차에는 상점을 숨긴다.
                _tycoonHUD.SetBusinessProgressVisible(false);
                _tycoonHUD.SetQuickSlotVisible(true);

                _tycoonBgCtrl.ShowAfter();
                break;
        }
    }

    #endregion ===== 업데이트 =====

    #region ===== 상태 =====

    /// <summary> KDH : 마지막 날인지 확인한다.  </summary>
    private bool IsFinalDay()
    {
        if (Managers.Data.GetCurrentDay() >= FinalDay)
        {
            return true;
        }

        return false;
    }

    #endregion ===== 상태 =====

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
        if (_customerSpawner?.DeliverFood() == true)
        {
            Managers.Quest?.ReportProgress(Type, QuestType.Serving);
        }
    }

    #endregion ===== 이벤트 =====
}
