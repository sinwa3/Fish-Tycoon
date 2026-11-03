using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary> YSW : 화면 전체를 검게 덮었다가 걷어내는 전환 연출. 필요한 캔버스는 실행 중에 직접 만든다. </summary>
public class PlayerScreenFade : MonoBehaviour
{
    #region ===== 설정 =====

    private const int FadeSortingOrder = 1000;

    [Tooltip("화면이 완전히 어두워지기까지 걸리는 시간(초)")]
    [SerializeField] private float _fadeOutDuration = 0.2f;

    [Tooltip("어두워진 상태로 유지하는 시간(초)")]
    [SerializeField] private float _holdDuration = 0.05f;

    [Tooltip("화면이 다시 밝아지기까지 걸리는 시간(초)")]
    [SerializeField] private float _fadeInDuration = 0.2f;

    [SerializeField] private Color _fadeColor = Color.black;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private Canvas _fadeCanvas;
    private Image _fadeImage;
    private Coroutine _fadeRoutine;

    /// <summary> YSW : 전환 연출이 진행 중인지 여부 </summary>
    public bool IsFading => _fadeRoutine != null;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeFadeCanvas();
    }

    private void OnDisable()
    {
        ResetFade();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 화면을 덮을 오버레이 캔버스와 이미지를 만든다. </summary>
    private void InitializeFadeCanvas()
    {
        GameObject canvasObject = new GameObject("ScreenFadeCanvas");
        canvasObject.transform.SetParent(transform, false);

        _fadeCanvas = canvasObject.AddComponent<Canvas>();
        _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _fadeCanvas.sortingOrder = FadeSortingOrder;

        GameObject imageObject = new GameObject("ScreenFadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);

        _fadeImage = imageObject.AddComponent<Image>();
        _fadeImage.raycastTarget = false;

        RectTransform imageRect = _fadeImage.rectTransform;
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        SetFadeAlpha(0f);
    }

    #endregion ===== 초기화 =====

    #region ===== 전환 연출 =====

    /// <summary> YSW : 화면을 어둡게 한 뒤 onDark를 실행하고, 다시 밝힌 뒤 onFinished를 실행한다. 이미 진행 중이면 무시한다. </summary>
    public bool Play(Action onDark, Action onFinished = null)
    {
        if (IsFading)
        {
            return false;
        }

        _fadeRoutine = StartCoroutine(FadeRoutine(onDark, onFinished));

        return true;
    }

    /// <summary> YSW : 어두워짐 → 유지 → 밝아짐 순서로 진행한다. (일시정지 영향을 받지 않도록 실제 시간 기준) </summary>
    private IEnumerator FadeRoutine(Action onDark, Action onFinished)
    {
        yield return FadeTo(1f, _fadeOutDuration);

        onDark?.Invoke();

        if (_holdDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(_holdDuration);
        }

        yield return FadeTo(0f, _fadeInDuration);

        _fadeRoutine = null;

        onFinished?.Invoke();
    }

    /// <summary> YSW : 현재 투명도에서 목표 투명도까지 지정한 시간 동안 바꾼다. </summary>
    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = _fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetFadeAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));

            yield return null;
        }

        SetFadeAlpha(targetAlpha);
    }

    /// <summary> YSW : 진행 중인 연출을 멈추고 화면을 원래대로 밝힌다. </summary>
    private void ResetFade()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }

        SetFadeAlpha(0f);
    }

    /// <summary> YSW : 덮개 이미지의 투명도를 바꾸고, 완전히 투명하면 캔버스를 꺼서 그리지 않게 한다. </summary>
    private void SetFadeAlpha(float alpha)
    {
        if (_fadeImage == null)
        {
            return;
        }

        Color color = _fadeColor;
        color.a = alpha;
        _fadeImage.color = color;

        _fadeCanvas.enabled = alpha > 0f;
    }

    #endregion ===== 전환 연출 =====
}
