using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>SJY : 자물쇠 미니게임 테스트</summary>
public class RotationLockTest : MonoBehaviour
{
    [SerializeField] private RotationLockController _rotationLockController;

    void Start()
    {
        if (_rotationLockController == null)
        {
            return;
        }

        _rotationLockController.StartMiniGame();
    }


    void Update()
    {
        if (_rotationLockController == null)
        {
            return;
        }

        if (Managers.Input.KeyDown_Esc)
        {
            _rotationLockController.StopMinigame();
        }

        if (Managers.Input.KeyDown_1)
        {
            _rotationLockController.StartMiniGame();
        }

    }
}
