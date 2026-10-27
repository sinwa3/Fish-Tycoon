using UnityEngine;

/// <summary>SJY : 회전형 자물쇠 미니게임의 시작, 종료 및 진행 상태를 관리</summary>
public class RotationLockController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RotationLockInput _rotationLockInput;
    [SerializeField] private RotationLockLogic _rotationLockLogic;
    
    private bool _isPlaying;
    
    public bool IsPlaying => _isPlaying;
    public float CurrentAngle => _rotationLockInput != null ? _rotationLockInput.CurrentAngle : 0f;
    public float CurrentCylinderAngle => _rotationLockInput != null ? _rotationLockInput.CurrentCylinderAngle : 0f;

    private void Awake()
    {
        InitializeComponents();
    }


    private void Update()
    {
        if (!_isPlaying || _rotationLockInput == null || _rotationLockLogic == null)
        {
            return;
        }

        _rotationLockInput.UpdateInput(_rotationLockLogic.MaxCylinderAngle);
    }

    /// <summary>SJY : 오브젝트 비활성화 시 미니게임을 종료</summary>
    private void OnDisable()
    {
        StopMinigame();
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

    /// <summary>SJY : 회전각도를 초기화하고 플레이어 입력을 차단한 뒤 미니게임을 시작</summary>
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

        _rotationLockInput.ResetRotation();
        _isPlaying = true;

        Managers.Input.SetInputBlocked(this, true);
        Managers.Input.SetCursorLock(true);
        Managers.Input.ClearKeyAxisValue();
    }

    /// <summary>SJY : 미니게임을 종료하고 플레이어 입력 차단과 커서 잠금을 해제</summary>
    public void StopMinigame()
    {
        if (!_isPlaying)
        {
            return;
        }

        _isPlaying = false;

        Managers.Input.SetInputBlocked(this, false);
        Managers.Input.SetCursorLock(false);
    }

    #endregion ==== 미니게임 제어 ====
}
