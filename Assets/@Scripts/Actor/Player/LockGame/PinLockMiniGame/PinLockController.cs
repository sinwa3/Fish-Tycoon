using System;
using UnityEngine;

/// <summary>SJY : 핀 조절형 자물쇠의 선택, 고정, 성공 및 파손 상태 관리</summary>
public class PinLockController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PinLockInput _pinLockInput;
    [SerializeField] private LockpickDurability _lockpickDurability;
    [SerializeField] private PinLockPin[] _pins;
    [SerializeField] private Transform _lockpickPivot;

    [Header("락픽 설정")]
    [SerializeField] private float _lockpickMoveSpeed = 8f;
    [SerializeField] private float _arrivalTolerance = 0.01f;
    [SerializeField] private float _resetMoveSpeed = 10f;

    [Header("내구도 설정")]
    [SerializeField] private float _failedAttempDamage = 25f;

    private int _curentPinIndex;
    private int _fixedPinCount;
    private bool _isPlaying;
    private bool _isUnlocked;
    private bool _isReplacing;

    public int CurrentPinIndex => _curentPinIndex;
    public int TotalPinCount => _pins != null ? _pins.Length : 0;
    public int FixedPinCount => _fixedPinCount;

    public bool IsPlaying => _isPlaying;
    public bool IsUnlocked => _isUnlocked;
    public bool IsReplacing => _isReplacing;

    public event Action OnUnlockCompleted;
    public event Action OnLockpickBroken;
    public event Action<int> OnPinFixed;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        
    }

    private void OnDisable()
    {
        StopMiniGame();
    }

    /// <summary>SJY : 입력, 판정 및 내구도 컴포넌트를 초기화</summary>
	private void InitializeComponents()
    {
        if (_pinLockInput == null)
        {
            _pinLockInput = GetComponent<PinLockInput>();
        }

        if (_lockpickDurability == null)
        {
            _lockpickDurability = GetComponent<LockpickDurability>();
        }

        if (_pinLockInput == null)
        {
            CPrint.Warning("[PinLockController] PinLockInput이 없습니다.");
        }

        if (_lockpickDurability == null)
        {
            CPrint.Warning("[PinLockController] LockpickDurability가 없습니다.");
        }

        _isPlaying = false;
        _isUnlocked = false;
        _isReplacing = false;

    }

    #region ==== 미니게임 제어 ====

    /// <summary>SJY : 핀과 락픽 내구도를 초기화하고 미니게임을 시작</summary>
    public void StartMiniGame()
    {
        if (_isPlaying || _pinLockInput == null || _lockpickDurability == null)
        {
            return;
        }

        if (_pins == null || _pins.Length == 0)
        {
            return;
        }

        ResetAllPins();
        _lockpickDurability.Initialize();

        _isPlaying = true;
        _isUnlocked = false;
        _isReplacing = false;

        Managers.Input.SetInputBlocked(this, true);
        Managers.Input.ClearKeyAxisValue();
    }

    /// <summary>SJY : 미니게임을 종료하고 플레이어 입력 차단을 해제</summary>
	public void StopMiniGame()
    {
        if (!_isPlaying)
        {
            return;
        }

        _isPlaying = false;
        _isReplacing= false;

        Managers.Input.SetInputBlocked(this, false);
    }

    /// <summary>SJY : 새로운 락픽으로 교체하고 모든 핀을 처음부터 다시 시작</summary>
	public void ReplaceLockpick()
    {
        if (!_isPlaying || !_isReplacing || _lockpickDurability == null)
        {
            return;
        }

        ResetAllPins();
        _lockpickDurability.Initialize();

        _isReplacing = false;

        CPrint.Log("[PinLockController] 새 락픽 교체 완료");
    }

    /// <summary>SJY : 모든 핀을 초기화하고 첫 번째 핀을 선택</summary>
	private void ResetAllPins()
    {
        _curentPinIndex = 0;
        _fixedPinCount = 0;

        foreach (PinLockPin pin in _pins)
        {
            pin.Initialize();
        }

        UpdateSelectionIndicator();
    }

    #endregion ==== 미니게임 제어 ====

    #region ==== 핀 제어 ====

    /// <summary>SJY : A와 D키로 선택 가능한 핀 사이를 이동</summary>
	private void UpdatePinSelection()
    {
        int nextIndex = _curentPinIndex;

        if (_pinLockInput.IsPreviousPressed)
        {
            nextIndex = Mathf.Max(0, _curentPinIndex - 1);
        }
        else if (_pinLockInput.IsNextPressed)
        {
            nextIndex = Mathf.Min(Mathf.Min(_fixedPinCount, _pins.Length - 1), _curentPinIndex + 1);
        }

        if (nextIndex == _curentPinIndex)
        {
            return;
        }

        _curentPinIndex = nextIndex;
    }

    /// <summary>SJY : 현재 핀의 고정 성공 여부를 판정</summary>
	private void CheckFixAttempt(PinLockPin currentPin)
    {
        if (currentPin.CanFix)
        {
            CompletePin(currentPin);
        }
        else
        {
            HandleFailedAttempt(currentPin);
        }
    }

    /// <summary>SJY : 현재 핀을 고정하고 다음 핀으로 진행하거나 해제 성공 처리</summary>
	private void CompletePin(PinLockPin currentPin)
    {
        currentPin.FixPin();

        int fixedIndex = _curentPinIndex;

        _fixedPinCount++;

        CPrint.Log($"[PinLockController] {fixedIndex + 1}번 핀 고정 성고");

        OnPinFixed?.Invoke(fixedIndex);

        if (_fixedPinCount >= _pins.Length)
        {
            CompleteUnlock();
            return;
        }

        _curentPinIndex = _fixedPinCount;
    }

    /// <summary>SJY : 잘못된 고정 시도 시 내구도를 감소시키고 현재 핀을 복귀</summary>
	private void HandleFailedAttempt(PinLockPin currentPin)
    {
        _lockpickDurability.ApplyDamage(_failedAttempDamage);

        currentPin.StartReturn();

        if (_lockpickDurability.IsBroken)
        {
            HandleLockpickBroken();
        }
    }

    /// <summary>SJY : 모든 핀을 고정했을 때 성공 이벤트를 발생</summary>
	private void CompleteUnlock()
    {
        if (_isUnlocked)
        {
            return;
        }

        _isUnlocked = true;

        CPrint.Log("[PinLockController] 자물쇠 해제 성공");

        OnUnlockCompleted?.Invoke();
    }

    /// <summary>SJY : 락픽 파손 시 입력을 중단하고 교체 요청 이벤트를 발생</summary>
	private void HandleLockpickBroken()
    {
        if (_isReplacing)
        {
            return;
        }

        _isReplacing = true;

        CPrint.Log("[PinLockController] 락픽 파손");

        OnLockpickBroken?.Invoke();
    }

    #endregion ==== 핀 제어 ====

    #region ==== 락픽 이동 ====

    /// <summary>SJY : 현재 선택된 핀의 Anchor 위치로 락픽을 이동</summary>
	private void UpdateSelectionIndicator()
    {
        if (_lockpickPivot == null || _pins == null || _pins.Length == 0)
        {
            return;
        }

        Transform targetAnchor = _pins[_curentPinIndex].LockpickAnchor;

        if (targetAnchor == null)
        {
            return;
        }

        _lockpickPivot.position = Vector3.MoveTowards(_lockpickPivot.position, targetAnchor.position, _lockpickMoveSpeed * Time.deltaTime);

    }

    /// <summary>SJY : 락픽을 첫 번째 핀의 Anchor 위치록 빠르게 이동</summary>
    private void ResetLockpickPosition()
    {
        if (_lockpickPivot == null || _pins == null || _pins.Length == 0)
        {
            return;
        }

        Transform targetAnchor = _pins[0].LockpickAnchor;

        if (targetAnchor != null)
        {
            _lockpickPivot.position = Vector3.MoveTowards(_lockpickPivot.position, targetAnchor.position, _resetMoveSpeed * Time.deltaTime);
        }
    }

    private bool IsLockpickArrived()
    {
        if (_lockpickPivot == null || _pins == null || _pins.Length == 0)
        {
            return false;
        }

        Transform targetAnchor = _pins[_curentPinIndex].LockpickAnchor;

        if (targetAnchor == null)
        {
            return false;
        }

        return Vector3.Distance(_lockpickPivot.position, targetAnchor.position) <= _arrivalTolerance;
    }

    #endregion ==== 락픽 이동 ====
}
