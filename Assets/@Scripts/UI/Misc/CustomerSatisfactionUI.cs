using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 손님의 만족도 수치와 상태에 따른 UI 애니메이션을 관리한다. </summary>
public class CustomerSatisfactionUI : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [SerializeField] private float _countDuration = 0.6f;
    [SerializeField] private float _emojiDisplayDuration = 0.5f;

    #region ===== 참조 =====

    private TMP_Text _valueText;
    private Image _bg;

    private RectTransform _rect;

    #endregion ===== 참조 =====

    #region ===== 애니메이션 =====

    private Tween _countTween;

    #endregion ===== 애니메이션 =====

    private void Awake()
    {
        _valueText = GetComponentInChildren<TMP_Text>();
        if (_valueText == null)
        {
            CPrint.Warning("[CustomerSatisfactionUI] ValueText가 설정되지 않았습니다.");
            return;
        }

        _bg = GetComponent<Image>();
        if (_bg != null)
        {
            _bg.enabled = false;
        }

        _rect = transform as RectTransform;
        if (_rect == null)
        {
            CPrint.Warning("[CustomerSatisfactionUI] RectTransform이 Null 입니다.");
            return;
        }

        // 플레이 전에는 만족도 UI를 표시하지 않는다.
        _valueText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _countTween?.Kill();
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

        // 배경 활성화
        if (_bg != null)
        {
            _bg.enabled = true;
        }

        // 상태에 따른 이모지와 색상 설정
        string spriteIndex = GetSpriteIndex(state);
        _valueText.color = GetSatisfactionColor(state);

        // UI 표시 및 초기화
        _valueText.gameObject.SetActive(true);
        _countTween = DOTween.Sequence()
            // 1. 감정 이모지 먼저 표시
            .AppendCallback(() => { _valueText.SetText($"<size=6><sprite={spriteIndex}></size>"); })
            // 2. 이모지를 잠시 보여주기
            .AppendInterval(_emojiDisplayDuration)
            // 3. 만족도 숫자 증가 애니메이션
            .Append(DOVirtual
                .Int(0, satisfaction, _countDuration,
                    value => { _valueText.SetText($"만족도(+{value})"); }).SetEase(Ease.OutCubic))
            .AppendCallback(() =>
            {
                _valueText.gameObject.SetActive(false);
                if (_bg != null)
                {
                    _bg.enabled = false;
                }
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
                return ColorKey.LegendaryColor;

            case SatisfactionGrade.Normal:
                return ColorKey.RareColor;

            case SatisfactionGrade.Unhappy:
                return ColorKey.CommonColor;

            default:
                return Color.white;
        }
    }
}