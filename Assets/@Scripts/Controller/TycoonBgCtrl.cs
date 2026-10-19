using DG.Tweening;
using UnityEngine;

/// <summary> KDH : 타이쿤 씬의 배경과 배경 전환 연출을 관리한다. </summary>
public class TycoonBgCtrl : MonoBehaviour
{
    #region ===== 참조 =====

    [Header("페이즈별 배경")]
    [SerializeField]
    private Sprite _beforeOpeningBackground;

    [SerializeField]
    private Sprite _afterBackground;

    [SerializeField]
    private Sprite _openedBackground;

    private SpriteRenderer _backgroundA; // 상시 이미지
    private SpriteRenderer _backgroundB; // 페이드용 이미지

    #endregion ===== 참조 =====

    #region ===== 애니메이션 =====

    [Header("배경 전환 설정")]
    [SerializeField, Min(0.1f)]
    private float _fadeDuration = 0.5f;

    private Tween _fadeTween;

    #endregion ===== 애니메이션 =====

    private void Awake()
    {
        InitializeBackgrounds();

        if (TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            Destroy(spriteRenderer);
        }
    }

    private void OnDestroy()
    {
        _fadeTween?.Kill();
    }
    
    /// <summary> KDH : 배경 오브젝트를 생성하고 초기 배경을 설정한다. </summary>
    private void InitializeBackgrounds()
    {
        _backgroundA = CreateBackground("BackgroundA", 0);
        _backgroundB = CreateBackground("BackgroundB", 1);
    }

    /// <summary> KDH : 배경 오브젝트와 SpriteRenderer를 생성한다. </summary>
    private SpriteRenderer CreateBackground(string objectName, int sortingOrder)
    {
        GameObject background = new GameObject(objectName);
        background.transform.SetParent(transform, false);

        SpriteRenderer spriteRenderer = background.GetOrAddComponent<SpriteRenderer>();

        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = sortingOrder;

        return spriteRenderer;
    }

    #region ===== 배경 전환 =====

    /// <summary> KDH : 영업 준비 배경으로 전환한다. </summary>
    public void ShowBeforeOpening()
    {
        ChangeBackground(_beforeOpeningBackground);
    }

    /// <summary> KDH : 영업 중 배경으로 전환한다. </summary>
    public void ShowOpened()
    {
        ChangeBackground(_openedBackground);
    }

    /// <summary> KDH : 영업 종료 배경으로 전환한다. </summary>
    public void ShowAfter()
    {
        ChangeBackground(_afterBackground);
    }

    /// <summary> KDH : 지정한 배경으로 부드럽게 전환한다. </summary>
    private void ChangeBackground(Sprite nextSprite)
    {
        if (nextSprite == null || _backgroundA == null || _backgroundB == null)
        {
            return;
        }

        // 이미 표시 중인 배경이면 전환하지 않는다.
        if (_backgroundA.sprite == nextSprite)
        {
            return;
        }

        // 진행 중인 전환을 종료하고 B를 정리한다.
        _fadeTween?.Kill();
        _fadeTween = null;

        _backgroundB.DOKill();
        _backgroundB.sprite = null;
        _backgroundB.color = Color.white;
        _backgroundB.gameObject.SetActive(false);

        // 기존 배경을 B에 복사한다.
        if (_backgroundA.sprite != null)
        {
            _backgroundB.sprite = _backgroundA.sprite;
            _backgroundB.color = Color.white;
            _backgroundB.gameObject.SetActive(true);
        }
        else
        {
            // 최초 진입 시에는 검은 화면에서 시작한다.
            _backgroundB.sprite = nextSprite;
            _backgroundB.color = Color.black;
            _backgroundB.gameObject.SetActive(true);
        }

        // A에는 새로운 배경을 설정한다.
        _backgroundA.sprite = nextSprite;

        // B를 서서히 투명하게 만든다.
        _fadeTween = _backgroundB.DOFade(0f, _fadeDuration).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                _backgroundB.sprite = null;
                _backgroundB.color = Color.white;
                _backgroundB.gameObject.SetActive(false);

                _fadeTween = null;
            });
    }

    #endregion ===== 배경 전환 =====
}