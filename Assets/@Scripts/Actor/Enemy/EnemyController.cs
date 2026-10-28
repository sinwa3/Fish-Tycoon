using System;
using UnityEngine;

/// <summary> KYH : 집주인의 상태를 등록하고 우선순위 함수로 지금 할 상태를 고른다 </summary>
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

    #region ===== 날짜 =====

    // 수면하는 날
    private const int SleepDay = 1;
    // 날짜가 시작하는 첫날
    private const int FirstDay = 1;

    // 1일 차인지
    private bool _isSleepDay;

    #endregion ===== 날짜 =====

    private void Awake()
    {
        InitializeStateMachine();
    }

    private void Start()
    {
        InitializeDay();
    }

    private void Update()
    {
        UpdateStateSelection();
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

    /// <summary> KYH : 오늘이 수면하는 날인지 읽어 둔다 </summary>
    private void InitializeDay()
    {
        int day = Managers.Data.GetCurrentDay();

        if (day < FirstDay)
        {
            CPrint.Warning($"[EnemyController] {day}일 차, 저장 데이터 안 불러와짐");
        }

        _isSleepDay = day == SleepDay;
    }

    #endregion ===== 초기화 =====

    #region ===== 상태 선택 =====

    /// <summary> KYH : 우선순위 함수가 고른 상태로 바꾸고, 끝난 상태가 다시 골라지면 처음부터 시작한다 </summary>
    private void UpdateStateSelection()
    {
        Type nextState = SelectState();

        if (_stateMachine.IsCurrentState(nextState) && _stateMachine.IsCurrentStateFinished)
        {
            _stateMachine.RestartCurrentState();

            return;
        }

        _stateMachine.ChangeState(nextState);
    }

    /// <summary> KYH : 기억을 보고 맞는 상태의 타입을 돌려준다 </summary>
    private Type SelectState()
    {
        if (_isSleepDay)
        {
            return typeof(EnemySleepState);
        }

        if (Memory.AlertLevel == AlertLevel.Detected)
        {
            return typeof(EnemyChaseState);
        }

        if (Memory.HasInvestigateTarget)
        {
            return typeof(EnemyInvestigateState);
        }

        if (Memory.AlertLevel == AlertLevel.Suspicious)
        {
            return typeof(EnemySuspectState);
        }

        return typeof(EnemyPatrolState);
    }

    #endregion ===== 상태 선택 =====
}
