using UnityEngine;

/// <summary>SJY : 핀 조절형 자물쇠의 개인 핀 이동 및 고정 상태 관리</summary>
public class PinLockPin : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform _pinMotion;

    [Header("핀 이동 설정")]
    [SerializeField] private float _travelDistance = 1.5f;
    [SerializeField] private float _minFallDuration = 0.4f;
    [SerializeField] private float _maxFallDuration = 0.8f;
    [SerializeField] private float _botomWindowDuration = 0.25f;
    [SerializeField] private float _returnDuration = 0.2f;

    private Vector3 _initPosition;
    private float _fallDuration;
    private float _elapsedTime;
    private float _returnStartDistance;
    private float _currentDistance;
    private bool _hasInitPosition;
    private PinLockState _state = PinLockState.Idle;

    public PinLockState State => _state;
    public bool IsReady => _pinMotion != null;
    public bool IsFixed => _state == PinLockState.Fixed;

    /// <summary>SJY : 최초 핀 위치 저장 및 상태 초기화</summary>
    public void Initialize()
    {
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
