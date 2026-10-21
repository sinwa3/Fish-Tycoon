/// <summary> KYH : 좌석을 반납하고 가게를 떠나는 상태 </summary>
public class CustomerExitState : BaseCustomerState
{
    public CustomerExitState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    public override void Enter()
    {
        Customer.PlayExiting();
    }

    public override void Update(float deltaTime)
    {
        if (!Customer.IsExitAnimationFinished())
        {
            return;
        }

        Customer.LeaveSeat();
        Customer.Despawn();
    }

    public override void Exit()
    {

    }
}
