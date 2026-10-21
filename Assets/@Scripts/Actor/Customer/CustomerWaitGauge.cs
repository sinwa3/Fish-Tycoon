using UnityEngine;

/// <summary> KYH : 남은 대기시간을 보여주는 손님 머리 위 게이지 </summary>
public class CustomerWaitGauge : MonoBehaviour
{
    // 자식 오브젝트 이름 (프리팹 자식 이름과 같아야함)
    private const string BackgroundName = "Background";
    private const string FillName = "Fill";

    // 렌더러만 켜고 끈다
    private SpriteRenderer _backgroundRenderer;
    private SpriteRenderer _fillRenderer;

    // 비율 1일 때 크기
    private Vector3 _fillFullScale;

    private void Awake()
    {
        InitializeComponents();
    }

    /// <summary> KYH : 자식의 배경, 채움 렌더러를 찾고 채움 막대의 처음 크기를 기억한다 </summary>
    private void InitializeComponents()
    {
        _backgroundRenderer = gameObject.FindChild<SpriteRenderer>(BackgroundName);
        _fillRenderer = gameObject.FindChild<SpriteRenderer>(FillName);

        if (_fillRenderer == null)
        {
            return;
        }

        _fillFullScale = _fillRenderer.transform.localScale;
    }

    /// <summary> KYH : 게이지를 가득 찬 상태로 켠다 </summary>
    public void Show()
    {
        if (_fillRenderer == null || _backgroundRenderer == null)
        {
            return;
        }

        _fillRenderer.enabled = true;
        _backgroundRenderer.enabled = true;

        SetRatio(1.0f);
    }

    /// <summary> KYH : 게이지를 끈다 </summary>
    public void Hide()
    {
        if (_fillRenderer == null || _backgroundRenderer == null)
        {
            return;
        }

        _fillRenderer.enabled = false;
        _backgroundRenderer.enabled = false;
    }

    /// <summary> KYH : 남은 대기시간 비율만큼 게이지 가로 길이를 줄인다 </summary>
    public void SetRatio(float ratio)
    {
        if (_fillRenderer == null)
        {
            return;
        }

        Vector3 fillScale = _fillFullScale;
        fillScale.x *= ratio;

        _fillRenderer.transform.localScale = fillScale;
    }
}
