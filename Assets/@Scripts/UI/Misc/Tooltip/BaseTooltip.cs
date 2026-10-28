using DG.Tweening;
using UnityEngine;

/// <summary> KDH : 툴팁의 위치와 애니메이션을 관리한다. </summary>
public abstract class BaseTooltip : BaseUI
{
    public enum TooltipDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }

    [Header("위치 설정")]
    [SerializeField] private TooltipDirection _direction = TooltipDirection.Right;
    [SerializeField] private Vector2 _offset = new Vector2(20f, 0f);

    [Header("애니메이션")]
    [SerializeField] private float _showDuration = 0.2f;
    [SerializeField] private float _hideDuration = 0.2f;
    [SerializeField] private float _startScaleX = 0.85f;
    [SerializeField] private Ease _showEase = Ease.OutBack;
    [SerializeField] private Ease _hideEase = Ease.InQuad;

    private Tween _tween;
    private Vector3 _defaultScale;

    #region ===== 참조 =====

    private RectTransform _rect;
    private CanvasGroup _canvasGroup;

    #endregion ===== 참조 =====

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        _canvasGroup.blocksRaycasts = false;
        
        _defaultScale = _rect.localScale;

        _rect.pivot = _direction switch
        {
            TooltipDirection.Right => new Vector2(0f, 0.5f),
            TooltipDirection.Left => new Vector2(1f, 0.5f),
            TooltipDirection.Top => new Vector2(0.5f, 0f),
            TooltipDirection.Bottom => new Vector2(0.5f, 1f),
        };

        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);

        OnAwake();
    }

    protected abstract void OnAwake();

    protected virtual void OnDestroy()
    {
        _tween?.Kill();
    }

    /// <summary> KDH : 툴팁의 데이터를 갱신하고 표시한다. </summary>
    public void Show(RectTransform trigger)
    {
        if (trigger == null)
        {
            return;
        }

        _tween?.Kill();

        // 파생 클래스에서 툴팁 내용을 갱신한다.
        UpdateContent();

        // 슬롯을 기준으로 위치를 계산한다.
        SetPosition(trigger);

        gameObject.SetActive(true);

        _rect.localScale = new Vector3(_defaultScale.x * _startScaleX, _defaultScale.y, _defaultScale.z);

        _canvasGroup.alpha = 0f;

        _tween = DOTween.Sequence().Join(_rect.DOScale(_defaultScale, _showDuration).SetEase(_showEase))
            .Join(_canvasGroup.DOFade(1f, _showDuration).SetEase(Ease.InCirc));
    }

    /// <summary> KDH : 파생 클래스에서 툴팁 내용을 갱신한다. </summary>
    protected abstract void UpdateContent();

    /// <summary> KDH : 슬롯 위치를 기준으로 툴팁의 위치를 설정한다. </summary>
    private void SetPosition(RectTransform trigger)
    {
        Vector3[] corners = new Vector3[4];
        trigger.GetWorldCorners(corners);

        Vector3 bottomLeft = corners[0];
        Vector3 topLeft = corners[1];
        Vector3 topRight = corners[2];
        Vector3 bottomRight = corners[3];

        Vector3 targetPosition;

        switch (_direction)
        {
            case TooltipDirection.Left:
                targetPosition = new Vector3(bottomLeft.x, (bottomLeft.y + topLeft.y) * 0.5f, bottomLeft.z);
                break;

            case TooltipDirection.Right:
                targetPosition = new Vector3(bottomRight.x, (bottomRight.y + topRight.y) * 0.5f, bottomRight.z);
                break;

            case TooltipDirection.Top:
                targetPosition = new Vector3((topLeft.x + topRight.x) * 0.5f, topLeft.y, topLeft.z);
                break;

            case TooltipDirection.Bottom:
                targetPosition = new Vector3((bottomLeft.x + bottomRight.x) * 0.5f, bottomLeft.y, bottomLeft.z);
                break;

            default:
                return;
        }

        targetPosition += new Vector3(_offset.x, _offset.y, 0f);

        _rect.position = targetPosition;
    }

    /// <summary> KDH : 툴팁을 애니메이션과 함께 숨긴다. </summary>
    public void Hide()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        _tween?.Kill();

        Vector3 targetScale = new Vector3(_defaultScale.x * _startScaleX, _defaultScale.y, _defaultScale.z);

        _tween = DOTween.Sequence().Join(_rect.DOScale(targetScale, _hideDuration).SetEase(_hideEase))
            .Join(_canvasGroup.DOFade(0f, _hideDuration))
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                _rect.localScale = _defaultScale;
            });
    }
}