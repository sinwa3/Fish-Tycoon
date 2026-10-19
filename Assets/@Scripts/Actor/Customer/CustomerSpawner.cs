using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> KYH : 입장 간격마다 손님을 들여보내고 손님을 다 받아 모두 나가면 영업 종료를 알린다 </summary>
public class CustomerSpawner : MonoBehaviour
{
    #region ===== 연결 =====

    // 자식 오브젝트의 좌석
    private CustomerSeats _customerSeats;

    // TODO : 경로가 정해지면 Managers.Resource.Load로 불러온다
    [SerializeField] private CustomerController _customerPrefab;

    #endregion ===== 연결 =====

    #region ===== 영업 =====

    private bool _isOpen;
    private int _dailyCustomerCount;
    private int _spawnedCount;
    private float _spawnInterval;
    private float _spawnTimer;

    // 그날 메뉴. 손님마다 Spawn 때 넘긴다
    private DailyMenu _dailyMenu;

    // 만석이면 true CustomerSeats.OnFullChanged로 바뀐다
    private bool _isTimerPaused;

    public bool IsOpen => _isOpen;
    public int SpawnedCount => _spawnedCount;
    public int DailyCustomerCount => _dailyCustomerCount;

    // 요리 쪽이 읽는 섞은 그날 메뉴 [번호]가 현재, [번호 + 1]이 다음 음식
    public IReadOnlyList<int> ShuffledCandidates => _dailyMenu?.ShuffledCandidates ?? Array.Empty<int>();

    // 지금까지 주문한 손님 수. 요리는 주문한 만큼만 시작한다
    public int OrderedCount => _dailyMenu == null ? 0 : _dailyMenu.OrderedCount;

    /// <summary> KYH : 하루 손님을 다 받고 마지막 손님이 나가면 발행 </summary>
    public event Action OnBusinessClosed;

    #endregion ===== 영업 =====

    #region ===== 손님 목록 =====

    // 가게 안에 있는 손님 (들어온 순서)
    private readonly List<CustomerController> _activeCustomers = new();

    public IReadOnlyList<CustomerController> ActiveCustomers => _activeCustomers;

    #endregion ===== 손님 목록 =====

    private void Awake()
    {
        InitializeComponents();
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Update()
    {
        UpdateSpawnTimer(Time.deltaTime);
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 자식의 좌석을 찾아 둔다 </summary>
    private void InitializeComponents()
    {
        _customerSeats = GetComponentInChildren<CustomerSeats>(true);
    }

    #endregion ===== 초기화 =====

    #region ===== 이벤트 등록 =====

    /// <summary> KYH : 좌석의 만석 알림을 받도록 등록한다 </summary>
    private void SubscribeEvents()
    {
        if (_customerSeats == null)
        {
            CPrint.Warning("[CustomerSpawner] CustomerSeats 없음");

            return;
        }

        _customerSeats.OnFullChanged += HandleFullChanged;
    }

    /// <summary> KYH : 좌석의 만석 알림 등록을 해제한다 </summary>
    private void UnsubscribeEvents()
    {
        if (_customerSeats == null)
        {
            return;
        }

        _customerSeats.OnFullChanged -= HandleFullChanged;
    }

    #endregion ===== 이벤트 등록 =====

    #region ===== 영업 시작 =====

    /// <summary> KYH : 영업을 시작한다. 하루 손님 수와 입장 간격, 그날 메뉴를 받는다 </summary>
    public void StartBusiness(int dailyCustomerCount, float spawnInterval, DailyMenu dailyMenu)
    {
        if (_isOpen)
        {
            CPrint.Warning("[CustomerSpawner] 이미 영업 중");

            return;
        }

        if (dailyMenu == null)
        {
            CPrint.Warning("[CustomerSpawner] 그날 메뉴 없음");

            return;
        }

        if (dailyCustomerCount <= 0)
        {
            CPrint.Warning("[CustomerSpawner] 손님 수가 0 이하");

            return;
        }

        if (spawnInterval <= 0.0f)
        {
            CPrint.Warning("[CustomerSpawner] 스폰 인터벌이 0 이하");

            return;
        }

        _dailyMenu = dailyMenu;
        _dailyCustomerCount = dailyCustomerCount;
        _spawnInterval = spawnInterval;

        _spawnTimer = 0.0f;
        _spawnedCount = 0;

        _dailyMenu.ShuffleCandidates(_dailyCustomerCount);

        _isTimerPaused = _customerSeats != null ? _customerSeats.IsFull : false;
        _isOpen = true;
    }

    #endregion ===== 영업 시작 =====

    #region ===== 입장 =====

    /// <summary> KYH : 입장 타이머를 진행하고 간격이 차면 손님을 들여보낸다 </summary>
    private void UpdateSpawnTimer(float deltaTime)
    {
        if (!_isOpen || _isTimerPaused || _spawnedCount >= _dailyCustomerCount)
        {
            return;
        }

        _spawnTimer += deltaTime;

        if (_spawnTimer < _spawnInterval)
        {
            return;
        }

        _spawnTimer -= _spawnInterval;
        SpawnCustomer();
    }

    /// <summary> KYH : 손님 한 명을 만들어 빈 좌석에 앉힌다 </summary>
    private void SpawnCustomer()
    {
        if (_customerSeats == null)
        {
            CPrint.Warning("[CustomerSpawner] CustomerSeats 없음");

            return;
        }

        if (_customerPrefab == null)
        {
            CPrint.Warning("[CustomerSpawner] customerPrefab 없음");

            return;
        }

        CustomerController customer = Instantiate(_customerPrefab);

        if (!_customerSeats.TryAssignSeat(customer, out int seatIndex))
        {
            Destroy(customer.gameObject);

            return;
        }
        customer.OnDespawned += HandleCustomerDespawned;
        _activeCustomers.Add(customer);

        customer.Spawn(_customerSeats, seatIndex, _dailyMenu);
        _spawnedCount++;
    }

    #endregion ===== 입장 =====

    #region ===== 이벤트 처리 =====

    /// <summary> KYH : 만석이면 입장 타이머를 멈추고, 자리가 비면 다시 돌린다 </summary>
    private void HandleFullChanged(bool isFull)
    {
        _isTimerPaused = isFull;
    }

    /// <summary> KYH : 나간 손님을 목록에서 빼고 영업 종료인지 확인한다 </summary>
    private void HandleCustomerDespawned(CustomerController customer)
    {
        customer.OnDespawned -= HandleCustomerDespawned;
        _activeCustomers.Remove(customer);
        CheckBusinessClosed();
    }

    #endregion ===== 이벤트 처리 =====

    #region ===== 영업 종료 =====

    /// <summary> KYH : 하루 손님을 다 받았고 가게에 아무도 없으면 영업 종료를 알린다 </summary>
    private void CheckBusinessClosed()
    {
        if (_activeCustomers.Count > 0)
        {
            return;
        }

        if (_spawnedCount < _dailyCustomerCount)
        {
            return;
        }

        _isOpen = false;
        OnBusinessClosed?.Invoke();
    }

    #endregion ===== 영업 종료 =====
}
