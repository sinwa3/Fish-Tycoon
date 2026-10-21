/// <summary> KYH : 식사를 마치고 값을 치르는 상태 </summary>
public class CustomerPayState : BaseCustomerState
{
    public CustomerPayState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        Customer.PlayIdle();
        Customer.Pay();
    }

    public override void Update(float deltaTime)
    {
        if (StateMachine.StateElapsedTime >= CustomerTempValue.PayDuration)
        {
            StateMachine.ChangeState<CustomerExitState>();
        }
    }

    public override void Exit()
    {
    }
}
