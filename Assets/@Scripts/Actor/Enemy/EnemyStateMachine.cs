using System;
using System.Collections.Generic;

/// <summary> KYH : 집주인의 상태를 등록하고 현재 상태의 전환과 갱신을 관리한다 </summary>
public class EnemyStateMachine
{
    #region ===== 상태 =====

    private readonly Dictionary<Type, BaseEnemyState> _stateMap = new();

    private BaseEnemyState _currentState;

    /// <summary> KYH : 현재 상태의 이름 (테스트 화면 표시용) </summary>
    public string CurrentStateName => _currentState != null ? _currentState.GetType().Name : string.Empty;

    #endregion ===== 상태 =====

    #region ===== 상태 시간 =====

    private float _stateElapsedTime;

    /// <summary> KYH : 현재 상태에 진입한 후 경과한 시간 </summary>
    public float StateElapsedTime => _stateElapsedTime;

    #endregion ===== 상태 시간 =====

    #region ===== 상태 등록 =====

    /// <summary> KYH : 상태 머신에서 사용할 상태를 등록한다 </summary>
    public void RegisterState(BaseEnemyState state)
    {
        if (state == null)
        {
            CPrint.Warning("[EnemyStateMachine] 상태 없음");

            return;
        }

        Type stateType = state.GetType();

        if (_stateMap.ContainsKey(stateType))
        {
            CPrint.Warning($"[EnemyStateMachine] 이미 등록됨 [{stateType.Name}]");

            return;
        }

        _stateMap.Add(stateType, state);
    }

    #endregion ===== 상태 등록 =====

    #region ===== 상태 갱신 =====

    /// <summary> KYH : 현재 상태의 경과 시간을 갱신하고 Update를 호출한다 </summary>
    public void UpdateCurrentState(float deltaTime)
    {
        if (_currentState == null)
        {
            return;
        }

        _stateElapsedTime += deltaTime;
        _currentState.Update(deltaTime);
    }

    #endregion ===== 상태 갱신 =====

    #region ===== 상태 전환 =====

    /// <summary> KYH : 지정한 타입의 상태로 전환한다 (우선순위 함수가 고른 타입을 받는다) </summary>
    public void ChangeState(Type type)
    {
        if (!_stateMap.TryGetValue(type, out BaseEnemyState nextState))
        {
            CPrint.Error($"[EnemyStateMachine] {type.Name} 상태 등록 안됨");

            return;
        }

        if (_currentState == nextState)
        {
            return;
        }

        _currentState?.Exit();
        _currentState = nextState;
        _stateElapsedTime = 0.0f;
        _currentState.BeginState();
    }

    /// <summary> KYH : 현재 상태를 나갔다가 처음부터 다시 들어온다 </summary>
    public void RestartCurrentState()
    {
        if (_currentState == null)
        {
            return;
        }

        _currentState.Exit();
        _stateElapsedTime = 0.0f;
        _currentState.BeginState();
    }

    /// <summary> KYH : 현재 상태를 종료하고 상태가 없는 처음으로 되돌린다 </summary>
    public void Clear()
    {
        _currentState?.Exit();
        _currentState = null;
        _stateElapsedTime = 0.0f;
    }

    #endregion ===== 상태 전환 =====

    #region ===== 상태 확인 =====

    /// <summary> KYH : 현재 상태가 지정한 타입인지 확인한다 (우선순위 함수가 고른 타입과 비교) </summary>
    public bool IsCurrentState(Type type)
    {
        if (_currentState == null)
        {
            return false;
        }

        return _currentState.GetType() == type;
    }

    /// <summary> KYH : 현재 상태가 할 일을 마쳤는지 </summary>
    public bool IsCurrentStateFinished => _currentState != null && _currentState.IsFinished;

    #endregion ===== 상태 확인 =====
}
