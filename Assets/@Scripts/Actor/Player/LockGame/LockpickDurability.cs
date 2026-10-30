using System;
using UnityEngine;

/// <summary>SJY : 보안 미니게임에서 공통으로 사용하는 락픽 내구도 및 파손 상태 관리</summary>
public class LockpickDurability : MonoBehaviour
{
    [Header("내구도 설정")]
    [SerializeField] private float _maxDurability = 100f;
    [SerializeField] private float _damageAmount = 20f;

    private float _currentDurability;
    private bool _isBroken;

    public float CurrentDurability => _currentDurability;
    public float MaxDurability => _maxDurability;
    public bool IsBroken => _isBroken;

    public event Action<float> OnDurabilityChanged;
    public event Action OnLockpickBroken;

    /// <summary>SJY : 락픽 내구도와 파손 상태를 초기화</summary>
    public void Initialize()
    {
        _currentDurability = Mathf.Max(1f, _maxDurability);
        _isBroken = false;

        OnDurabilityChanged?.Invoke(_currentDurability);
    }

    /// <summary>SJY : 기본 감소량 만큼 내구도 감소</summary>
    public void ApplyDamage()
    {
        ApplyDamage(_damageAmount);
    }

    /// <summary>SJY : 지정한 양만큼 내구도 감소시키고 파손 여부 확인</summary>
    public void ApplyDamage(float damage)
    {
        if (_isBroken || damage <= 0f)
        {
            return;
        }

        _currentDurability = Mathf.Max(0f, _currentDurability - damage);

        OnDurabilityChanged?.Invoke(_currentDurability);
        CPrint.Log($"[LockpickDurability] 남은 내구도 : {_currentDurability:F0}");

        if (_currentDurability <= 0f)
        {
            HandleLockpickBroken();
        }
    }

    /// <summary>SJY : 내구도가 모두 소모되면 파손 상태로 전환하고 이벤트를 발생</summary>
    private void HandleLockpickBroken()
    {
        if (_isBroken)
        {
            return;
        }

        _isBroken = true;

        CPrint.Log("[LockpickDurability] 락픽 파손");
        OnLockpickBroken?.Invoke();
    }
}
