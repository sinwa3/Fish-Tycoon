using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 재활용 스크롤의 슬롯 생성, 데이터 갱신 및 스크롤 이동을 관리한다. </summary>
public abstract class BaseVerticalScroll<T> : MonoBehaviour
{
    #region ===== 참조 =====

    private RectTransform _rect;
    private ScrollRect _scrollRect;

    /// <summary> KDH : 생성할 슬롯 </summary>
    [SerializeField] BaseRecycleSlot<T> _recycleSlot;

    /// <summary> KDH : 재활용하며 사용하는 슬롯 목록 </summary>
    private readonly List<BaseRecycleSlot<T>> _slots = new();
    public IReadOnlyList<BaseRecycleSlot<T>> Slots => _slots;

    public List<T> DataList { get; protected set; }

    #endregion ===== 참조 =====

    #region ===== 설정 =====

    [Header("그리드")]
    [Min(1)]
    [SerializeField] private int _columnCount = 4;
    [SerializeField] private float _spacingWidth;
    [SerializeField] private float _spacingHeight;

    [Header("스크롤")]
    [SerializeField] private ScrollRect.MovementType _movementType = ScrollRect.MovementType.Clamped;
    [SerializeField] private float _decelerationRate = 0.005f;
    [SerializeField] private float _scrollSensitivity = 3f;

    [SerializeField] private int _paddingTop = 5;
    [SerializeField] private int _paddingBottom = 5;
    [SerializeField] private int _paddingLeft = 5;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private float _slotHeight;
    private float _slotWidth;

    private int _totalRowCount;
    private int _visibleRowCount;

    /// <summary> KDH : 슬롯 한 행의 세로 간격을 포함한 높이 </summary>
    private float ItemHeight => _slotHeight + _spacingHeight;
    /// <summary> KDH : 슬롯 한 열의 가로 간격을 포함한 너비 </summary>
    private float ItemWidth => _slotWidth + _spacingWidth;

    /// <summary> KDH : 현재 화면에서 처음 표시할 행의 인덱스 </summary>
    private int _firstVisibleRow;

    #endregion ===== 상태 =====

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _scrollRect = GetComponent<ScrollRect>();

        OnAwake();
    }

    /// <summary> KDH : DataList 초기화 필수 </summary>
    protected abstract void OnAwake();

    private void Start()
    {
        InitializeScroll();

        OnStart();
    }

    protected abstract void OnStart();

#if UNITY_EDITOR
    /// <summary> KDH : 실행 중 Inspector 설정 변경 시 스크롤 레이아웃을 갱신한다. </summary>
    private void OnValidate()
    {
        _columnCount = Mathf.Max(1, _columnCount);

        _spacingWidth = Mathf.Max(0f, _spacingWidth);
        _spacingHeight = Mathf.Max(0f, _spacingHeight);

        _paddingTop = Mathf.Max(0, _paddingTop);
        _paddingBottom = Mathf.Max(0, _paddingBottom);
        _paddingLeft = Mathf.Max(0, _paddingLeft);

        _decelerationRate = Mathf.Clamp01(_decelerationRate);
        _scrollSensitivity = Mathf.Max(0f, _scrollSensitivity);

        if (!Application.isPlaying)
        {
            return;
        }

        RefreshLayout();
    }
#endif

    private void OnDestroy()
    {
        if (_scrollRect != null)
        {
            _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }
    }

#if UNITY_EDITOR
    /// <summary> KDH : 변경된 설정값을 적용해 Content와 슬롯 배치를 갱신한다. </summary>
    private void RefreshLayout()
    {
        if (_scrollRect == null || _scrollRect.content == null || _recycleSlot == null)
        {
            return;
        }

        if (_slotHeight <= 0f || _slotWidth <= 0f)
        {
            return;
        }

        _scrollRect.movementType = _movementType;
        _scrollRect.decelerationRate = _decelerationRate;
        _scrollRect.scrollSensitivity = _scrollSensitivity;

        SetContent();
        RefreshSlots();
    }
