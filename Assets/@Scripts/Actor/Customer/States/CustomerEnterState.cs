/// <summary> KYH : 손님이 가게에 들어오는 상태 (지금은 자리에 바로 나타나므로 곧장 착석으로 넘어간다) </summary>
public class CustomerEnterState : BaseCustomerState
{
    public CustomerEnterState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        // TODO : 등장 연출이 생기면 여기서 시작
    }

    public override void Update(float deltaTime)
    {
        StateMachine.ChangeState<CustomerSitState>();
    }

    public override void Exit()
    {
    }
}
