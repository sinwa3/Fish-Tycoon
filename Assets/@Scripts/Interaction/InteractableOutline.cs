
using UnityEngine;

/// <summary>SJY : 상호작용 오브젝트의 아웃라인 표시 상태를 제어한다.</summary>
public class InteractableOutline : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    private bool _isOutlined;

    /// <summary>SJY : MeshRenderer와 Material을 초기화한다.</summary>
    private void Awake()
    {
        InitializeComponents();
        
        SetOutline(false);
    }

    /// <summary>SJY : 오브젝트 비활성화 시 아웃라인을 숨긴다.</summary>
    private void OnDisable()
    {
        SetOutline(false);
    }
    
    private void OnDestroy()
    {
        InteractionOutlineRendererFeature.SetRendererActive(_meshRenderer, false);
    }
    
    /// <summary>SJY : 현재 오브젝트의 MeshRenderer를 가져온다.</summary>
    private void InitializeComponents()
    {
        if (!gameObject.TryGetComponent(out  _meshRenderer))
        {
            CPrint.Error($"[InteractableOutline] {gameObject.name} has not MeshRenderer");
        }
    }

    /// <summary>SJY : 아웃라인을 켜거나 끈다.</summary>
    public void SetOutline(bool isEnabled)
    {
        if (_meshRenderer == null || _isOutlined == isEnabled)
        {
            return;
        }

        InteractionOutlineRendererFeature.SetRendererActive(_meshRenderer, isEnabled);
        _isOutlined = isEnabled;
    }
}
