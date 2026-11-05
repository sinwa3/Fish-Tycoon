/// <summary> KYH : 받은 요리를 먹는 상태 </summary>
public class CustomerEatState : BaseCustomerState
{
    public CustomerEatState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    /// <summary> KYH : 식사 상태에 들어올 때 호출한다 </summary>
    public override void Enter()
    {
        Customer.PlayEating();
    }

    /// <summary> KYH : 식사 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        if (StateMachine.StateElapsedTime >= CustomerTempValue.EatDuration)
        {
            StateMachine.ChangeState<CustomerPayState>();
        }
    }

    /// <summary> KYH : 식사 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
    }
}
