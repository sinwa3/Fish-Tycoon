using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> SJY : 회전형 자물쇠의 회전 제한 및 해제 판정 규칙을 관리한다. </summary>
public class RotationLockLogic : MonoBehaviour
{
    [SerializeField] private float _maxCylinderAngle = 90f;

    public float MaxCylinderAngle => _maxCylinderAngle;
}
