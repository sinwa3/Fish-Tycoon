using UnityEngine;

/// <summary> KDH : UI의 Sorting Order를 임시 변경하고 복원 시 추가된 컴포넌트를 제거한다. </summary>
public class UIHighlightCanvas : MonoBehaviour
{
    private Canvas _canvas;

    private bool _isCanvasAdded;
    private bool _originalOverrideSorting;
    private int _originalSortingOrder;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();

        // 기존 Canvas가 없을 때만 새로 추가
        _isCanvasAdded = _canvas == null;

        if (_isCanvasAdded)
        {
            _canvas = gameObject.AddComponent<Canvas>();
        }

        _originalOverrideSorting = _canvas.overrideSorting;
        _originalSortingOrder = _canvas.sortingOrder;
        _canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
    }

    /// <summary> KDH : 지정된 Sorting Order로 UI를 표시한다. </summary>
    public void Highlight(int sortingOrder)
    {
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = sortingOrder;
    }

    /// <summary> KDH : 기존 Canvas 상태를 복원하고 하이라이트 컴포넌트를 제거한다. </summary>
    public void Restore()
    {
        ResetSorting();

        if (_isCanvasAdded)
        {
            Destroy(_canvas);
        }

        Destroy(this);
    }

    /// <summary> KDH : 컴포넌트를 유지한 채 원래 정렬 상태로 돌린다. </summary>
    public void ResetSorting()
    {
        if (_canvas == null)
        {
            return;
        }

        _canvas.overrideSorting = _originalOverrideSorting;
        _canvas.sortingOrder = _originalSortingOrder;
    }
}
