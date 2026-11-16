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
    [SerializeField] private Vector2 _offset = new (20f, 0f);

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
    
    private RectTransform _trigger;
    private readonly Vector3[] _corners = new Vector3[4];

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

    private void LateUpdate()
    {
        if (_trigger != null && _trigger.gameObject.activeInHierarchy)
        {
            SetPosition(_trigger);
        }
    }
    
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

        _trigger = trigger;
        gameObject.SetActive(true);
        SetPosition(trigger);

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
        RectTransform parent = _rect.parent as RectTransform;
        if (parent == null)
        {
            return;
        }

        trigger.GetWorldCorners(_corners);

        Vector3 bottomLeft = _corners[0];
        Vector3 topLeft = _corners[1];
        Vector3 topRight = _corners[2];
        Vector3 bottomRight = _corners[3];

        Vector3 targetPosition;

        switch (_direction)
        {
            case TooltipDirection.Left:
                targetPosition = (bottomLeft + topLeft) * 0.5f;
                break;

            case TooltipDirection.Right:
                targetPosition = (bottomRight + topRight) * 0.5f;
                break;

            case TooltipDirection.Top:
                targetPosition = (topLeft + topRight) * 0.5f;
                break;

            case TooltipDirection.Bottom:
                targetPosition = (bottomLeft + bottomRight) * 0.5f;
                break;

            default:
                return;
        }

        // targetPosition이라는 월드 좌표를 parent의 로컬 좌표계로 변환한다.
        Vector3 localPosition = parent.InverseTransformPoint(targetPosition);
        _rect.localPosition = new Vector3(localPosition.x + _offset.x, localPosition.y + _offset.y, _rect.localPosition.z);
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
                _trigger = null;
                _rect.localScale = _defaultScale;
            });
    }
}
