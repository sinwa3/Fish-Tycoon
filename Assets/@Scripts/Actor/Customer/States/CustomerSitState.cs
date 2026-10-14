/// <summary> KYH : 손님이 좌석에 앉는 상태 </summary>
public class CustomerSitState : BaseCustomerState
{
    public CustomerSitState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        // TODO: 앉는 모습 (스프라이트 교체 등)
    }

    public override void Update(float deltaTime)
    {
        // TODO: 주문 상태로 전환
    }

    public override void Exit()
    {
    }
}
