/// <summary> KYH : 손님이 좌석에 앉는 상태 </summary>
public class CustomerSitState : BaseCustomerState
{
    public CustomerSitState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    /// <summary> KYH : 착석 상태에 들어올 때 호출한다 </summary>
    public override void Enter()
    {
        // TODO : 앉는 모습 (스프라이트 교체 등)
    }

    /// <summary> KYH : 착석 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        StateMachine.ChangeState<CustomerOrderState>();
    }

    /// <summary> KYH : 착석 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
    }
}
