using System;
using UnityEngine;

/// <summary> KYH : 손님 본체. 상태 머신을 돌리고 상태들이 쓰는 기능과 정보를 제공한다 </summary>
public class CustomerController : MonoBehaviour
{
    #region ===== 행동 =====

    private readonly CustomerStateMachine _stateMachine = new();

    public string CurrentStateName => _stateMachine.CurrentStateName;

    #endregion ===== 행동 =====

    #region ===== 좌석 =====

    private CustomerSeats _customerSeats;
    private int _seatIndex = -1;

    #endregion ===== 좌석 =====

    #region ===== 음식과 대기 =====

    private bool _hasReceivedFood;

    // 주문 후 음식을 받을 때까지 걸린 시간
    private float _waitTime;

    public bool HasReceivedFood => _hasReceivedFood;
    public float WaitTime => _waitTime;

    #endregion ===== 음식과 대기 =====

    #region ===== 주문 =====

    // 자식 오브젝트의 말풍선. Awake에서 찾아 둔다
    private CustomerOrderBubble _orderBubble;

    // 스포너가 Spawn 때 넘겨주는 그날 메뉴
    private DailyMenu _dailyMenu;

    // 주문한 메뉴 ID, 주문 전이면 -1
    private int _orderedMenuId = -1;

    public int OrderedMenuId => _orderedMenuId;

    #endregion ===== 주문 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeStateMachine();
    }

    private void Update()
    {
        _stateMachine.UpdateCurrentState(Time.deltaTime);
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 자식의 주문 말풍선을 찾아 둔다 </summary>
    private void InitializeComponents()
    {
        _orderBubble = GetComponentInChildren<CustomerOrderBubble>(true);

        if (_orderBubble == null)
        {
            CPrint.Warning("[CustomerController] 말풍선 없음");
        }
    }

    /// <summary> KYH : 손님 상태 7개를 상태 머신에 등록한다 </summary>
    private void InitializeStateMachine()
    {
        _stateMachine.RegisterState(new CustomerEnterState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerSitState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerOrderState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerWaitState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerEatState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerPayState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerExitState(_stateMachine, this));
    }

    #endregion ===== 초기화 =====

    #region ===== 등장과 퇴장 =====

    /// <summary> KYH : 손님이 사라지기 직전에 발행. 스포너가 남은 손님 목록에서 빼는 용도 </summary>
    public event Action<CustomerController> OnDespawned;

    /// <summary> KYH : 배정받은 좌석에 손님을 세우고 입장 상태부터 시작한다 </summary>
    public void Spawn(CustomerSeats customerSeats, int seatIndex, DailyMenu dailyMenu)
    {
        if (customerSeats == null)
        {
            CPrint.Error("[CustomerController] CustomerSeats 없음");

            return;
        }

        _customerSeats = customerSeats;
        _seatIndex = seatIndex;
        _dailyMenu = dailyMenu;

        _hasReceivedFood = false;
        _waitTime = 0.0f;
        _orderedMenuId = -1;

        HideOrderBubble();

        Transform seatTransform = _customerSeats.GetSeat(_seatIndex);

        if (seatTransform == null)
        {
            CPrint.Warning($"[CustomerController] {_seatIndex} 자리 없음");

            return;
        }

        transform.position = seatTransform.position;
        _stateMachine.ChangeState<CustomerEnterState>();
    }

    /// <summary> KYH : 좌석을 반납한다 (퇴장 상태에서 호출) </summary>
    public void LeaveSeat()
    {
        if (_customerSeats == null)
        {
            return;
        }

        _customerSeats.ReleaseSeat(_seatIndex);
        _seatIndex = -1;
    }

    /// <summary> KYH : 손님을 없앤다 (지금은 Destroy) </summary>
    public void Despawn()
    {
        _stateMachine.Clear();
        OnDespawned?.Invoke(this);
        Destroy(gameObject);
    }

    #endregion ===== 등장과 퇴장 =====

    #region ===== 주문 처리 =====

    /// <summary> KYH : 그날 메뉴에서 하나를 주문하고 말풍선에 띄운다 (주문 상태에서 호출) </summary>
    public void Order()
    {
        if (_dailyMenu == null)
        {
            CPrint.Warning("[CustomerController] 그날 메뉴 없음");

            return;
        }

        _orderedMenuId = _dailyMenu.TakeNextMenu();

        if (_orderBubble == null)
        {
            return;
        }

        _orderBubble.Show(_orderedMenuId);
    }

    /// <summary> KYH : 주문 말풍선을 끈다 (대기 상태가 끝날 때 호출) </summary>
    public void HideOrderBubble()
    {
        if (_orderBubble == null)
        {
            return;
        }

        _orderBubble.Hide();
    }

    #endregion ===== 주문 처리 =====

    #region ===== 음식 =====

    /// <summary> KYH : 완성된 요리를 받는다. 대기 상태일 때만 받고 결과를 반환한다 </summary>
    public bool ReceiveFood()
    {
        if (!_stateMachine.IsCurrentState<CustomerWaitState>())
        {
            return false;
        }

        _hasReceivedFood = true;

        return true;
    }

    /// <summary> KYH : 대기 상태가 끝날 때 잰 대기 시간을 기록한다 </summary>
    public void RecordWaitTime(float waitTime)
    {
        _waitTime = waitTime;
    }

    #endregion ===== 음식 =====
}
