using JetBrains.Annotations;
using UnityEngine;

/// <summary>SJY : 상호작용 오브젝트의 Material을 교체하여 아웃라인을 표시한다.</summary>
public class InteractableOutline : MonoBehaviour
{
    [SerializeField] private Material _outlineMaterial;
    [SerializeField] private bool _showOutline;

    private Renderer[] _renderers;
    private Material[][] _originalMaterials;
    private Material[][] _outlineMaterials;

    private bool _isInitialized;
    private bool _isOutlineApplied;
    private bool _isDirty;

    private void Awake()
    {
        InitializeComponents();
        InitializeMaterials();
        ApplyOutline(_showOutline);
    }

    /// <summary>SJY : 자신과 자식의 Renderer를 자동으로 검색</summary>
    private void InitializeComponents()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    /// <summary>SJY : 원본 Material을 저장하고 교체용 Material 배열을 생성</summary>
    private void InitializeMaterials()
    {
        _isInitialized = false;
        if (_renderers == null || _outlineMaterial == null)
        {
            return;
        }

        _originalMaterials = new Material[_renderers.Length][];
        _outlineMaterials = new Material[_renderers.Length][];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalMaterials[i] = _renderers[i].sharedMaterials;
            _outlineMaterials[i] = new Material[_originalMaterials[i].Length];

            for (int j = 0; j < _outlineMaterials[i].Length; j++)
            {
                _outlineMaterials[i][j] = _outlineMaterial;
            }
        }

        _isInitialized = true;
        _isOutlineApplied = false;
    }

    private void Update()
    {
        if (!_isInitialized)
        {
            return;
        }

        if (_isDirty || _isOutlineApplied != _showOutline)
        {
            _isDirty = false;
            ApplyOutline(_showOutline);
        }
    }

    /// <summary>SJY : 외부에서 아웃라인 활성화 여부를 변경</summary>
    public void SetOutline(bool isEnabled)
    {
        _showOutline = isEnabled;
        ApplyOutline(isEnabled);
    }

    private void ApplyOutline(bool isEnabled)
    {
        if (!_isInitialized)
        {
            return;
        }

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null)
            {
                continue;
            }

            _renderers[i].sharedMaterials = isEnabled ? _outlineMaterials[i] : _originalMaterials[i];
        }

        _isOutlineApplied = isEnabled;
    }

    private void OnValidate()
    {
        _isDirty = true;
    }

    /// <summary>SJY : </summary>
    private void OnDisable()
    {
        ApplyOutline(false);
        _isInitialized = false;
    }

    private void OnDestroy()
    {
        ApplyOutline(false);
    }
}
