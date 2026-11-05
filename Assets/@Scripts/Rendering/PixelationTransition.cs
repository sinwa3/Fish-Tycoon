using System;
using DG.Tweening;
using UnityEngine;

/// <summary> KDH : 화면 전체 픽셀화 전환을 제어한다. 렌더링은 PixelationRendererFeature가 담당하며 전환 중일 때만 패스를 요청한다. </summary>
public class PixelationTransition : MonoBehaviour
{
    #region ===== 설정 =====

    [SerializeField, Min(0.01f)] private float _duration = 0.8f;
    [SerializeField, Min(2)] private int _startPixelSize = 64;
    [SerializeField] private Ease _ease = Ease.OutCubic;
    [SerializeField] private bool _useUnscaledTime = true;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private Tween _pixelationTween;
    private float _pixelSize = 1f;
    private bool _isPlaying;

    private static PixelationTransition _instance;
    public static bool IsRendering => _instance != null && _instance._isPlaying;
    public static float CurrentPixelSize => _instance != null ? _instance._pixelSize : 1f;

    #endregion ===== 상태 =====

    private void Awake()
    {
        _instance = this;
    }

    private void OnDisable()
    {
        Stop();

        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnDestroy()
    {
        Stop();

        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary> KDH : 등록된 전환 인스턴스를 기본 설정으로 재생한다. </summary>
    public void Play(Action onComplete = null)
    {
        _instance?.PlayTransition(onComplete);
    }

    /// <summary> KDH : 첫 화면이 선명하게 보이지 않도록 전환 시작 상태를 즉시 적용한다. </summary>
    private void Prepare()
    {
        _pixelationTween?.Kill(false);
        _pixelationTween = null;
        _pixelSize = _startPixelSize;
        _isPlaying = true;
    }

    /// <summary> KDH : Inspector 설정값으로 픽셀화가 원본 화면으로 돌아오는 전환을 재생한다. </summary>
    private void PlayTransition(Action onComplete = null)
    {
        Prepare();

        _pixelationTween = DOTween.To(() => _pixelSize, value => _pixelSize = value, 1f, _duration)
            .SetEase(_ease).SetUpdate(_useUnscaledTime).SetLink(gameObject).OnComplete(() =>
            {
                _pixelationTween = null;
                _pixelSize = 1f;
                _isPlaying = false;
                onComplete?.Invoke();
            });
    }

    /// <summary> KDH : 전환을 중단하고 Renderer Feature의 화면 패스를 즉시 비활성화한다. </summary>
    private void Stop()
    {
        _pixelationTween?.Kill();
        _pixelationTween = null;
        _pixelSize = 1f;
        _isPlaying = false;
    }
}
