using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>SJY : 자물쇠 미니게임 테스트</summary>
public class RotationLockTest : MonoBehaviour
{
    [SerializeField] private RotationLockController _rotationLockController;

    /// <summary>SJY : 성공 이벤트를 구독하고 테스트 미니게임을 시작</summary>
    void Start()
    {
        if (_rotationLockController == null)
        {
            return;
        }

        _rotationLockController.OnUnlockCompleted += HandleUnlockCompleted;
        _rotationLockController.StartMiniGame();
    }

    /// <summary>SJY : ESC로 종료, 숫자 1로 재시작 (테스트 용)</summary>
    void Update()
    {
        if (_rotationLockController == null)
        {
            return;
        }

        if (Managers.Input.KeyDown_Esc)
        {
            _rotationLockController.StopMiniGame();
        }

        if (Managers.Input.KeyDown_1)
        {
            _rotationLockController.StartMiniGame();
        }

    }

    /// <summary>SJY : 자물쇠 해제 성공 이벤트를 수신하고 로그 출력</summary>
    private void HandleUnlockCompleted()
    {
        CPrint.Log("[RotationLockTest] 성공 이벤트 수신");
    }

    /// <summary>SJY : 테스트 종료 시 성공 이벤트 구독 해제</summary>
    private void OnDestroy()
    {
        if (_rotationLockController != null)
        {
            _rotationLockController.OnUnlockCompleted -= HandleUnlockCompleted;
        }
    }
}
