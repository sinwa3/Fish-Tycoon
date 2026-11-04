using UnityEngine;

/// <summary>SJY : 플레이어가 바라보는 상호작용 오브젝트의 아웃라인을 제어한다.</summary>
[RequireComponent(typeof(PlayerInteractor))]
public class PlayerOutline : MonoBehaviour
{
    private PlayerInteractor _playerInteractor;
    private InteractableOutline _currentOutline;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        _playerInteractor = GetComponent<PlayerInteractor>();
    }

    private void OnEnable()
    {
        SubscribeEvents();
        UpdateOutline(_playerInteractor.CurrentTarget);
    }

    private void SubscribeEvents()
    {
        _playerInteractor.OnTargetChanged += UpdateOutline;
    }

    private void UpdateOutline(IInteractable target)
    {
        InteractableOutline nextOutline = null;

        if (target is Component targetComponnet)
        {
            nextOutline = targetComponnet.GetComponent<InteractableOutline>();

            if (nextOutline == null)
            {
                nextOutline = targetComponnet.GetComponentInChildren<InteractableOutline>();
            }
        }

        if (_currentOutline == nextOutline)
        {
            return;
        }

        if (_currentOutline != null)
        {
            _currentOutline.SetOutline(false);
        }

        _currentOutline = nextOutline;

        if (_currentOutline != null)
        {
            _currentOutline.SetOutline(true);
        }
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        UpdateOutline(null);
    }

    private void UnsubscribeEvents()
    {
        if (_playerInteractor != null)
        {
            _playerInteractor.OnTargetChanged -= UpdateOutline;
        }
    }
}
