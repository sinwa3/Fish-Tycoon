using UnityEngine;
using System;

/// <summary>SJY : 회전형 자물쇠 미니게임의 시작, 종료 및 진행 상태를 관리</summary>
public class RotationLockController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RotationLockInput _rotationLockInput;
    [SerializeField] private RotationLockLogic _rotationLockLogic;
    
    private bool _isPlaying;
    private bool _isUnlocked;
    
    public bool IsPlaying => _isPlaying;
    public bool IsUnlocked => _isUnlocked;

    public float CurrentAngle => _rotationLockInput != null ? _rotationLockInput.CurrentAngle : 0f;
    public float CurrentCylinderAngle => _rotationLockInput != null ? _rotationLockInput.CurrentCylinderAngle : 0f;

    public event Action OnUnlockCompleted;

    private void Awake()
    {
        InitializeComponents();
    }


    private void Update()
    {
        if (!_isPlaying || _isUnlocked || _rotationLockInput == null || _rotationLockLogic == null)
        {
            return;
        }

        float maxCylinderAngle = _rotationLockLogic.CalculateMaxCylinderAngle(_rotationLockInput.CurrentAngle);

        _rotationLockInput.UpdateInput(maxCylinderAngle);

        if (Managers.Input.Key_D && _rotationLockLogic.IsUnlockCompleted(_rotationLockInput.CurrentAngle, _rotationLockInput.CurrentCylinderAngle))
        {
            CompleteUnlock();
        }
    }

    /// <summary>SJY : 오브젝트 비활성화 시 미니게임을 종료</summary>
    private void OnDisable()
    {
        StopMiniGame();
    }

    /// <summary>SJY : 입력 및 판정 컴포넌트를 가져오고 초기 상태를 설정</summary>
    private void InitializeComponents()
    {
        if (_rotationLockInput == null)
        {
            _rotationLockInput = GetComponent<RotationLockInput>();
        }

        if (_rotationLockLogic == null)
        {
            _rotationLockLogic = GetComponent<RotationLockLogic>();
        }

        if (_rotationLockInput == null)
        {
            CPrint.Warning("[RotationLockController] RotationLockInput이 없습니다.");
        }

        if (_rotationLockLogic == null)
        {
            CPrint.Warning("[RotationLockController] RotationLockLogic이 없습니다.");
        }

        _isPlaying = false;

        _rotationLockInput?.Initialize();
    }

    #region ==== 미니게임 제어 ====

    /// <summary>SJY : 회전값과 정답각도를 초기화하고 미니게임을 시작</summary>
    public void StartMiniGame()
    {
        if (_isPlaying || _rotationLockInput == null || _rotationLockLogic == null)
        {
            return;
        }

        if (!_rotationLockInput.IsReady)
        {
            return;
        }

        _isUnlocked = false;

        _rotationLockInput.ResetRotation();
        _rotationLockLogic.Initialize();

        _isPlaying = true;

        Managers.Input.SetInputBlocked(this, true);
        Managers.Input.SetCursorLock(true);
        Managers.Input.ClearKeyAxisValue();
    }

    /// <summary>SJY : 미니게임을 종료하고 플레이어 입력 차단과 커서 잠금을 해제</summary>
    public void StopMiniGame()
    {
        if (!_isPlaying)
        {
            return;
        }

        _isPlaying = false;

        Managers.Input.SetInputBlocked(this, false);
        Managers.Input.SetCursorLock(false);
    }

    /// <summary>SJY : 자물쇠 해제 성공 상태로 전환하고 완료 이벤트를 발생</summary>
    private void CompleteUnlock()
    {
        if (_isUnlocked)
        {
            return;
        }

        _isUnlocked = true;

        CPrint.Log("[RotationLockController] 자물쇠 해제 성공");

        OnUnlockCompleted?.Invoke();
    }

    #endregion ==== 미니게임 제어 ====
}
