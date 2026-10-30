using UnityEngine;

/// <summary>SJY : 핀 조절형 자물쇠의 개인 핀 이동 및 고정 상태 관리</summary>
public class PinLockPin : MonoBehaviour
{

    [Header("핀 이동 설정")]
    /// <summary>SJY : 핀이 출발 위치에서 아래로 이동할 거리</summary>
    [SerializeField] private float _travelDistance = 1.5f;
    /// <summary>SJY : 핀이 바닥에 도착할때까지의 최소 시간</summary>
    [SerializeField] private float _minFallDuration = 0.4f;
    /// <summary>SJY : 핀이 바닥에 도착할때까지의 최대 시간</summary>
    [SerializeField] private float _maxFallDuration = 0.8f;
    /// <summary>SJY : 핀이 바닥에서 고정될 수 있는 대기시간</summary>
    [SerializeField] private float _botomWindowDuration = 0.25f;
    /// <summary>SJY : 핀이 원위치로 돌아오는데 걸리는 시간</summary>
    [SerializeField] private float _returnDuration = 0.2f;

    private Transform _pinMotion;
    private Transform _lockpickAnchor;

    private Vector3 _initPosition;
    /// <summary>SJY : S키로 핀을 튕겼을 때 내려가는 시간</summary>
    private float _fallDuration;
    /// <summary>SJY : 현재 상태 시작이후 경과 시간</summary>
    private float _elapsedTime;
    /// <summary>SJY : 복귀 시작시 얼마나 내려가있었는지</summary>
    private float _returnStartDistance;
    /// <summary>SJY : 출발위치에서 얼마나 이동했는지</summary>
    private float _currentDistance;
    private bool _hasInitPosition;
    private PinLockState _state = PinLockState.Idle;

    public PinLockState State => _state;
    public bool IsReady => _pinMotion != null && _lockpickAnchor != null;
    public bool IsFixed => _state == PinLockState.Fixed;
    public bool CanFix => _state == PinLockState.BottomWindow;
    public Transform LockpickAnchor => _lockpickAnchor;

    /// <summary>SJY : 최초 핀 위치 저장 및 상태 초기화</summary>
    public void Initialize()
    {
        InitializeReferences();

        if (!IsReady)
        {
            return;
        }

        if (!_hasInitPosition)
        {
            _initPosition = _pinMotion.localPosition;
            _hasInitPosition = true;
        }

        ResetPin();
    }

    private void InitializeReferences()
    {
        if (_pinMotion == null)
        {
            _pinMotion = gameObject.FindChild<Transform>("PinMotion", true);
        }

        if (_lockpickAnchor == null)
        {
            _lockpickAnchor = gameObject.FindChild<Transform>("LockPickAnchor", true);
        }

        if (!IsReady)
        {
            CPrint.Warning($"[PinLockPin] {name}의 PinMotion or LockpickAnchor를 찾지 못했습니다.");
        }
    }

    /// <summary>SJY : 핀을 시작 위치와 대기 상태로 초기화</summary>
    public void ResetPin()
    {
        if (!_hasInitPosition)
        {
            return;
        }

        _state = PinLockState.Idle;
        _elapsedTime = 0f;
        _currentDistance = 0f;
        _returnStartDistance = 0f;

        UpdatePinTransform();

    }

    /// <summary>SJY : 핀을 무작위 하강 시간으로 아래로 튕기기 시작</summary>
    public void FlickPin()
    {
        if (_state != PinLockState.Idle)
        {
            return;
        }

        float minDuration = Mathf.Max(0.01f, _minFallDuration);
        float maxDuration = Mathf.Max(minDuration, _maxFallDuration);

        _fallDuration = Random.Range(minDuration, maxDuration);
        _elapsedTime = 0f;
        _state = PinLockState.Falling;

        CPrint.Log($"[PinLockPin] {name} 하강 시간 : {_fallDuration:F2}초");
    }

    /// <summary>SJY : 현재 상태에 맞춰 핀의 하강, 바닥 대기 및 복귀를 갱신</summary>
    public void UpdateMovement(float deltaTime)
    {
        if (_state == PinLockState.Idle || _state == PinLockState.Fixed || !IsReady)
        {
            return;
        }

        _elapsedTime += deltaTime;

        switch (_state)
        {
            case PinLockState.Falling:
                UpdateFalling();
                break;
            case PinLockState.BottomWindow:
                UpdateBottomWindow();
                break;
            case PinLockState.Returning:
                UpdateReturning();
                break;
        }

        UpdatePinTransform();
    }

    /// <summary>SJY : 무작위 하강 시간에 맞춰 핀을 바닥으로 이동</summary>
    private void UpdateFalling()
    {
        float ratio = Mathf.Clamp01(_elapsedTime / _fallDuration);

        _currentDistance = _travelDistance * ratio;

        if (ratio >= 1f)
        {
            _currentDistance = _travelDistance;
            _elapsedTime = 0f;
            _state = PinLockState.BottomWindow;
        }
    }

    /// <summary>SJY : 바닥 고정 가능 시간이 끝나면 핀을 복귀 상태로 전환</summary>
    private void UpdateBottomWindow()
    {
        if (_elapsedTime >= Mathf.Max(0.01f, _botomWindowDuration))
        {
            StartReturn();
        }
    }

    /// <summary>SJY : 핀을 현재 위치에서 시작 위치까지 복귀</summary>
    private void UpdateReturning()
    {
        float ratio = Mathf.Clamp01(_elapsedTime / _returnDuration);

        _currentDistance = Mathf.Lerp(_returnStartDistance, 0f, ratio);

        if (ratio >= 1f)
        {
            ResetPin();
        }
    }

    /// <summary>SJY : 핀의 복귀 동작을 시작</summary>
    public void StartReturn()
    {
        if (_state == PinLockState.Idle ||  _state == PinLockState.Fixed)
        {
            return;
        }

        _returnStartDistance = _currentDistance;
        _elapsedTime = 0f;
        _state = PinLockState.Returning;
    }

    /// <summary>SJY : 성공한 핀을 바닥 위치에 고정</summary>
    public void FixPin()
    {
        if (_state != PinLockState.BottomWindow)
        {
            return;
        }

        _currentDistance = _travelDistance;
        _state = PinLockState.Fixed;

        UpdatePinTransform();
    }

    /// <summary>SJY : 현재 이동거리를 PinMotion의 Local Position에 반영</summary>
    private void UpdatePinTransform()
    {
        if (!IsReady || !_hasInitPosition)
        {
            return;
        }

        _pinMotion.localPosition = _initPosition + Vector3.down * _currentDistance;
    }
}
