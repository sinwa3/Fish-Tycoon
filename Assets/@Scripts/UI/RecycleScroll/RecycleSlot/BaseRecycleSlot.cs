using UnityEngine;

/// <summary> KDH : 재활용 스크롤에서 사용하는 슬롯의 기본 기능을 관리한다. </summary>
public abstract class BaseRecycleSlot<T> : BaseUI
{
    #region ===== 참조 =====

    private RectTransform _rect;

    public RectTransform Rect
    {
        get
        {
            if (_rect == null)
            {
                ResolveRect();
            }
            
            return _rect;
        }
    }

    #endregion ===== 참조 =====

    #region ===== 상태 =====

    public T Data { get; private set; }
    
    #endregion ===== 상태 =====
    
    private void Awake()
    {
        OnAwake();
    }

    protected abstract void OnAwake();

    /// <summary> KDH : RectTransform을 참조한다. </summary>
    private void ResolveRect()
    {
        _rect =  GetComponent<RectTransform>();
    }

    /// <summary> KDH : 전달받은 데이터로 슬롯의 UI를 갱신한다. </summary>
    public virtual void UpdateData(T data)
    {
        Data = data;
    }
}
