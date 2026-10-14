using UnityEngine;

/// <summary> KYH : 손님 본체. 상태 머신을 돌린다 </summary>
public class CustomerController : MonoBehaviour
{
    #region ===== 행동 =====

    private readonly CustomerStateMachine _stateMachine = new();

    public string CurrentStateName => _stateMachine.CurrentStateName;

    #endregion ===== 행동 =====

    private void Awake()
    {
        InitializeStateMachine();
    }

    private void Update()
    {
        _stateMachine.UpdateCurrentState(Time.deltaTime);
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 손님 상태 7개를 상태 머신에 등록한다 </summary>
    private void InitializeStateMachine()
    {
        _stateMachine.RegisterState(new CustomerEnterState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerSitState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerOrderState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerWaitState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerEatState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerPayState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerExitState(_stateMachine, this));
    }

    #endregion ===== 초기화 =====
}
