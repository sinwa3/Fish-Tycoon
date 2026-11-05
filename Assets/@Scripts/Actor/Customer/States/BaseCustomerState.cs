/// <summary> KYH : 손님 상태 머신에서 사용하는 모든 상태의 기본 클래스 </summary>
public abstract class BaseCustomerState
{
    protected readonly CustomerStateMachine StateMachine;

    public CustomerController Customer { get; private set; }

    protected BaseCustomerState(CustomerStateMachine stateMachine, CustomerController customer)
    {
        StateMachine = stateMachine;
        Customer = customer;
    }

    /// <summary> KYH : 현재 상태에 진입할 때 호출한다 </summary>
    public abstract void Enter();

    /// <summary> KYH : 현재 상태를 갱신한다 </summary>
    public abstract void Update(float deltaTime);

    /// <summary> KYH : 현재 상태를 종료할 때 호출한다 </summary>
    public abstract void Exit();
}
