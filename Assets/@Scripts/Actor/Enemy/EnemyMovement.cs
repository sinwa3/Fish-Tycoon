using UnityEngine;
using UnityEngine.AI;

/// <summary> KYH : NavMeshAgent로 집주인 움직이고 도착 판정 </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    private NavMeshAgent _agent;
    private NavMeshPath _path;

    /// <summary> KYH : 목표에 도착했는지 </summary>
    public bool HasArrived => _agent != null && _agent.remainingDistance <= _agent.stoppingDistance;

    private void Awake()
    {
        InitializeComponents();
    }

    /// <summary> KYH : NavMeshAgent를 찾고 경로를 만든다 </summary>
    private void InitializeComponents()
    {
        _agent = GetComponent<NavMeshAgent>();
        _path = new NavMeshPath();

        if (_agent == null)
        {
            CPrint.Warning("[EnemyMovement] Agent 없음");
        }
    }

    /// <summary> KYH : 목표까지 갈 수 있으면 출발하고 true, 갈 수 없으면 false를 돌려준다 </summary>
    public bool MoveTo(Vector3 target)
    {
        if (_agent == null)
        {
            return false;
        }

        if (!NavMesh.SamplePosition(target, out NavMeshHit hit, EnemyTempValue.SampleMaxDistance, _agent.areaMask))
        {
            return false;
        }

        if (!_agent.CalculatePath(hit.position, _path))
        {
            return false;
        }

        if (_path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        _agent.SetPath(_path);
        _agent.isStopped = false;

        return true;
    }

    /// <summary> KYH : 이동을 멈추고 경로를 지운다 </summary>
    public void Stop()
    {
        if (_agent == null)
        {
            return;
        }

        _agent.isStopped = true;
        _agent.ResetPath();
    }

    /// <summary> KYH : 걷기, 달리기에 맞는 속도로 바꾼다 </summary>
    public void SetSpeedMode(EnemySpeedMode speedMode)
    {
        if (_agent == null)
        {
            return;
        }

        switch (speedMode)
        {
            case EnemySpeedMode.Walk:
                _agent.speed = EnemyTempValue.WalkSpeed;
                break;
            case EnemySpeedMode.Run:
                _agent.speed = EnemyTempValue.RunSpeed;
                break;
        }

        // TODO : 애니메이터 Speed 값 넣기
    }
}
