using DG.Tweening;
using UnityEngine;

/// <summary> KDH : UI 이미지의 크기를 지정한 범위에서 반복적으로 변경한다. </summary>
[RequireComponent(typeof(RectTransform))]
public class UIScaleTween : MonoBehaviour
{
    #region ===== 설정 =====

    [SerializeField] private float _minScale = 0.9f;
    [SerializeField] private float _maxScale = 1.1f;
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private Ease _ease = Ease.InOutSine;

    #endregion ===== 설정 =====

    #region ===== 컴포넌트 =====

    private RectTransform _rectTransform;
    private Tween _scaleTween;

    #endregion ===== 컴포넌트 =====

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Play();
    }

    private void OnDisable()
    {
        _scaleTween?.Kill();
    }

    /// <summary> KDH : 최소 크기에서 최대 크기까지 반복하는 스케일 애니메이션을 실행한다. </summary>
    public void Play()
    {
        _scaleTween?.Kill();

        _rectTransform.localScale = Vector3.one * _minScale;

        _scaleTween = _rectTransform
            .DOScale(_maxScale, _duration)
            .SetEase(_ease)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary> KDH : 스케일 애니메이션을 중지하고 기본 크기로 복원한다. </summary>
    public void Stop()
    {
        _scaleTween?.Kill();

        _rectTransform.localScale = Vector3.one;
    }
}