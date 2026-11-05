using UnityEngine;
using UnityEngine.AI;

/// <summary> KYH : NavMeshAgent로 집주인 움직이고 도착 판정 </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    #region ===== 상태 =====

    private NavMeshAgent _agent;
    private NavMeshPath _path;

    // 지금 지나가는 문 (없으면 null)
    private Door _door;
    // 문이 다 열릴 때까지 남은 시간
    private float _doorOpenTimer;
    // 문을 지난 뒤 다시 갈 목적지
    private Vector3 _destination;

    /// <summary> KYH : 목표에 도착했는지 </summary>
    public bool HasArrived => _agent != null && _door == null && _agent.remainingDistance <= _agent.stoppingDistance;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        UpdateDoorPassing(Time.deltaTime);
    }

    #region ===== 초기화 =====

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

    #endregion ===== 초기화 =====

    #region ===== 이동 =====

    /// <summary> KYH : 목표로 출발하고 갈 수 있는지 돌려준다 </summary>
    public bool MoveTo(Vector3 target)
    {
        if (_agent == null)
        {
            return false;
        }

        if (_door != null)
        {
            _destination = target;

            return true;
        }

        if (!SetAgentPath(target))
        {
            return false;
        }

        _destination = target;

        return true;
    }

    /// <summary> KYH : 목표까지 경로를 찾아 Agent에 넣는다 </summary>
    private bool SetAgentPath(Vector3 target)
    {
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

        ClearDoorPassing();

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

    #endregion ===== 이동 =====

    #region ===== 문 통과 =====

    /// <summary> KYH : 문 통과를 이어서 하거나 앞의 닫힌 문을 찾는다 </summary>
    private void UpdateDoorPassing(float deltaTime)
    {
        if (_agent == null)
        {
            return;
        }

        if (_door == null)
        {
            if (_agent.hasPath && TryFindClosedDoor(out Door door))
            {
                StartDoorPassing(door);
            }

            return;
        }

        UpdateDoorOpening(deltaTime);
    }

    /// <summary> KYH : 가는 방향 앞의 닫힌 문을 찾는다 </summary>
    private bool TryFindClosedDoor(out Door door)
    {
        door = null;

        Vector3 eyePosition = transform.position + Vector3.up * EnemyTempValue.EyeHeight;
        Vector3 direction = Vector3.ProjectOnPlane(_agent.steeringTarget - eyePosition, Vector3.up);

        // 목표가 문 앞이면 그 문은 찾지 않는다
        float detectDistance = Mathf.Min(EnemyTempValue.DoorDetectDistance, _agent.remainingDistance);

        bool isHit = Physics.Raycast
            (
            eyePosition,
            direction,
            out RaycastHit hit,
            detectDistance,
            LayerKey.Mask.Door,
            QueryTriggerInteraction.Ignore
            );

        if (!isHit)
        {
            return false;
        }

        door = hit.collider.GetComponentInParent<Door>();

        if (door == null)
        {
            return false;
        }

        if (door.IsOpen)
        {
            return false;
        }

        return true;
    }

    /// <summary> KYH : 문을 기억하고 연다 </summary>
    private void StartDoorPassing(Door door)
    {
        _door = door;
        OpenDoor();
    }

    /// <summary> KYH : 문을 열고 기다리기 시작한다 </summary>
    private void OpenDoor()
    {
        _agent.isStopped = true;
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;

        _door.Open(transform.position);
        _doorOpenTimer = EnemyTempValue.DoorOpenWaitDuration;
    }

    /// <summary> KYH : 문이 다 열릴 때까지 기다린다 </summary>
    private void UpdateDoorOpening(float deltaTime)
    {
        _doorOpenTimer -= deltaTime;

        if (_doorOpenTimer > 0.0f)
        {
            return;
        }

        FinishDoorPassing();
    }

    /// <summary> KYH : 문 통과를 끝내고 원래 목적지로 다시 출발한다 </summary>
    private void FinishDoorPassing()
    {
        ClearDoorPassing();
        SetAgentPath(_destination);
    }

    /// <summary> KYH : 문 통과 값을 지운다 </summary>
    private void ClearDoorPassing()
    {
        _door = null;
        _doorOpenTimer = 0.0f;
    }

    #endregion ===== 문 통과 =====
}
