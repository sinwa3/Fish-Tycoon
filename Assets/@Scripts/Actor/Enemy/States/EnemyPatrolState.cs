/// <summary> KYH : 웨이포인트를 순서대로 도는 상태 </summary>
public class EnemyPatrolState : BaseEnemyState
{
    public EnemyPatrolState(EnemyStateMachine stateMachine, EnemyController enemy) : base(stateMachine, enemy)
    {
    }

    /// <summary> KYH : 순찰 상태에 들어올 때 호출한다 </summary>
    protected override void Enter()
    {
        // TODO : 걷기 속도, 다음 웨이포인트로 출발
    }

    /// <summary> KYH : 순찰 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        // TODO : 도착하면 다음 웨이포인트로. 끝나지 않는다(Finish 안 부름)
    }

    /// <summary> KYH : 순찰 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
        // TODO : 이동 멈춤
    }
}