#endif

    #region ===== 스크롤 =====

    /// <summary> KDH : 스크롤을 초기화한다. </summary>
    private void InitializeScroll()
    {
        if (!IsValidateScroll())
        {
            return;
        }

        _scrollRect.movementType = _movementType;
        _scrollRect.decelerationRate = _decelerationRate;
        _scrollRect.scrollSensitivity = _scrollSensitivity;

        _scrollRect.horizontal = false;
        _scrollRect.vertical = true;

        _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

        RectTransform slotRect = _recycleSlot.Rect;
        _slotHeight = slotRect.rect.height;
        _slotWidth = slotRect.rect.width;

        if (_slotHeight <= 0f || _slotWidth <= 0f)
        {
            CPrint.Error($"[{gameObject.name}] 슬롯의 너비와 높이를 확인해주세요.");
            return;
        }

        SetContent();

        // Content 크기 변경에 따른 레이아웃 갱신
        Canvas.ForceUpdateCanvases();

        RefreshSlots();
    }

    /// <summary> KDH : 전체 데이터 개수에 맞춰 Content의 크기와 앵커, 피벗을 설정한다. </summary>
    private void SetContent()
    {
        if (_scrollRect == null || _scrollRect.content == null || DataList == null)
        {
            return;
        }

        _totalRowCount = Mathf.CeilToInt((float)DataList.Count / _columnCount);

        float scrollHeight = _totalRowCount * _slotHeight;

        if (_totalRowCount > 1)
        {
            scrollHeight += (_totalRowCount - 1) * _spacingHeight;
        }

        _scrollRect.content.anchorMin = new Vector2(0f, 1f);
        _scrollRect.content.anchorMax = new Vector2(1f, 1f);
        _scrollRect.content.pivot = new Vector2(0.5f, 1f);

        Vector2 sizeDelta = _scrollRect.content.sizeDelta;
        sizeDelta.y = _paddingTop + scrollHeight + _paddingBottom;

        _scrollRect.content.sizeDelta = sizeDelta;
    }

    /// <summary> KDH : 스크롤에 필요한 참조와 데이터가 존재하는지 확인한다. </summary>
    private bool IsValidateScroll()
    {
        if (_scrollRect == null || _scrollRect.content == null)
        {
            CPrint.Error($"[{gameObject.name}] ScrollRect 또는 Content가 없습니다.");
            return false;
        }

        if (_recycleSlot == null)
        {
            CPrint.Error($"[{gameObject.name}] Content에 BaseRecycleSlot이 존재하지 않습니다.");
            return false;
        }

        if (DataList == null)
        {
            CPrint.Error($"[{gameObject.name}] DataList가 초기화되지 않았습니다.");
            return false;
        }

        return true;
    }

    /// <summary> KDH : 특정 데이터 인덱스가 있는 행으로 스크롤을 이동한다. </summary>
    public void ScrollToIndex(int dataIndex)
    {
        if (!IsValidateScroll())
        {
            return;
        }

        if (dataIndex < 0 || dataIndex >= DataList.Count)
        {
            return;
        }

        int rowIndex = dataIndex / _columnCount;
        float targetY = _paddingTop + rowIndex * ItemHeight;
        float maxScrollY = Mathf.Max(0f, _scrollRect.content.rect.height - _rect.rect.height);
        targetY = Mathf.Clamp(targetY, 0f, maxScrollY);
        _scrollRect.StopMovement();

        _scrollRect.content.anchoredPosition = new Vector2(_scrollRect.content.anchoredPosition.x, targetY);

        _firstVisibleRow = GetRowIdxCurrentFirstRow();

        RefreshSlots();
    }

    /// <summary> KDH : 현재 스크롤 위치에서 보이는 첫 번째 행의 인덱스를 가져온다. </summary>
    private int GetRowIdxCurrentFirstRow()
    {
        if (ItemHeight <= 0f || DataList == null || DataList.Count == 0)
        {
            return 0;
        }

        float scrollOffset = Mathf.Max(0f, _scrollRect.content.anchoredPosition.y - _paddingTop);

        int rowIndex = Mathf.FloorToInt(scrollOffset / ItemHeight);

        return Mathf.Clamp(rowIndex, 0, Mathf.Max(0, _totalRowCount - 1));
    }

    /// <summary> KDH : 스크롤 변경에 맞춰 필요한 슬롯의 데이터를 갱신한다. </summary>
    private void OnScrollValueChanged(Vector2 scrollPos)
    {
        if (DataList == null || DataList.Count == 0)
        {
            return;
        }

        int firstVisibleRow = GetRowIdxCurrentFirstRow();

        if (_firstVisibleRow == firstVisibleRow)
        {
            return;
        }

        _firstVisibleRow = firstVisibleRow;

        RefreshSlots();
    }

    #endregion ===== 스크롤 =====

    #region ===== 슬롯 =====

    /// <summary> KDH : 현재 화면에 필요한 슬롯을 생성하고 데이터를 갱신한다. </summary>
    private void RefreshSlots()
    {
        if (DataList == null || DataList.Count == 0)
        {
            foreach (BaseRecycleSlot<T> slot in _slots)
            {
                slot.gameObject.SetActive(false);
            }

            return;
        }

        float viewportHeight = _scrollRect.viewport != null ? _scrollRect.viewport.rect.height : _rect.rect.height;
        _totalRowCount = Mathf.CeilToInt((float)DataList.Count / _columnCount);
        _visibleRowCount = Mathf.CeilToInt((viewportHeight / ItemHeight) + 1);

        int requiredSlotCount = Mathf.Min(DataList.Count, _visibleRowCount * _columnCount);

        while (_slots.Count < requiredSlotCount)
        {
            BaseRecycleSlot<T> slot = CreateSlot();

            if (slot == null)
            {
                return;
            }

            _slots.Add(slot);
            OnSlotCreated(slot);
        }

        int firstDataIndex = _firstVisibleRow * _columnCount;

        for (int i = 0; i < _slots.Count; i++)
        {
            BaseRecycleSlot<T> slot = _slots[i];

            int dataIndex = firstDataIndex + i;

            if (dataIndex >= DataList.Count)
            {
                slot.gameObject.SetActive(false);
                continue;
            }

            UpdateSlotForIndex(slot, dataIndex);
            SetSlotPosition(slot, dataIndex);
        }
    }

    /// <summary> KDH : 새 슬롯 생성 직후 추가 초기화를 수행한다. </summary>
    protected virtual void OnSlotCreated(BaseRecycleSlot<T> slot)
    {
    }
    
    /// <summary> KDH : Content에 슬롯을 생성한다. </summary>
    private BaseRecycleSlot<T> CreateSlot()
    {
        GameObject slotObj = Instantiate(_recycleSlot.gameObject, _scrollRect.content);

        slotObj.name = _recycleSlot.gameObject.name;

        BaseRecycleSlot<T> slot = slotObj.GetComponent<BaseRecycleSlot<T>>();
        if (slot == null)
        {
            CPrint.Error($"[{gameObject.name}] 슬롯에 BaseRecycleSlot 컴포넌트가 없습니다.");
            Destroy(slotObj);
            return null;
        }

        slot.Rect.anchorMin = new Vector2(0f, 1f);
        slot.Rect.anchorMax = new Vector2(0f, 1f);
        slot.Rect.pivot = new Vector2(0f, 1f);
        slot.Rect.localScale = Vector3.one;

        return slot;
    }

    /// <summary> KDH : 슬롯에 데이터 인덱스를 지정하고 UI를 갱신한다. </summary>
    private void UpdateSlotForIndex(BaseRecycleSlot<T> slot, int dataIndex)
    {
        slot.gameObject.SetActive(true);
        slot.UpdateData(DataList[dataIndex]);
    }

    /// <summary> KDH : 데이터 인덱스에 맞춰 슬롯의 위치를 설정한다. </summary>
    private void SetSlotPosition(BaseRecycleSlot<T> slot, int dataIndex)
    {
        int rowIndex = dataIndex / _columnCount;
        int columnIndex = dataIndex % _columnCount;

        float x = _paddingLeft + columnIndex * ItemWidth;
        float y = -_paddingTop - rowIndex * ItemHeight;

        slot.Rect.anchoredPosition = new Vector2(x, y);
    }

    #endregion ===== 슬롯 =====
}