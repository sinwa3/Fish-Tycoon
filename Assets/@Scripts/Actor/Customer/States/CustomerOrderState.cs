/// <summary> KYH : 손님이 메뉴를 주문하는 상태 </summary>
public class CustomerOrderState : BaseCustomerState
{
    public CustomerOrderState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        // TODO: 주문 메뉴 정하기, 말풍선 표시
    }

    public override void Update(float deltaTime)
    {
        if (StateMachine.StateElapsedTime >= CustomerTempValue.OrderDuration)
        {
            StateMachine.ChangeState<CustomerWaitState>();
        }
    }

    public override void Exit()
    {
    }
}
