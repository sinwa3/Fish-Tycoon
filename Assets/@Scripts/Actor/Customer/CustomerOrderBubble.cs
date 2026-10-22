using UnityEngine;

/// <summary> KYH : 주문한 메뉴를 보여주는 손님 머리 위 말풍선 </summary>
public class CustomerOrderBubble : MonoBehaviour
{
    // 렌더러만 켜고 끄고
    private SpriteRenderer _iconRenderer;
    // SJY : 말풍선 배경 렌더러
    private SpriteRenderer _backgroundRenderer;

    private void Awake()
    {
        InitializeComponents();
    }

    /// <summary> KYH : 같은 오브젝트의 SpriteRenderer를 찾아 둔다 </summary>
    private void InitializeComponents()
    {
        _iconRenderer = GetComponent<SpriteRenderer>();
        // SJY : 말풍선 렌더러
        _backgroundRenderer = transform.Find("BubbleBackground")?.GetComponent<SpriteRenderer>();

        if (_iconRenderer == null)
        {
            CPrint.Warning("[CustomerOrderBubble] 아이콘 렌더러 없음");
        }

        if (_backgroundRenderer == null)
        {
            CPrint.Warning("[CustomerOrderBubble] 말풍선 배경 렌더러 없음");
        }
    }

    /// <summary> KYH : 메뉴 아이콘을 넣고 말풍선을 켠다 </summary>
    public void Show(int menuId)
    {
        if (_iconRenderer == null)
        {
            return;
        }

        if (!Managers.Table.IsLoaded)
        {
            CPrint.Warning("[CustomerOrderBubble] 테이블이 아직 로드되지 않음");

            return;
        }

        Sprite icon = Managers.Table.Food.GetSpriteByFoodID(menuId);

        if (icon == null)
        {
            CPrint.Warning($"[CustomerOrderBubble] 메뉴 ID {menuId} 아이콘 없음");

            return;
        }

        _iconRenderer.sprite = icon;
        _iconRenderer.enabled = true;

        // SJY : 음식 이미지와 함께 말풍선 배경 활성화
        if (_backgroundRenderer != null)
        {
            _backgroundRenderer.enabled = true;
        }
    }

    /// <summary> KYH : 말풍선을 끈다 </summary>
    public void Hide()
    {
        if (_iconRenderer == null)
        {
            return;
        }

        _iconRenderer.enabled = false;

        // SJY : 음식 이미지와 함께 말풍선 배경 비활성화
        if (_backgroundRenderer != null)
        {
            _backgroundRenderer.enabled = false;
        }
    }
}
