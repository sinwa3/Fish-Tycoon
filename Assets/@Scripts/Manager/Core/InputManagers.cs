using System.Collections.Generic;
using UnityEngine;

/// <summary> KDH : 게임에서 사용하는 마우스와 키보드 입력을 관리한다. </summary>
public class InputManager
{
    #region ===== 마우스 =====

    public float MouseAxisX { get; private set; }
    public float MouseAxisY { get; private set; }
    public float MouseWheel { get; private set; }

    public bool MouseDown_Left => Input.GetMouseButtonDown(0);
    public bool MouseDown_Right => Input.GetMouseButtonDown(1);
    public bool MouseDown_Wheel => Input.GetMouseButtonDown(2);

    public Vector3 MousePosition => Input.mousePosition;
    
    #endregion ===== 마우스 =====

    #region ===== 키보드 =====

    private const float AxisSensitivity = 1f;
    private const float AxisReturnSpeedMultiplier = 2f;

    public float KeyAxisX { get; private set; }
    public float KeyAxisY { get; private set; }

    /// <summary> KDH : 입력 자체의 세기 </summary>
    public float KeyInputStrength => Mathf.Max(Mathf.Abs(KeyAxisX), Mathf.Abs(KeyAxisY));

    public bool Key_LeftShift => Input.GetKey(KeyCode.LeftShift);
    public bool KeyDown_LeftCtrl => Input.GetKeyDown(KeyCode.LeftControl);
    public bool KeyDown_Space => Input.GetKeyDown(KeyCode.Space);
    public bool KeyDown_Esc => Input.GetKeyDown(KeyCode.Escape);
    public bool KeyDown_Enter => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);

    public bool KeyDown_0 => Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0);
    public bool KeyDown_1 => Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1);
    public bool KeyDown_2 => Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
    public bool KeyDown_3 => Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3);
    public bool KeyDown_4 => Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4);
    public bool KeyDown_5 => Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5);
    public bool KeyDown_6 => Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6);
    public bool KeyDown_7 => Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7);
    public bool KeyDown_8 => Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8);
    public bool KeyDown_9 => Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9);
    
    public bool KeyDown_E => Input.GetKeyDown(KeyCode.E);
    public bool KeyDown_W => Input.GetKeyDown(KeyCode.W);
    public bool KeyDown_A => Input.GetKeyDown(KeyCode.A);
    public bool KeyDown_S => Input.GetKeyDown(KeyCode.S);
    public bool KeyDown_D => Input.GetKeyDown(KeyCode.D);
    
    #endregion ===== 키보드 =====

    #region ===== 상태 =====

    /// <summary> KDH : 입력을 차단 중인 외부 요청 목록 </summary>
    private readonly HashSet<object> _inputBlockers = new();
    
    /// <summary> KDH : 플레이어가 입력을 받을 수 있는 상태인지 나타낸다. </summary>
    public bool CanReceivePlayer => _inputBlockers.Count == 0;

    #endregion ===== 상태 =====

    /// <summary> KDH : 현재 프레임의 입력 값을 업데이트한다. </summary>
    public void OnUpdate()
    {
        UpdateMouseInput();
        UpdateKeyboardInput();
    }

    #region ===== 입력 업데이트 =====

    /// <summary> KDH : 마우스 입력 값을 업데이트한다. </summary>
    private void UpdateMouseInput()
    {
        // TODO: 마우스 민감도 설정
        MouseAxisX = Input.GetAxis("Mouse X") * 0.5f;
        MouseAxisY = Input.GetAxis("Mouse Y") * 0.5f;
        MouseWheel = Input.GetAxis("Mouse ScrollWheel");
    }

    /// <summary> KDH : 키보드 이동 축 값을 업데이트한다. </summary>
    private void UpdateKeyboardInput()
    {
        KeyAxisX = UpdateAxis(KeyAxisX, KeyCode.A, KeyCode.D);
        KeyAxisY = UpdateAxis(KeyAxisY, KeyCode.S, KeyCode.W);
    }

    /// <summary> KDH : 현재 입력 축 값을 초기화하여 이전 이동 입력이 유지되지 않도록 한다. </summary>
    public void ClearKeyAxisValue()
    {
        KeyAxisX = 0f;
        KeyAxisY = 0f;
    }
    
    /// <summary> KDH : 지정한 두 키의 입력에 따라 축 값을 부드럽게 변경한다. </summary>
    private float UpdateAxis(float currentValue, KeyCode negativeKey, KeyCode positiveKey)
    {
        bool isNegativePressed = Input.GetKey(negativeKey);
        bool isPositivePressed = Input.GetKey(positiveKey);

        if (isNegativePressed && !isPositivePressed)
        {
            if (currentValue > 0f)
            {
                currentValue = 0f;
            }

            return Mathf.MoveTowards(currentValue, -1f, Time.deltaTime * AxisSensitivity);
        }

        if (isPositivePressed && !isNegativePressed)
        {
            if (currentValue < 0f)
            {
                currentValue = 0f;
            }

            return Mathf.MoveTowards(currentValue, 1f, Time.deltaTime * AxisSensitivity);
        }

        return Mathf.MoveTowards(currentValue, 0f, Time.deltaTime * AxisSensitivity * AxisReturnSpeedMultiplier);
    }

    #endregion ===== 입력 업데이트 =====

    #region ===== 입력 제어 =====

    /// <summary> KDH : 지정한 요청자의 입력 차단 상태를 설정한다. </summary>
    public void SetInputBlocked(object requester, bool isBlocked)
    {
        if (requester == null)
        {
            return;
        }

        if (isBlocked)
        {
            _inputBlockers.Add(requester);

            return;
        }

        _inputBlockers.Remove(requester);
    }

    /// <summary> KDH : 마우스 커서의 잠금 상태를 설정한다. </summary>
    public void SetCursorLock(bool isLock)
    {
        Cursor.visible = !isLock;
        Cursor.lockState = isLock ? CursorLockMode.Locked : CursorLockMode.None;
    }

    #endregion ===== 입력 제어 =====
}