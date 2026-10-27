using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>SJY : 회전형 자물쇠 미니게임의 락픽 조작 관리</summary>
public class RotationLockController : MonoBehaviour
{
    [SerializeField] private Transform _lockpickPivot;
    [SerializeField] private float _rotationSensitivity = 100f;
    [SerializeField] private float _minAngle = -90f;
    [SerializeField] private float _maxAngle = 90f;

    private float _currentAngle;
    private bool _isPlaying;

    public float CurrentAngle => _currentAngle;
    public bool IsPlaying => _isPlaying;

    private void Awake()
    {
        InitializeComponents();
    }


    void Update()
    {
        if (!_isPlaying)
        {
            return;
        }

        UpdateLockpickRotation();
    }

    private void OnDisable()
    {
        if (_isPlaying)
        {
            StopMinigame();
        }
    }

    /// <summary>SJY : 락픽 참조를 확인하고 회전 각도와 진행 상태를 초기화</summary>
    private void InitializeComponents()
    {
        if (_lockpickPivot == null)
        {
            CPrint.Warning("[RotationLockController] LockpickPivot이 연결되지 않았습니다.");

            _currentAngle = 0f;
            _isPlaying = false;
        }
    }

    /// <summary>SJY : 락픽을 초기화하고 플레이어 입력을 차단한 뒤 미니게임을 시작</summary>
    public void StartMiniGame()
    {
        if (_isPlaying || _lockpickPivot == null)
        {
            return;
        }

        _currentAngle = 0f;
        _isPlaying = true;

        Managers.Input.SetInputBlocked(this, true);
        Managers.Input.SetCursorLock(true);
        Managers.Input.ClearKeyAxisValue();

        UpdateLockpickTransform();
    }

    /// <summary>SJY : 미니게임을 종료하고 플레이어 입력 및 마우스 커서 상태를 복구</summary>
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

    /// <summary>SJY : 마우스 X축 입력을 받아 제한 각도 내에서 락픽의 회전값을 갱신</summary>
    private void UpdateLockpickRotation()
    {
        float mouseX = Managers.Input.MouseAxisX;

        if (Mathf.Approximately(mouseX, 0f))
        {
            return;
        }

        _currentAngle = Mathf.Clamp(_currentAngle - mouseX * _rotationSensitivity * Time.deltaTime, _minAngle, _maxAngle);

        UpdateLockpickTransform();
    }

    /// <summary>SJY : 현재 회전 각도를 락픽의 Transform에 적용</summary>
    private void UpdateLockpickTransform()
    {
        _lockpickPivot.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
    }

}
