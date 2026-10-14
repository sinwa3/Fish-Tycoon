/// <summary> KYH : 가게를 떠나는 상태 </summary>
public class CustomerExitState : BaseCustomerState
{
    public CustomerExitState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        // TODO: 좌석 반납 후 사라지기
    }

    public override void Update(float deltaTime)
    {

    }

    public override void Exit()
    {

    }
}
