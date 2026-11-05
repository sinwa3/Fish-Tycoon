/// <summary> KYH : 조사 목표로 걸어가 두리번거리는 상태 (수색도 이 상태) </summary>
public class EnemyInvestigateState : BaseEnemyState
{
    // 두리번거리는 중인지
    private bool _isLookingAround;

    public EnemyInvestigateState(EnemyStateMachine stateMachine, EnemyController enemy) : base(stateMachine, enemy)
    {
    }

    /// <summary> KYH : 조사 상태에 들어올 때 호출한다 </summary>
    protected override void Enter()
    {
        _isLookingAround = false;
        Enemy.Movement.SetSpeedMode(EnemySpeedMode.Walk);

        if (!Enemy.Movement.MoveTo(Enemy.Memory.InvestigateTarget))
        {
            StartLookAround();
        }
    }

    /// <summary> KYH : 조사 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        if (!_isLookingAround)
        {
            if (Enemy.Movement.HasArrived)
            {
                StartLookAround();
            }

            return;
        }

        if (Enemy.Look.IsLookAroundFinished)
        {
            FinishInvestigate();
        }
    }

    /// <summary> KYH : 조사 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
        Enemy.Movement.Stop();
        Enemy.Look.StopLook();
    }

    #region ===== 두리번, 끝내기 =====

    /// <summary> KYH : 멈추고 두리번을 시작한다 </summary>
    private void StartLookAround()
    {
        Enemy.Look.LookAround(EnemyTempValue.InvestigateLookAroundDuration);
        Enemy.Movement.Stop();
        _isLookingAround = true;
    }

    /// <summary> KYH : 조사 목표를 지우고 끝낸다 </summary>
    private void FinishInvestigate()
    {
        Enemy.Memory.ClearInvestigateTarget();
        Finish();
    }

    #endregion ===== 두리번, 끝내기 =====
}
