using UnityEngine;

/// <summary> GYH : 손님이 받은 요리를 손님 앞에 보여준다 </summary>
public class CustomerServedFood : MonoBehaviour
{
    // 요리 렌더러
    private SpriteRenderer _foodRenderer;
    // 접시 렌더러
    private SpriteRenderer _plateRenderer;

    private void Awake()
    {
        InitializeComponents();
    }

    /// <summary> GYH : 렌더러를 찾아 둔다 </summary>
    private void InitializeComponents()
    {
        _foodRenderer = GetComponent<SpriteRenderer>();
        _plateRenderer = transform.Find("Plate")?.GetComponent<SpriteRenderer>();

        if (_foodRenderer == null)
        {
            CPrint.Warning("[CustomerServedFood] 요리 렌더러 없음");
        }

        if (_plateRenderer == null)
        {
            CPrint.Warning("[CustomerServedFood] 접시 렌더러 없음");
        }
    }

    /// <summary> GYH : 메뉴 그림을 넣고 요리를 켠다 </summary>
    public void Show(int menuId)
    {
        if (_foodRenderer == null)
        {
            return;
        }

        if (_plateRenderer == null)
        {
            return;
        }

        if (!Managers.Table.IsLoaded)
        {
            CPrint.Warning("[CustomerServedFood] 테이블이 아직 로드되지 않음");

            return;
        }

        Sprite icon = Managers.Table.Food.GetSpriteByFoodID(menuId);

        if (icon == null)
        {
            CPrint.Warning($"[CustomerServedFood] 메뉴 ID {menuId} 아이콘 없음");

            return;
        }

        _foodRenderer.sprite = icon;
        _foodRenderer.enabled = true;

        _plateRenderer.enabled = true;
    }

    /// <summary> GYH : 요리를 끈다 </summary>
    public void Hide()
    {
        if (_foodRenderer == null)
        {
            return;
        }

        if (_plateRenderer == null)
        {
            return;
        }

        _foodRenderer.enabled = false;
        _plateRenderer.enabled = false;
    }
}
