using UnityEngine;

/// <summary> KYH : 결제할 때 만족 단계를 보여주는 손님 머리 위 임시 말풍선 (만족도 +N 표시가 들어오면 교체) </summary>
public class CustomerReactionBubble : MonoBehaviour
{
    // 주문 말풍선처럼 렌더러만 켜고 끈다
    private SpriteRenderer _bubbleRenderer;

    private void Awake()
    {
        InitializeComponents();
    }

    /// <summary> KYH : 같은 오브젝트의 SpriteRenderer를 찾아 둔다 </summary>
    private void InitializeComponents()
    {
        _bubbleRenderer = GetComponent<SpriteRenderer>();

        if (_bubbleRenderer == null)
        {
            CPrint.Warning("[CustomerReactionBubble] 스프라이트 렌더러 없음");
        }
    }

    /// <summary> KYH : 만족 단계에 맞는 모습으로 말풍선을 켠다 </summary>
    public void Show(SatisfactionGrade grade)
    {
        if (_bubbleRenderer == null)
        {
            return;
        }

        _bubbleRenderer.color = GetGradeColor(grade);
        _bubbleRenderer.enabled = true;
    }

    /// <summary> KYH : 말풍선을 끈다 </summary>
    public void Hide()
    {
        if (_bubbleRenderer == null)
        {
            return;
        }

        _bubbleRenderer.enabled = false;
    }

    /// <summary> KYH : 만족 단계별 임시 색을 고른다 </summary>
    private Color GetGradeColor(SatisfactionGrade grade)
    {
        Color color = grade switch
        {
            SatisfactionGrade.Unhappy => Color.red,
            SatisfactionGrade.Normal => Color.yellow,
            SatisfactionGrade.Happy => Color.green,
            _ => Color.white
        };

        return color;
    }
}
