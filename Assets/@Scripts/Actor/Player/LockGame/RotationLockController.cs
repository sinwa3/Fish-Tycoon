using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>SJY : 회전형 자물쇠 미니게임의 락픽 조작 관리</summary>
public class RotationLockController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform _lockpickPivot;
    [SerializeField] private Transform _lockCylinderPivot;
    [SerializeField ] private Transform _tensionWrenchPivot;
    [SerializeField] private Transform _lockpickAnchor;

    [Header("락픽 세팅")]
    [SerializeField] private float _rotationSensitivity = 100f;
    [SerializeField] private float _minAngle = -90f;
    [SerializeField] private float _maxAngle = 90f;

    [Header("키 홀 실린더 세팅")]
    [SerializeField] private float _cylinderRotationSpeed = 90f;
    [SerializeField] private float _cylinderReturnSpeed = 180f;
    [SerializeField] private float _maxCylinderAngle = 90f;

    private float _currentAngle;
    private float _currentCylinderAngle;
    private bool _isPlaying;

    public float CurrentAngle => _currentAngle;
    public float CurrentCylinderAngle => _currentCylinderAngle;
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

        UpdateCylinderRotation();

        if (!Managers.Input.Key_D)
        {
            UpdateLockpickRotation();
        }

        UpdateLockpickTransform();
    }

    private void OnDisable()
    {
        if (_isPlaying)
        {
            StopMinigame();
        }
    }

    /// <summary>SJY : 필요한 참조를 확인하고 회전 각도와 진행 상태를 초기화</summary>
    private void InitializeComponents()
    {
        if (_lockpickPivot == null)
        {
            CPrint.Warning("[RotationLockController] LockpickPivot이 연결되지 않았습니다.");
        }

        if (_lockCylinderPivot == null)
        {
            CPrint.Warning("[RotationLockController] LockCylinderPivot이 연결되지 않았습니다.");
        }

        if (_tensionWrenchPivot == null)
        {
            CPrint.Warning("[RotationLockController] TensionWrenchPivot이 연결되지 않았습니다.");
        }

        if (_lockpickAnchor == null)
        {
            CPrint.Warning("[RotationLockController] LockpickAnchor가 연결되지 않았습니다.");
        }

        _currentAngle = 0f;
        _currentCylinderAngle = 0f;
        _isPlaying = false;
    }

    #region ==== 미니게임 제어 ====

    /// <summary>SJY : 회전각도를 초기화하고 플레이어 입력을 차단한 뒤 미니게임을 시작</summary>
    public void StartMiniGame()
    {
        if (_isPlaying || _lockpickPivot == null || _lockpickAnchor == null || _lockCylinderPivot == null || _tensionWrenchPivot == null)
        {
            return;
        }

        _currentAngle = 0f;
        _currentCylinderAngle = 0f;
        _isPlaying = true;

        Managers.Input.SetInputBlocked(this, true);
        Managers.Input.SetCursorLock(true);
        Managers.Input.ClearKeyAxisValue();

        UpdateCylinderTransform();
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

    #endregion ==== 미니게임 제어 ====

    #region ==== 락픽 회전 ====

    /// <summary>SJY : 마우스 X축 입력을 받아 제한 각도 내에서 락픽의 회전값을 갱신</summary>
    private void UpdateLockpickRotation()
    {
        float mouseX = Managers.Input.MouseAxisX;

        if (Mathf.Approximately(mouseX, 0f))
        {
            return;
        }

        _currentAngle = Mathf.Clamp(_currentAngle - mouseX * _rotationSensitivity * Time.deltaTime, _minAngle, _maxAngle);
    }

    /// <summary>SJY : 현재 회전 각도를 락픽의 Transform에 적용</summary>
    private void UpdateLockpickTransform()
    {
        _lockpickPivot.position = _lockpickAnchor.position;
        _lockpickPivot.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
    }

    #endregion ==== 락픽 회전 ====

    #region ==== 실린더 회전 ====

    /// <summary>SJY : D키 입력에 따라 실린더를 회전시키거나 원래 위치로 복귀</summary>
    private void UpdateCylinderRotation()
    {
        bool isRotating = Managers.Input.Key_D;

        if (isRotating)
        {
            _currentCylinderAngle = Mathf.MoveTowards(_currentCylinderAngle, _maxCylinderAngle, _cylinderRotationSpeed * Time.deltaTime);
        }
        else
        {
            _currentCylinderAngle = Mathf.MoveTowards(_currentCylinderAngle, 0f, _cylinderReturnSpeed * Time.deltaTime);
        }

        UpdateCylinderTransform();
    }

    /// <summary>SJY : 현재 실린더 각도를 실린더와 텐션 렌치의 Transform에 적용</summary>
    private void UpdateCylinderTransform()
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, _currentCylinderAngle);

        _lockCylinderPivot.localRotation = rotation;
        _tensionWrenchPivot.localRotation = rotation;
    }

    #endregion ==== 실린더 회전 ====

}
