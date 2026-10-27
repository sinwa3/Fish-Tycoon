using UnityEngine;

/// <summary> KYH : 집주인의 상태를 등록하고 현재 상태를 갱신한다 </summary>
public class EnemyController : MonoBehaviour
{
    #region ===== 상태 =====

    private readonly EnemyStateMachine _stateMachine = new();

    /// <summary> KYH : 현재 상태의 이름 (테스트 화면 표시용) </summary>
    public string CurrentStateName => _stateMachine.CurrentStateName;

    #endregion ===== 상태 =====

    #region ===== 기억 =====

    /// <summary> KYH : 감지 결과를 담는 기억 </summary>
    public EnemyMemory Memory { get; } = new();

    #endregion ===== 기억 =====

    private void Awake()
    {
        InitializeStateMachine();
    }

    private void Update()
    {
        _stateMachine.UpdateCurrentState(Time.deltaTime);
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 집주인 상태 5개를 상태 머신에 등록한다 </summary>
    private void InitializeStateMachine()
    {
        _stateMachine.RegisterState(new EnemySleepState(_stateMachine, this));
        _stateMachine.RegisterState(new EnemyPatrolState(_stateMachine, this));
        _stateMachine.RegisterState(new EnemySuspectState(_stateMachine, this));
        _stateMachine.RegisterState(new EnemyInvestigateState(_stateMachine, this));
        _stateMachine.RegisterState(new EnemyChaseState(_stateMachine, this));
    }

    #endregion ===== 초기화 =====
}
