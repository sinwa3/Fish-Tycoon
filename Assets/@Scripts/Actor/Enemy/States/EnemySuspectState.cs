/// <summary> KYH : 멈춰서 마지막 자극 쪽을 바라보는 상태 </summary>
public class EnemySuspectState : BaseEnemyState
{
    public EnemySuspectState(EnemyStateMachine stateMachine, EnemyController enemy) : base(stateMachine, enemy)
    {
    }

    /// <summary> KYH : 의심 상태에 들어올 때 호출한다 </summary>
    protected override void Enter()
    {
        // TODO : 이동 멈춤
    }

    /// <summary> KYH : 의심 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        // TODO : 기억의 마지막 자극 위치를 바라봄. 끝나지 않는다(게이지가 내려가면 컨트롤러가 바꿈)
    }

    /// <summary> KYH : 의심 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
    }
}
