/// <summary> KYH : 좌석을 반납하고 가게를 떠나는 상태 </summary>
public class CustomerExitState : BaseCustomerState
{
    public CustomerExitState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        Customer.LeaveSeat();
        Customer.Despawn();
        // (퇴장 연출이 생기면 Despawn은 연출이 끝난 뒤 Update에서)
    }

    public override void Update(float deltaTime)
    {

    }

    public override void Exit()
    {

    }
}
