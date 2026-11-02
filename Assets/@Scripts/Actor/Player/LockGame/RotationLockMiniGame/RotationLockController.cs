using UnityEngine;
using System;

/// <summary>SJY : 회전형 자물쇠 미니게임의 시작, 종료 및 진행 상태를 관리</summary>
public class RotationLockController : MonoBehaviour
{

    [Header("내구도 감소 설정")]
    [SerializeField] private float _smallDamage = 20f;
    [SerializeField] private float _largeDamage = 30f;
    
    private RotationLockInput _rotationLockInput;
    private RotationLockLogic _rotationLockLogic;
    private LockpickDurability _lockpickDurability;
    
    private bool _isPlaying;
    private bool _isUnlocked;
    private bool _isReplacing;

    public bool IsPlaying => _isPlaying;
    public bool IsUnlocked => _isUnlocked;
    public bool IsReplacing => _isReplacing;

    public float CurrentAngle => _rotationLockInput != null ? _rotationLockInput.CurrentAngle : 0f;
    public float CurrentCylinderAngle => _rotationLockInput != null ? _rotationLockInput.CurrentCylinderAngle : 0f;

    public event Action OnUnlockCompleted;
    public event Action OnLockpickBroken;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        if (!_isPlaying || _isUnlocked || _isReplacing | _rotationLockInput == null || _rotationLockLogic == null)
        {
            return;
        }

        CheckUnlockAttempt();

        if (!_isPlaying || _isReplacing)
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

        if (_lockpickDurability == null)
        {
            _lockpickDurability = GetComponent<LockpickDurability>();
        }

        if (_rotationLockInput == null)
        {
            CPrint.Warning("[RotationLockController] RotationLockInput이 없습니다.");
        }

        if (_rotationLockLogic == null)
        {
            CPrint.Warning("[RotationLockController] RotationLockLogic이 없습니다.");
        }

        if (_lockpickDurability == null)
        {
            CPrint.Warning("[RotationLockController] LockpickDurability가 없습니다.");
        }

        _isPlaying = false;
        _isUnlocked = false;

        _rotationLockInput?.Initialize();
    }

    #region ==== 미니게임 제어 ====

    /// <summary>SJY : 회전값과 정답각도를 초기화하고 미니게임을 시작</summary>
    public void StartMiniGame()
    {
        if (_isPlaying || _rotationLockInput == null || _rotationLockLogic == null || _lockpickDurability == null || Managers.Input == null)
        {
            return;
        }

        if (!_rotationLockInput.IsReady)
        {
            return;
        }

        _isUnlocked = false;
        _isReplacing = false;

        _rotationLockInput.ResetRotation();
        _rotationLockLogic.Initialize();
        _lockpickDurability.Initialize();

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
        _isReplacing = false;

        if (Managers.Input != null)
        {
            Managers.Input.SetInputBlocked(this, false);
            Managers.Input.SetCursorLock(false);
        }
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

    #region ==== 내구도 ====

    /// <summary>SJY : D키로 해제를 시도했을 때 잘못된 각도라면 내구도를 감소</summary>
    private void CheckUnlockAttempt()
    {
        if (!Managers.Input.KeyDown_D || _lockpickDurability == null)
        {
            return;
        }

        float currentAngle = _rotationLockInput.CurrentAngle;

        if (_rotationLockLogic.IsCorrectAngle(currentAngle))
        {
            return;
        }

        float damage = _rotationLockLogic.IsOutsideFalloffRange(currentAngle) ? _largeDamage : _smallDamage;

        _lockpickDurability.ApplyDamage(damage);

        if (_lockpickDurability.IsBroken)
        {
            HandleLockpickBroken();
        }
    }

    /// <summary>SJY : 락픽이 파손되면 미니게임 종료 후 실패 이벤트를 발생 (추후 수정 - 종료시킬지 이어서할지)</summary>
    private void HandleLockpickBroken()
    {
        if (_isReplacing)
        {
            return;
        }
        
        _isReplacing = true;

        CPrint.Log("[RotationLockController] 락픽 파손");

        OnLockpickBroken?.Invoke();
    }

    public void ReplaceLockpick()
    {
        if (!_isPlaying || !_isReplacing || _rotationLockInput == null || _lockpickDurability == null)
        {
            return;
        }

        _rotationLockInput.ResetRotation();
        _lockpickDurability.Initialize();

        _isReplacing = false;

        CPrint.Log("[RotationLockController] 새 락픽 교체 완료");

    }

    #endregion ==== 내구도 ====
}
