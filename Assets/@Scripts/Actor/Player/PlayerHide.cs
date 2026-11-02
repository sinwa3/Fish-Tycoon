using System.Collections.Generic;
using UnityEngine;

/// <summary> YSW : 플레이어가 엄폐 오브젝트에 숨고 나오는 상태를 관리한다. 숨어 있는 동안 몸은 사라지고 시점만 숨는 위치로 옮겨진다. </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerHide : MonoBehaviour
{
    #region ===== 상태 =====

    private CharacterController _characterController;
    private PlayerMovement _playerMovement;
    private PlayerLook _playerLook;

    private readonly List<Renderer> _hiddenRenderers = new List<Renderer>();

    /// <summary> YSW : 현재 숨어 있는 엄폐 오브젝트 (숨어 있지 않으면 null) </summary>
    public HidingSpot CurrentSpot { get; private set; }

    /// <summary> YSW : 숨어 있는 상태 여부 </summary>
    public bool IsHiding => CurrentSpot != null;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 숨기에 필요한 컴포넌트를 가져온다. </summary>
    private void InitializeComponents()
    {
        _characterController = GetComponent<CharacterController>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerLook = GetComponent<PlayerLook>();
    }

    #endregion ===== 초기화 =====

    #region ===== 숨기 처리 =====

    /// <summary> YSW : 몸을 그 자리에서 사라지게 하고, 시점을 엄폐 오브젝트의 숨는 위치(눈 위치)로 옮긴다. </summary>
    public void EnterHiding(HidingSpot hidingSpot)
    {
        if (IsHiding || hidingSpot == null)
        {
            return;
        }

        CurrentSpot = hidingSpot;

        SetBodyActive(false);

        if (_playerLook != null)
        {
            _playerLook.AttachView(hidingSpot.HidePoint);
        }
    }

    /// <summary> YSW : 시점을 몸으로 되돌리고, 나오는 위치(없으면 원래 자리)에 몸을 다시 나타나게 한다. </summary>
    public void ExitHiding()
    {
        if (!IsHiding)
        {
            return;
        }

        Transform exitPoint = CurrentSpot.ExitPoint;

        if (exitPoint != null)
        {
            transform.SetPositionAndRotation(exitPoint.position, GetYawRotation(exitPoint));
        }

        if (_playerLook != null)
        {
            _playerLook.DetachView();
        }

        CurrentSpot = null;

        SetBodyActive(true);
    }

    /// <summary> YSW : 이동, 충돌, 렌더러를 한 번에 켜거나 끈다. </summary>
    private void SetBodyActive(bool isActive)
    {
        _playerMovement.enabled = isActive;

        if (_characterController != null)
        {
            _characterController.enabled = isActive;
        }

        SetRenderersVisible(isActive);
    }

    /// <summary> YSW : 숨을 때 켜져 있던 렌더러만 끄고, 나올 때 그 렌더러들만 다시 켠다. </summary>
    private void SetRenderersVisible(bool isVisible)
    {
        if (isVisible)
        {
            foreach (Renderer bodyRenderer in _hiddenRenderers)
            {
                if (bodyRenderer != null)
                {
                    bodyRenderer.enabled = true;
                }
            }

            _hiddenRenderers.Clear();

            return;
        }

        _hiddenRenderers.Clear();

        foreach (Renderer bodyRenderer in GetComponentsInChildren<Renderer>())
        {
            if (!bodyRenderer.enabled)
            {
                continue;
            }

            bodyRenderer.enabled = false;
            _hiddenRenderers.Add(bodyRenderer);
        }
    }

    /// <summary> YSW : 지정한 지점의 좌우 회전만 반환한다. (플레이어가 기울지 않도록) </summary>
    private Quaternion GetYawRotation(Transform point)
    {
        return Quaternion.Euler(0f, point.eulerAngles.y, 0f);
    }

    #endregion ===== 숨기 처리 =====
}
