using UnityEngine;

/// <summary>SJY : 핀의 이동에 맞춰 스프링의 Z축 위치와 크기를 변경</summary>
public class PinSpring : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform _pinMotion;

    [Header("스프링 설정")]
    [SerializeField] private float _springLength = 0.5f;
    [SerializeField, Range(0.01f, 1f)] private float _minScaleRatio = 0.2f;
    [SerializeField] private bool _isPositiveZTop = true;

    private Vector3 _initialPinPosition;
    private Vector3 _initialSpringPosition;
    private Vector3 _initialSpringScale;

    private void Start()
    {
        InitializeSpring();
    }

    /// <summary>SJY : 초기 Transform 정보 저장</summary>
    private void InitializeSpring()
    {
        if (_pinMotion == null)
        {
            Debug.LogWarning($"[PinSpring] {name} : PinMotion이 연결되지 않았습니다.", this);

            return;
        }

        _initialPinPosition = _pinMotion.localPosition;
        _initialSpringPosition = transform.localPosition;
        _initialSpringScale = transform.localScale;
    }

    /// <summary>SJY : 핀의 움직임이 처리된 후 스프링 갱신</summary>
    private void LateUpdate()
    {
        UpdateSpring();
    }

    /// <summary>SJY : 핀 이동거리에 따라 스프링을 압축하거나 복원</summary>
    private void UpdateSpring()
    {
        if (_pinMotion == null || _springLength <= 0f)
        {
            return;
        }

        float movedDistance = _initialPinPosition.y - _pinMotion.localPosition.y;
        float compressedDistance = Mathf.Clamp(movedDistance, 0f, _springLength * (1f - _minScaleRatio));
        float scaleRatio = 1f - compressedDistance / _springLength;

        Vector3 springScale = _initialSpringScale;
        springScale.z = _initialSpringScale.z * scaleRatio;
        transform.localScale = springScale;

        Vector3 springPosition = _initialSpringPosition;
        springPosition.y -= compressedDistance * 0.5f;
        transform.localPosition = springPosition;
    }
}
