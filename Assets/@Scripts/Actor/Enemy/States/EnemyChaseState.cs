/// <summary> KYH : 플레이어를 달려서 쫓는 상태 </summary>
public class EnemyChaseState : BaseEnemyState
{
    public EnemyChaseState(EnemyStateMachine stateMachine, EnemyController enemy) : base(stateMachine, enemy)
    {
    }

    /// <summary> KYH : 추격 상태에 들어올 때 호출한다 </summary>
    protected override void Enter()
    {
        // TODO : 달리기 속도, 기억에 게이지 감소 막기
    }

    /// <summary> KYH : 추격 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        // TODO : 플레이어 위치로 계속 이동, 잡으면 RaisePlayerArrested, 놓치면 수색 목표를 적고 Finish
    }

    /// <summary> KYH : 추격 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
        // TODO : 기억에 게이지 감소 허락
    }
}
