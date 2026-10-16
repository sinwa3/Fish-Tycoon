using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> KYH : 손님의 상태를 등록하고 현재 상태의 전환과 갱신을 관리한다 </summary>
public class CustomerStateMachine
{
    #region ===== 상태 =====

    private readonly Dictionary<Type, BaseCustomerState> _stateMap = new();

    private BaseCustomerState _currentState;

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
    public void RegisterState(BaseCustomerState state)
    {
        if (state == null)
        {
            CPrint.Warning("[CustomerStateMachine] 상태 없음");

            return;
        }

        Type stateType = state.GetType();

        if (_stateMap.ContainsKey(stateType))
        {
            CPrint.Warning($"[CustomerStateMachine] 이미 등록됨 [{stateType.Name}]");

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

    /// <summary> KYH : 지정한 타입의 상태로 전환한다 </summary>
    public void ChangeState<T>() where T : BaseCustomerState
    {
        Type type = typeof(T);

        if (!_stateMap.TryGetValue(type, out BaseCustomerState nextState))
        {
            CPrint.Error($"[CustomerStateMachine] {type.Name} 상태 등록 안됨");

            return;
        }

        if (_currentState == nextState)
        {
            return;
        }

        _currentState?.Exit();
        _currentState = nextState;
        _stateElapsedTime = 0.0f;
        _currentState.Enter();
    }

    /// <summary> KYH : 현재 상태를 종료하고 상태가 없는 처음으로 되돌린다 (재사용 대비) </summary>
    public void Clear()
    {
        _currentState?.Exit();
        _currentState = null;
        _stateElapsedTime = 0.0f;
    }

    #endregion ===== 상태 전환 =====

    #region ===== 상태 확인 =====

    /// <summary> KYH : 현재 상태가 지정한 타입인지 확인한다 </summary>
    public bool IsCurrentState<T>() where T : BaseCustomerState
    {
        return _currentState is T;
    }

    #endregion ===== 상태 확인 =====
}
