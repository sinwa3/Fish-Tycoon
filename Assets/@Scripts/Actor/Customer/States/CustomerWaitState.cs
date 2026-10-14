/// <summary> KYH : 주문한 요리를 기다리는 상태 </summary>
public class CustomerWaitState : BaseCustomerState
{
    public CustomerWaitState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
    }

    public override void Update(float deltaTime)
    {
        // TODO: 요리를 받으면 식사 상태로 전환
    }

    public override void Exit()
    {
        // TODO: 기다린 시간 기록 (만족도 계산용)
    }
}
