using UnityEngine;

/// <summary> KYH : 플레이어를 달려서 쫓는 상태 </summary>
public class EnemyChaseState : BaseEnemyState
{
    // 잡았는지
    private bool _isArrested;
    // 다음 목적지 갱신까지 남은 시간
    private float _repathTimer;

    public EnemyChaseState(EnemyStateMachine stateMachine, EnemyController enemy) : base(stateMachine, enemy)
    {
    }

    /// <summary> KYH : 추격 상태에 들어올 때 호출한다 </summary>
    protected override void Enter()
    {
        _isArrested = false;
        _repathTimer = 0.0f;
        Enemy.Movement.SetSpeedMode(EnemySpeedMode.Run);
    }

    /// <summary> KYH : 추격 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        if (_isArrested)
        {
            return;
        }

        if (Enemy.Sight.CanCatchPlayer())
        {
            ArrestPlayer();

            return;
        }

        if (GetTimeSinceLastSeen() > EnemyTempValue.LoseSightDuration)
        {
            LosePlayer();

            return;
        }

        UpdateChaseMove(deltaTime);
    }

    /// <summary> KYH : 추격 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
        Enemy.Movement.Stop();
    }

    #region ===== 쫓기 =====

    /// <summary> KYH : 간격마다 목적지를 다시 잡는다 </summary>
    private void UpdateChaseMove(float deltaTime)
    {
        _repathTimer -= deltaTime;

        if (_repathTimer > 0.0f)
        {
            return;
        }

        _repathTimer = EnemyTempValue.ChaseRepathInterval;
        Enemy.Movement.MoveTo(GetChaseTarget());
    }

    /// <summary> KYH : 쫓을 위치를 돌려준다 </summary>
    private Vector3 GetChaseTarget()
    {
        if (GetTimeSinceLastSeen() <= EnemyTempValue.ChaseTrackDuration)
        {
            return Enemy.Sight.PlayerPosition;
        }

        return Enemy.Memory.LastSeenPosition;
    }

    /// <summary> KYH : 플레이어를 못 본 지 몇 초인지 돌려준다 </summary>
    private float GetTimeSinceLastSeen()
    {
        return Time.time - Enemy.Memory.LastSeenTime;
    }

    #endregion ===== 쫓기 =====

    #region ===== 잡음, 놓침 =====

    /// <summary> KYH : 멈추고 잡힘 이벤트를 한 번 낸다 </summary>
    private void ArrestPlayer()
    {
        _isArrested = true;
        Enemy.Movement.Stop();
        Managers.Event.RaisePlayerArrested();
    }

    /// <summary> KYH : 수색 목표를 적고 끝낸다 </summary>
    private void LosePlayer()
    {
        Enemy.Memory.SetInvestigateTarget(Enemy.Memory.LastSeenPosition);
        Enemy.Memory.SetGaugeDecayAllowed(true);
        Finish();
    }

    #endregion ===== 잡음, 놓침 =====
}
