/// <summary> KYH : 주문한 요리를 기다리는 상태. 기다린 시간을 잰다 </summary>
public class CustomerWaitState : BaseCustomerState
{
    public CustomerWaitState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    /// <summary> KYH : 대기 상태에 들어올 때 호출한다 </summary>
    public override void Enter()
    {
        Customer.ShowWaitGauge();
    }

    /// <summary> KYH : 대기 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        Customer.UpdateWaitGauge(StateMachine.StateElapsedTime);

        if (Customer.HasReceivedFood)
        {
            StateMachine.ChangeState<CustomerEatState>();
        }
    }

    /// <summary> KYH : 대기 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
        Customer.RecordWaitTime(StateMachine.StateElapsedTime);
        Customer.HideOrderBubble();
        Customer.HideWaitGauge();
    }
}
