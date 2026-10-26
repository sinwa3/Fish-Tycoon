/// <summary> KYH : 좌석을 반납하고 가게를 떠나는 상태 </summary>
public class CustomerExitState : BaseCustomerState
{
    public CustomerExitState(CustomerStateMachine stateMachine, CustomerController customer) : base(stateMachine, customer)
    {
    }

    /// <summary> KYH : 퇴장 상태에 들어올 때 호출한다 </summary>
    public override void Enter()
    {
        Customer.PlayExiting();
    }

    /// <summary> KYH : 퇴장 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        if (!Customer.IsExitAnimationFinished())
        {
            return;
        }

        Customer.LeaveSeat();
        Customer.Despawn();
    }

    /// <summary> KYH : 퇴장 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
    }
}
