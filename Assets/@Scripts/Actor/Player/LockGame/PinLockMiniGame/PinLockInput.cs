using UnityEngine;

/// <summary>SJY : 핀 조절형 자물쇠의 선택, 튕기기 및 고정 입력 관리</summary>
public class PinLockInput : MonoBehaviour
{
    /// <summary>SJY : 이전 핀 이동 키</summary>
    public bool IsPreviousPressed => Managers.Input.KeyDown_A;
    /// <summary>SJY : 다음 핀 이동 키</summary>
    public bool IsNextPressed => Managers.Input.KeyDown_D;
    /// <summary>SJY : 핀 아래로 튕기는 키</summary>
    public bool IsFlickPressed => Managers.Input.KeyDown_S;
    /// <summary>SJY : 핀 고정 시도 키</summary>
    public bool IsFixPressed => Managers.Input.MouseDown_Left;
}
