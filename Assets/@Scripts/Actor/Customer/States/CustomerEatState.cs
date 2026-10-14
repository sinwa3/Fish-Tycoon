/// <summary> KYH : 받은 요리를 먹는 상태 </summary>
public class CustomerEatState : BaseCustomerState
{
    public CustomerEatState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        // TODO: 먹는 모습
    }

    public override void Update(float deltaTime)
    {
        // TODO: 식사 시간이 지나면 결제 상태로 전환
    }

    public override void Exit()
    {
    }
}
