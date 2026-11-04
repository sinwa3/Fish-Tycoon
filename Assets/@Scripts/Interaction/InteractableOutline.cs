
using UnityEngine;

/// <summary>SJY : 상호작용 오브젝트에 등록된 OutlineMaterial을 자동으로 찾아 표시 상태를 제어한다.</summary>
public class InteractableOutline : MonoBehaviour
{
    private const string OutlineMaterialName = "OutlineMaterial";

    private MeshRenderer _meshRenderer;
    private Material[] _normalMaterials;
    private Material[] _outlineMaterials;
    private bool _hasOutlineMaterial;
    private bool _isOutlined;

    /// <summary>SJY : MeshRenderer와 Material을 초기화한다.</summary>
    private void Awake()
    {
        InitializeComponents();
        InitializeMaterials();
        ApplyOutline(false);
    }

    /// <summary>SJY : 현재 오브젝트의 MeshRenderer를 가져온다.</summary>
    private void InitializeComponents()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    /// <summary>SJY : 마지막 슬롯에 등록된 OutlineMaterial을 찾아 저장한다.</summary>
    private void InitializeMaterials()
    {
        Material[] materials = _meshRenderer.sharedMaterials;

        if (materials.Length < 2)
        {
            return;
        }

        Material lastMaterial = materials[materials.Length - 1];

        if (lastMaterial == null || !lastMaterial.name.StartsWith(OutlineMaterialName))
        {
            return;
        }

        _outlineMaterials = materials;
        _normalMaterials = new Material[materials.Length - 1];

        for (int i = 0; i < _normalMaterials.Length; i++)
        {
            _normalMaterials[i] = materials[i];
        }

        _hasOutlineMaterial = true;
    }

    /// <summary>SJY : 아웃라인을 켜거나 끈다.</summary>
    public void SetOutline(bool isEnabled)
    {
        if (!_hasOutlineMaterial || _isOutlined == isEnabled)
        {
            return;
        }

        ApplyOutline(isEnabled);
    }

    /// <summary>SJY : OutlineMaterial을 MeshRenderer에 추가하거나 제거한다.</summary>
    private void ApplyOutline(bool isEnabled)
    {
        if (!_hasOutlineMaterial || _meshRenderer == null)
        {
            return;
        }

        _meshRenderer.sharedMaterials = isEnabled ? _outlineMaterials : _normalMaterials;
        _isOutlined = isEnabled;
    }

    /// <summary>SJY : 오브젝트 비활성화 시 아웃라인을 숨긴다.</summary>
    private void OnDisable()
    {
        ApplyOutline(false);
    }
}
