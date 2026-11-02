/// <summary> KYH : 조사 목표로 걸어가 두리번거리는 상태 (수색도 이 상태) </summary>
public class EnemyInvestigateState : BaseEnemyState
{
    public EnemyInvestigateState(EnemyStateMachine stateMachine, EnemyController enemy) : base(stateMachine, enemy)
    {
    }

    /// <summary> KYH : 조사 상태에 들어올 때 호출한다 </summary>
    protected override void Enter()
    {
        // TODO : 걷기 속도, 기억의 조사 목표로 출발
    }

    /// <summary> KYH : 조사 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        // TODO : 도착하면 두리번 → 두리번이 끝나면 기억의 조사 목표를 지우고 Finish
    }

    /// <summary> KYH : 조사 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
        // TODO : 이동 멈춤
    }
}
