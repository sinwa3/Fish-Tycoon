using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// YSW : 숨어 있는 동안 화면 양옆을 옷장 문짝처럼 어둡게 가리고 가운데 틈으로만 밖이 보이게 하는 가림막.
/// 캔버스와 가림막 이미지는 실행 중에 직접 만든다. (다른 UI보다 뒤에 그려 안내 문구는 가리지 않는다)
/// </summary>
public class PlayerHidingOverlay : MonoBehaviour
{
    #region ===== 설정 =====

    private const int OverlaySortingOrder = -100;
    private const int TextureWidth = 512;
    private const int TextureHeight = 256;

    [Header("문틈")]
    [Tooltip("가운데 틈의 너비 (화면 너비 대비 비율)")]
    [SerializeField, Range(0.05f, 0.9f)] private float _gapWidth = 0.42f;

    [Tooltip("틈 가장자리가 어두워지는 구간의 너비 (화면 너비 대비 비율). 클수록 부드럽게 어두워진다.")]
    [SerializeField, Range(0.01f, 0.5f)] private float _gapSoftness = 0.14f;

    [Tooltip("문짝 부분의 어두운 정도 (1이면 완전히 가린다)")]
    [SerializeField, Range(0f, 1f)] private float _doorAlpha = 0.96f;

    [Header("위아래 그늘")]
    [Tooltip("화면 위아래가 어두워지기 시작하는 위치 (가운데에서 떨어진 정도, 0 ~ 0.5)")]
    [SerializeField, Range(0f, 0.5f)] private float _shadeStart = 0.22f;

    [Tooltip("화면 위아래 끝의 어두운 정도")]
    [SerializeField, Range(0f, 1f)] private float _shadeAlpha = 0.75f;

    [SerializeField] private Color _overlayColor = new Color(0.02f, 0.015f, 0.01f, 1f);

    [Tooltip("가림막을 그릴 거리 (카메라 Near Clip보다 조금 멀게)")]
    [SerializeField, Min(0.01f)] private float _planeDistance = 0.08f;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private Canvas _overlayCanvas;
    private RawImage _overlayImage;
    private Texture2D _overlayTexture;

    /// <summary> YSW : 가림막이 보이는 중인지 여부 </summary>
    public bool IsShowing => _overlayCanvas != null && _overlayCanvas.enabled;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeOverlayCanvas();
    }

    private void OnDisable()
    {
        Hide();
    }

    private void OnDestroy()
    {
        if (_overlayTexture != null)
        {
            Destroy(_overlayTexture);
        }
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 가림막을 그릴 캔버스와 이미지를 만든다. 처음에는 꺼둔다. </summary>
    private void InitializeOverlayCanvas()
    {
        GameObject canvasObject = new GameObject("HidingOverlayCanvas");
        canvasObject.transform.SetParent(transform, false);

        _overlayCanvas = canvasObject.AddComponent<Canvas>();
        _overlayCanvas.sortingOrder = OverlaySortingOrder;
        _overlayCanvas.enabled = false;

        GameObject imageObject = new GameObject("HidingOverlayImage");
        imageObject.transform.SetParent(canvasObject.transform, false);

        _overlayImage = imageObject.AddComponent<RawImage>();
        _overlayImage.raycastTarget = false;
        _overlayImage.texture = CreateOverlayTexture();

        RectTransform imageRect = _overlayImage.rectTransform;
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
    }

    /// <summary> YSW : 양옆은 문짝처럼 어둡고 가운데만 트인, 위아래로 그늘이 진 가림막 텍스처를 만든다. </summary>
    private Texture2D CreateOverlayTexture()
    {
        _overlayTexture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
        _overlayTexture.wrapMode = TextureWrapMode.Clamp;
        _overlayTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[TextureWidth * TextureHeight];
        float gapHalf = _gapWidth * 0.5f;

        for (int y = 0; y < TextureHeight; y++)
        {
            float fromCenterY = Mathf.Abs((y + 0.5f) / TextureHeight - 0.5f);
            float shade = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(_shadeStart, 0.5f, fromCenterY)) * _shadeAlpha;

            for (int x = 0; x < TextureWidth; x++)
            {
                float fromCenterX = Mathf.Abs((x + 0.5f) / TextureWidth - 0.5f);
                float door = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(gapHalf, gapHalf + _gapSoftness, fromCenterX)) * _doorAlpha;

                // 두 가림을 겹친다. (어느 한쪽이라도 어두우면 어둡게)
                float alpha = 1f - (1f - door) * (1f - shade);

                Color pixel = _overlayColor;
                pixel.a = alpha;
                pixels[y * TextureWidth + x] = pixel;
            }
        }

        _overlayTexture.SetPixels(pixels);
        _overlayTexture.Apply();

        return _overlayTexture;
    }

    #endregion ===== 초기화 =====

    #region ===== 표시 =====

    /// <summary> YSW : 지정한 카메라 바로 앞에 가림막을 띄운다. (카메라가 없으면 화면 맨 위에 덮는다) </summary>
    public void Show(Camera viewCamera)
    {
        if (_overlayCanvas == null)
        {
            return;
        }

        if (viewCamera != null)
        {
            _overlayCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            _overlayCanvas.worldCamera = viewCamera;
            _overlayCanvas.planeDistance = Mathf.Max(_planeDistance, viewCamera.nearClipPlane + 0.01f);
        }
        else
        {
            _overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        _overlayCanvas.enabled = true;
    }

    /// <summary> YSW : 가림막을 숨긴다. </summary>
    public void Hide()
    {
        if (_overlayCanvas == null)
        {
            return;
        }

        _overlayCanvas.enabled = false;
    }

    #endregion ===== 표시 =====
}
