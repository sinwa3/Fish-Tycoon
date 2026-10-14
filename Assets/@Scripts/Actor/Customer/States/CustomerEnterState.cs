/// <summary> KYH : 손님이 가게에 들어오는 상태 </summary>
public class CustomerEnterState : BaseCustomerState
{
    public CustomerEnterState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        // TODO: 등장 연출이 생기면 여기서 시작
    }

    public override void Update(float deltaTime)
    {
        // TODO: 착석 상태로 전환
    }

    public override void Exit()
    {
    }
}
