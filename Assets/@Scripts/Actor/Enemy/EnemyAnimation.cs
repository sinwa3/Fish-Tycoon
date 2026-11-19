using UnityEngine;
using UnityEngine.AI;

/// <summary> GYH : 집주인의 애니메이션 </summary>
[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class EnemyAnimation : MonoBehaviour
{
    #region ===== 수치 =====

    // 값이 실제 속도를 따라가는 시간
    private const float SpeedDampTime = 0.1f;

    #endregion ===== 수치 =====

    #region ===== 상태 =====

    private EnemyController _enemy;
    private NavMeshAgent _agent;
    private Animator _animator;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        UpdateSpeed(Time.deltaTime);
        UpdateLookAround();
    }

    #region ===== 초기화 =====

    /// <summary> GYH : 컨트롤러, NavMeshAgent, 애니메이터를 찾는다 </summary>
    private void InitializeComponents()
    {
        _enemy = GetComponent<EnemyController>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        if (_enemy == null)
        {
            CPrint.Warning("[EnemyAnimation] 적 컨트롤러 없음");
        }

        if (_agent == null)
        {
            CPrint.Warning("[EnemyAnimation] Agent 없음");
        }

        if (_animator == null)
        {
            CPrint.Warning("[EnemyAnimation] 애니메이터 없음");
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 애니메이터 값 =====

    /// <summary> GYH : 이동 속도를 걷기 1, 달리기 2로 바꿔 파라미터 넣기</summary>
    private void UpdateSpeed(float deltaTime)
    {
        float speed = _agent.velocity.magnitude;
        float walkSpeed = _enemy.Detection.WalkSpeed;
        float runSpeed = _enemy.Detection.RunSpeed;

        float speedValue = speed <= walkSpeed
            ? Mathf.InverseLerp(0.0f, walkSpeed, speed)
            : 1.0f + Mathf.InverseLerp(walkSpeed, runSpeed, speed);

        _animator.SetFloat(AnimatorKey.Hash.Speed, speedValue, SpeedDampTime, deltaTime);
    }

    /// <summary> GYH : 두리번 중인지 파라미터 넣기 </summary>
    private void UpdateLookAround()
    {
        _animator.SetBool(AnimatorKey.Hash.IsLookingAround, _enemy.Look.IsLookAround);
    }

    #endregion ===== 애니메이터 값 =====
}
