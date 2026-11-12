using UnityEngine;

/// <summary> KYH : 조사 목표로 걸어가 두리번거리는 상태 (수색도 이 상태) </summary>
public class EnemyInvestigateState : BaseEnemyState
{
    // 두리번거리는 중인지
    private bool _isLookingAround;
    // 지금 걸어가는 조사 목표
    private Vector3 _moveTarget;

    public EnemyInvestigateState(EnemyStateMachine stateMachine, EnemyController enemy) : base(stateMachine, enemy)
    {
    }

    /// <summary> KYH : 조사 상태에 들어올 때 호출한다 </summary>
    protected override void Enter()
    {
        Enemy.Movement.SetSpeedMode(EnemySpeedMode.Walk);
        StartMove();
    }

    /// <summary> KYH : 조사 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        // 조사하는 중에 목표가 바뀌면 새 목표로 다시 간다
        if (Enemy.Memory.InvestigateTarget != _moveTarget)
        {
            StartMove();

            return;
        }

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

    #region ===== 이동, 두리번, 끝내기 =====

    /// <summary> KYH : 조사 목표로 걷기 시작하고 못 가면 그 자리에서 두리번거린다 </summary>
    private void StartMove()
    {
        _isLookingAround = false;
        _moveTarget = Enemy.Memory.InvestigateTarget;
        Enemy.Look.StopLook();

        if (!Enemy.Movement.MoveTo(_moveTarget))
        {
            StartLookAround();
        }
    }

    /// <summary> KYH : 멈추고 두리번을 시작한다 </summary>
    private void StartLookAround()
    {
        Enemy.Look.LookAround(Enemy.Detection.InvestigateLookAroundDuration);
        Enemy.Movement.Stop();
        _isLookingAround = true;
    }

    /// <summary> KYH : 조사 목표를 지우고 끝낸다 </summary>
    private void FinishInvestigate()
    {
        Enemy.Memory.ClearInvestigateTarget();
        Finish();
    }

    #endregion ===== 이동, 두리번, 끝내기 =====
}
