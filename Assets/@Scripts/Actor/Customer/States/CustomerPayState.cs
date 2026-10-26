/// <summary> KYH : 식사를 마치고 값을 치르는 상태 </summary>
public class CustomerPayState : BaseCustomerState
{
    public CustomerPayState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    /// <summary> KYH : 결제 상태에 들어올 때 호출한다 </summary>
    public override void Enter()
    {
        Customer.PlayIdle();
        Customer.Pay();
    }

    /// <summary> KYH : 결제 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        if (StateMachine.StateElapsedTime >= CustomerTempValue.PayDuration)
        {
            StateMachine.ChangeState<CustomerExitState>();
        }
    }

    /// <summary> KYH : 결제 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
    }
}
