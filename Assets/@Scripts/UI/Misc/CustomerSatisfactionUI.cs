using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary> KDH : 손님의 만족도 수치와 상태에 따른 UI 애니메이션을 관리한다. </summary>
public class CustomerSatisfactionUI : MonoBehaviour
{
    [Header("만족도 색상")]
    [SerializeField] private Color _perfectColor = Color.yellow;
    [SerializeField] private Color _goodColor = Color.black;
    [SerializeField] private Color _badColor = Color.red;

    [Header("애니메이션 설정")]
    [SerializeField] private float _countDuration = 0.6f;
    [SerializeField] private float _scaleDuration = 0.2f;
    [SerializeField] private float _scaleMultiplier = 1.3f;
    [SerializeField] private float _displayDuration = 1.5f;

    #region ===== 참조 =====
    
    private TMP_Text _valueText;
    private RectTransform _rect;
    
    #endregion ===== 참조 =====
    
    #region ===== 애니메이션 =====

    private Tween _countTween;
    private Sequence _animationSequence;

    private Vector3 _originScale;

    #endregion ===== 애니메이션 =====

    private void Awake()
    {
        _valueText = GetComponentInChildren<TMP_Text>();
        
        if (_valueText == null)
        {
            CPrint.Warning("[CustomerSatisfactionUI] ValueText가 설정되지 않았습니다.");
            return;
        }

        _rect = transform as RectTransform;

        if (_rect == null)
        {
            CPrint.Warning("[CustomerSatisfactionUI] RectTransform이 Null 입니다.");
            return;
        }
        _originScale = _rect.localScale;

        // 플레이 전에는 만족도 UI를 표시하지 않는다.
        _valueText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _countTween?.Kill();
        _animationSequence?.Kill();
    }

    /// <summary> KDH : 만족도 수치와 상태에 따라 UI를 표시하고 애니메이션을 재생한다. </summary>
    public void Play(int satisfaction, SatisfactionGrade state)
    {
        if (_valueText == null || _rect == null)
        {
            CPrint.Warning("[CustomerSatisfactionUI] 참조가 설정되지 않았습니다.");
            return;
        }

        // 기존 애니메이션 종료
        _countTween?.Kill();
        _animationSequence?.Kill();

        // 상태에 따른 이모지와 색상 설정
        string spriteIndex = GetSpriteIndex(state);
        _valueText.color = GetSatisfactionColor(state);

        // UI 표시 및 초기화
        _valueText.gameObject.SetActive(true);
        _valueText.SetText($"0↑ <sprite={spriteIndex}>");
        _rect.localScale = _originScale;

        // 만족도 숫자 증가 애니메이션
        _countTween = DOVirtual.Int(0, satisfaction, _countDuration, 
                value => _valueText.SetText($"{value}↑ <sprite={spriteIndex}>")).SetEase(Ease.OutCubic);

        // 크기 강조 후 원래 크기로 복귀하고 UI 숨기기
        _animationSequence = DOTween.Sequence()
            .Append(_rect.DOScale(_originScale * _scaleMultiplier, _scaleDuration).SetEase(Ease.OutBack))
            .Append(_rect.DOScale(_originScale, _scaleDuration).SetEase(Ease.OutQuad))
            .AppendInterval(_displayDuration)
            .AppendCallback(() =>
            {
                _valueText.gameObject.SetActive(false);
            });
    }

    /// <summary> KDH : 만족도 상태에 대응하는 TMP Sprite 인덱스를 반환한다. </summary>
    private string GetSpriteIndex(SatisfactionGrade state)
    {
        switch (state)
        {
            case SatisfactionGrade.Happy:
                return "1";

            case SatisfactionGrade.Normal:
                return "14";

            case SatisfactionGrade.Unhappy:
                return "15";

            default:
                return "14";
        }
    }

    /// <summary> KDH : 만족도 상태에 대응하는 텍스트 색상을 반환한다. </summary>
    private Color GetSatisfactionColor(SatisfactionGrade state)
    {
        switch (state)
        {
            case SatisfactionGrade.Happy:
                return _perfectColor;

            case SatisfactionGrade.Normal:
                return _goodColor;

            case SatisfactionGrade.Unhappy:
                return _badColor;

            default:
                return Color.white;
        }
    }
}