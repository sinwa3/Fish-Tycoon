/// <summary> KYH : 식사를 마치고 값을 치르는 상태 </summary>
public class CustomerPayState : BaseCustomerState
{
    public CustomerPayState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        // TODO: 결제 처리 (골드, 만족도 연결은 다음 작업)
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
