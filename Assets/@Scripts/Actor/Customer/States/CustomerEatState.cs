/// <summary> KYH : 받은 요리를 먹는 상태 </summary>
public class CustomerEatState : BaseCustomerState
{
    public CustomerEatState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        Customer.PlayEating();
    }

    public override void Update(float deltaTime)
    {
        if (StateMachine.StateElapsedTime >= CustomerTempValue.EatDuration)
        {
            StateMachine.ChangeState<CustomerPayState>();
        }
    }

    public override void Exit()
    {
    }
}
