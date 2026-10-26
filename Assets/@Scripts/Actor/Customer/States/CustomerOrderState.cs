/// <summary> KYH : 손님이 메뉴를 주문하는 상태 </summary>
public class CustomerOrderState : BaseCustomerState
{
    public CustomerOrderState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    /// <summary> KYH : 주문 상태에 들어올 때 호출한다 </summary>
    public override void Enter()
    {
    }

    /// <summary> KYH : 주문 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        if (StateMachine.StateElapsedTime >= CustomerTempValue.OrderDuration)
        {
            Customer.Order();
            StateMachine.ChangeState<CustomerWaitState>();
        }
    }

    /// <summary> KYH : 주문 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
    }
}
