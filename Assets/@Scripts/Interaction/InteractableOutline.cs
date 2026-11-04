using UnityEngine;

/// <summary>SJY : 상호작용 오브젝트의 OutlineMaterial을 자동으로 찾아 표시 상태를 제어</summary>
public class InteractableOutline : MonoBehaviour
{
    private const string OutlineMaterialName = "OutlineMaterial";

    private MeshRenderer[] _targetRenderers;
    private Material[][] _normalMaterials;
    private Material[][] _outlineMaterials;
    private bool _isOutlined;

    private void Awake()
    {
        InitializeComponents();
        InitializeMaterials();
        //SetOutline(false);
        ApplyOutline(false);
    }

    /// <summary>SJY : 자신과 자식 오브젝트의 MeshRenderer를 자동으로 검색</summary>
    private void InitializeComponents()
    {
        _targetRenderers = GetComponentsInChildren<MeshRenderer>(true);
    }

    /// <summary>SJY : 마지막 Outline Material을 분리하여 저장</summary>
    private void InitializeMaterials()
    {
        _normalMaterials = new Material[_targetRenderers.Length][];
        _outlineMaterials = new Material[_targetRenderers.Length][];

        for (int i = 0; i < _targetRenderers.Length; i++)
        {
            Material[] materials = _targetRenderers[i].sharedMaterials;

            if (materials.Length < 2)
            {
                continue;
            }

            Material lastMaterial = materials[materials.Length - 1];

            if (lastMaterial == null || !lastMaterial.name.StartsWith(OutlineMaterialName))
            {
                continue;
            }

            _outlineMaterials[i] = materials;
            _normalMaterials[i] = new Material[materials.Length - 1];

            for (int j = 0; j < _normalMaterials[i].Length; j++)
            {
                _normalMaterials[i][j] = materials[j];
            }
        }
    }

    /// <summary>SJY : 외부에서 아웃라인 활성화 여부를 지정</summary>
    public void SetOutline(bool isEnabled)
    {
        if (_isOutlined == isEnabled)
        {
            return;
        }

        ApplyOutline(isEnabled);
    }

    /// <summary>SJY : 저장된 Material 배열을 적용하여 아웃라인을 켜거나 끔</summary>
    private void ApplyOutline(bool isEnabled)
    {
        if (_targetRenderers == null || _normalMaterials == null)
        {
            return;
        }

        for (int i = 0; i < _targetRenderers.Length; i++)
        {
            if (_targetRenderers[i] == null || _normalMaterials[i] == null)
            {
                continue;
            }

            _targetRenderers[i].sharedMaterials = isEnabled ? _outlineMaterials[i] : _normalMaterials[i];
        }

        _isOutlined = isEnabled;
    }

    /// <summary>SJY :  오브젝트 비활성화 시 아웃라인을 숨김</summary>
    private void OnDisable()
    {
        if (_targetRenderers != null)
        {
            ApplyOutline(false);
        }
    }
}
