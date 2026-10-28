using UnityEngine;

/// <summary> KYH : 웨이포인트를 순서대로 도는 상태 </summary>
public class EnemyPatrolState : BaseEnemyState
{
    // 가고 있는 웨이포인트 번호 (상태 나갔다 와도 그대로)
    private int _waypointIndex;
    // 웨이포인트로 가는 중인지
    private bool _isPatrolling;

    public EnemyPatrolState(EnemyStateMachine stateMachine, EnemyController enemy) : base(stateMachine, enemy)
    {
    }

    /// <summary> KYH : 순찰 상태에 들어올 때 호출한다 </summary>
    protected override void Enter()
    {
        Enemy.Movement.SetSpeedMode(EnemySpeedMode.Walk);
        _isPatrolling = MoveToCurrentWaypoint();
    }

    /// <summary> KYH : 순찰 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        if (!_isPatrolling)
        {
            return;
        }

        if (!Enemy.Movement.HasArrived)
        {
            return;
        }

        if (Enemy.PatrolRoute.WaypointCount == 1)
        {
            _isPatrolling = false;

            return;
        }

        AdvanceWaypointIndex();
        _isPatrolling = MoveToCurrentWaypoint();
    }

    /// <summary> KYH : 순찰 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
        Enemy.Movement.Stop();
    }

    /// <summary> KYH : 지금 번호의 웨이포인트로 출발한다. 갈 수 없으면 다음 번호로 넘기고, 모든 지점이 안 되면 false </summary>
    private bool MoveToCurrentWaypoint()
    {
        EnemyPatrolRoute patrolRoute = Enemy.PatrolRoute;

        if (patrolRoute == null)
        {
            CPrint.Warning("[EnemyPatrolState] 순찰 루트 없음");

            return false;
        }

        int waypointCount = patrolRoute.WaypointCount;

        if (waypointCount == 0)
        {
            CPrint.Warning("[EnemyPatrolState] 웨이포인트 0개");

            return false;
        }

        if (_waypointIndex >= waypointCount)
        {
            _waypointIndex = 0;
        }

        for (int i = 0; i < waypointCount; i++)
        {
            Transform waypoint = patrolRoute.GetWaypoint(_waypointIndex);

            if (waypoint != null && Enemy.Movement.MoveTo(waypoint.position))
            {
                return true;
            }

            CPrint.Warning($"[EnemyPatrolState] {_waypointIndex}번 웨이포인트로 갈 수 없음");
            AdvanceWaypointIndex();
        }

        CPrint.Error("[EnemyPatrolState] 갈 수 있는 웨이포인트 없음, 이동 불가");

        return false;
    }

    /// <summary> KYH : 다음 웨이포인트 번호로 넘긴다 </summary>
    private void AdvanceWaypointIndex()
    {
        int waypointCount = Enemy.PatrolRoute.WaypointCount;

        if (waypointCount == 0)
        {
            return;
        }

        _waypointIndex = (_waypointIndex + 1) % waypointCount;
    }
}
