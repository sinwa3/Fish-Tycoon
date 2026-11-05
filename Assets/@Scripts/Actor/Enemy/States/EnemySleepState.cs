/// <summary> KYH : 1일 차에 제자리에서 자는 상태. 깨지 않는다 </summary>
public class EnemySleepState : BaseEnemyState
{
    public EnemySleepState(EnemyStateMachine stateMachine, EnemyController enemy) : base(stateMachine, enemy)
    {
    }

    /// <summary> KYH : 수면 상태에 들어올 때 호출한다 </summary>
    protected override void Enter()
    {
        Enemy.Movement.Stop();
        Enemy.Look.StopLook();
    }

    /// <summary> KYH : 수면 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        // TODO : 게이지가 오르면 뒤척임 반응
    }

    /// <summary> KYH : 수면 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
    }
}
