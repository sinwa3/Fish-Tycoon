using System.Collections.Generic;
using UnityEngine;

/// <summary> KYH : 지금 있는 층의 웨이포인트를 순서대로 도는 상태 </summary>
public class EnemyPatrolState : BaseEnemyState
{
    // 지금 층의 웨이포인트 번호
    private readonly List<int> _floorWaypointIndices = new();
    // 층 웨이포인트 목록에서 가고 있는 순서
    private int _floorOrder;
    // 웨이포인트로 가는 중인지
    private bool _isPatrolling;

    public EnemyPatrolState(EnemyStateMachine stateMachine, EnemyController enemy) : base(stateMachine, enemy)
    {
    }

    /// <summary> KYH : 순찰 상태에 들어올 때 호출한다 </summary>
    protected override void Enter()
    {
        Enemy.Movement.SetSpeedMode(EnemySpeedMode.Walk);
        SelectFloorWaypoints();
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

        if (_floorWaypointIndices.Count == 1)
        {
            _isPatrolling = false;

            return;
        }

        AdvanceFloorOrder();
        _isPatrolling = MoveToCurrentWaypoint();
    }

    /// <summary> KYH : 순찰 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
        Enemy.Movement.Stop();
    }

    /// <summary> KYH : 층의 웨이포인트를 고르고 가장 가까운 곳부터 돌게 한다 </summary>
    private void SelectFloorWaypoints()
    {
        EnemyPatrolRoute patrolRoute = Enemy.PatrolRoute;

        _floorWaypointIndices.Clear();
        _floorOrder = 0;

        if (patrolRoute == null)
        {
            return;
        }

        Vector3 enemyPosition = Enemy.transform.position;

        patrolRoute.GetFloorWaypointIndices(enemyPosition.y, _floorWaypointIndices);
        _floorOrder = GetNearestFloorOrder(patrolRoute, enemyPosition);
    }

    /// <summary> KYH : 웨이포인트에서 가장 가까운 순서 반환 </summary>
    private int GetNearestFloorOrder(EnemyPatrolRoute patrolRoute, Vector3 enemyPosition)
    {
        int nearestOrder = 0;
        float minSqrDistance = float.MaxValue;

        for (int i = 0; i < _floorWaypointIndices.Count; i++)
        {
            Transform waypoint = patrolRoute.GetWaypoint(_floorWaypointIndices[i]);

            if (waypoint == null)
            {
                continue;
            }

            float sqrDistance = (waypoint.position - enemyPosition).sqrMagnitude;

            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                nearestOrder = i;
            }
        }

        return nearestOrder;
    }

    /// <summary> KYH : 지금 웨이포인트로 출발하고 갈 수 있는지 돌려준다 </summary>
    private bool MoveToCurrentWaypoint()
    {
        EnemyPatrolRoute patrolRoute = Enemy.PatrolRoute;

        if (patrolRoute == null)
        {
            CPrint.Warning("[EnemyPatrolState] 순찰 루트 없음");

            return false;
        }

        int floorWaypointCount = _floorWaypointIndices.Count;

        if (floorWaypointCount == 0)
        {
            CPrint.Warning("[EnemyPatrolState] 웨이포인트 0개");

            return false;
        }

        for (int i = 0; i < floorWaypointCount; i++)
        {
            int waypointIndex = _floorWaypointIndices[_floorOrder];
            Transform waypoint = patrolRoute.GetWaypoint(waypointIndex);

            if (waypoint != null && Enemy.Movement.MoveTo(waypoint.position))
            {
                return true;
            }

            CPrint.Warning($"[EnemyPatrolState] {waypointIndex}번 웨이포인트로 갈 수 없음");
            AdvanceFloorOrder();
        }

        CPrint.Error("[EnemyPatrolState] 지금 층에 갈 수 있는 웨이포인트 없음, 이동 불가");

        return false;
    }

    /// <summary> KYH : 층 웨이포인트 목록에서 다음 순서로 넘긴다 </summary>
    private void AdvanceFloorOrder()
    {
        int floorWaypointCount = _floorWaypointIndices.Count;

        if (floorWaypointCount == 0)
        {
            return;
        }

        _floorOrder = (_floorOrder + 1) % floorWaypointCount;
    }
}
