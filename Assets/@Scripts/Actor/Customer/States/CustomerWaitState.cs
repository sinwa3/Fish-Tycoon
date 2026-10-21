/// <summary> KYH : 주문한 요리를 기다리는 상태. 기다린 시간을 잰다 </summary>
public class CustomerWaitState : BaseCustomerState
{
    public CustomerWaitState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        Customer.ShowWaitGauge();
    }

    public override void Update(float deltaTime)
    {
        Customer.UpdateWaitGauge(StateMachine.StateElapsedTime);

        if (Customer.HasReceivedFood)
        {
            StateMachine.ChangeState<CustomerEatState>();
        }
    }

    public override void Exit()
    {
        Customer.RecordWaitTime(StateMachine.StateElapsedTime);
        Customer.HideOrderBubble();
        Customer.HideWaitGauge();
    }
}
