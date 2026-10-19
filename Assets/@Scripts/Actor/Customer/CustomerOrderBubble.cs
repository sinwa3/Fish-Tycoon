using System.Collections.Generic;
using UnityEngine;

/// <summary> KYH : 주문한 메뉴를 보여주는 손님 머리 위 말풍선 </summary>
public class CustomerOrderBubble : MonoBehaviour
{
    // 렌더러만 켜고 끄고
    private SpriteRenderer _iconRenderer;

    private void Awake()
    {
        InitializeComponents();
    }

    /// <summary> KYH : 같은 오브젝트의 SpriteRenderer를 찾아 둔다 </summary>
    private void InitializeComponents()
    {
        _iconRenderer = GetComponent<SpriteRenderer>();

        if (_iconRenderer == null)
        {
            CPrint.Warning("[CustomerOrderBubble] 아이콘 렌더러 없음");
        }
    }

    /// <summary> KYH : 메뉴 아이콘을 넣고 말풍선을 켠다 </summary>
    public void Show(int menuId)
    {
        if (_iconRenderer == null)
        {
            return;
        }

        Sprite icon = GetMenuIcon(menuId);

        if (icon == null)
        {
            CPrint.Warning($"[CustomerOrderBubble] 메뉴 ID {menuId} 아이콘 없음");

            return;
        }

        _iconRenderer.sprite = icon;
        _iconRenderer.enabled = true;
    }

    /// <summary> KYH : 말풍선을 끈다 </summary>
    public void Hide()
    {
        if (_iconRenderer == null)
        {
            return;
        }

        _iconRenderer.enabled = false;
    }

    /// <summary> KYH : 음식 테이블의 리소스명으로 초밥 시트에서 메뉴 아이콘을 찾는다. 없으면 null </summary>
    private Sprite GetMenuIcon(int menuId)
    {
        if (!Managers.Table.IsLoaded)
        {
            return null;
        }

        IReadOnlyDictionary<int, FoodData> foodMap = Managers.Table.Food.Map;

        if (!foodMap.TryGetValue(menuId, out FoodData foodData))
        {
            return null;
        }

        return Managers.Resource.LoadSprite(ResourceKey.Path.FoodSprite, foodData.ResourceName);
    }
}
