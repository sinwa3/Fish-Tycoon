using UnityEngine;
using UnityEngine.AI;

/// <summary> KYH : 집주인이 걸은 거리만큼 발소리를 낸다 </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFootstep : MonoBehaviour
{
    #region ===== 수치 =====

    // 발소리 사이 거리
    private const float StepLength = 0.8f;
    // 층높이 차이
    private const float FloorHeightRange = 2.0f;
    // 발소리 크기
    private const float FootstepVolume = 1.0f;
    // 발소리 겹칠 수 있는 개수
    private const int MaxFootstepSourceCount = 2;

    #endregion ===== 수치 =====

    #region ===== 상태 =====

    private EnemyController _enemy;
    private NavMeshAgent _agent;

    // 마지막 발소리 뒤로 걸은 거리
    private float _stepDistance;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        UpdateFootstep(Time.deltaTime);
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 컨트롤러와 NavMeshAgent를 찾는다 </summary>
    private void InitializeComponents()
    {
        _enemy = GetComponent<EnemyController>();
        _agent = GetComponent<NavMeshAgent>();

        if (_enemy == null)
        {
            CPrint.Warning("[EnemyFootstep] 적 컨트롤러 없음");
        }

        if (_agent == null)
        {
            CPrint.Warning("[EnemyFootstep] Agent 없음");
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 발소리 =====

    /// <summary> KYH : 이번 프레임에 걸은 거리를 더하고 한 걸음이 차면 발소리를 낸다 </summary>
    private void UpdateFootstep(float deltaTime)
    {
        if (_agent == null)
        {
            return;
        }

        _stepDistance += _agent.velocity.magnitude * deltaTime;

        if (_stepDistance < StepLength)
        {
            return;
        }

        _stepDistance -= StepLength;
        PlayFootstep();
    }

    /// <summary> KYH : 플레이어와 같은 층이면 집주인 발밑에서 발소리를 낸다 </summary>
    private void PlayFootstep()
    {
        if (!IsPlayerOnSameFloor())
        {
            return;
        }

        Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.EnemyFoot, FootstepVolume, is3D: true, position: transform.position, maxAudioSourceCount: MaxFootstepSourceCount);
    }

    /// <summary> KYH : 플레이어가 집주인과 같은 층에 있는지 </summary>
    private bool IsPlayerOnSameFloor()
    {
        if (_enemy == null)
        {
            return false;
        }

        float distance = Mathf.Abs(_enemy.Sight.PlayerFootHeight - transform.position.y);

        return distance <= FloorHeightRange;
    }

    #endregion ===== 발소리 =====
}
