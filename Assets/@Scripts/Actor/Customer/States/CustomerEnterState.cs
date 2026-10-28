/// <summary> KYH : 손님이 가게에 들어오는 상태 (지금은 자리에 바로 나타나므로 곧장 착석으로 넘어간다) </summary>
public class CustomerEnterState : BaseCustomerState
{
    public CustomerEnterState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    /// <summary> KYH : 입장 상태에 들어올 때 호출한다 </summary>
    public override void Enter()
    {
        Customer.PlayIdle();
    }

    /// <summary> KYH : 입장 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        StateMachine.ChangeState<CustomerSitState>();
    }

    /// <summary> KYH : 입장 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
    }
}
