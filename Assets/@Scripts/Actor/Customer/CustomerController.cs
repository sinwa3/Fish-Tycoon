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

    private void Awake()
    {
        InitializeStateMachine();
    }

    private void Update()
    {
        _stateMachine.UpdateCurrentState(Time.deltaTime);
    }

    #region ===== 초기화 =====

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

    /// <summary> KYH : 배정받은 좌석에 손님을 세우고 입장 상태부터 시작한다 </summary>
    public void Spawn(CustomerSeats customerSeats, int seatIndex)
    {
        if (customerSeats == null)
        {
            CPrint.Error("CustomerSeats 없음 (CustomerController)");

            return;
        }

        _customerSeats = customerSeats;
        _seatIndex = seatIndex;
        _hasReceivedFood = false;
        _waitTime = 0.0f;

        Transform seatTransform = _customerSeats.GetSeat(_seatIndex);

        if (seatTransform == null)
        {
            CPrint.Warning($"{_seatIndex} 자리 없음 (CustomerController)");

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
        Destroy(gameObject);
    }

    #endregion ===== 등장과 퇴장 =====

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
