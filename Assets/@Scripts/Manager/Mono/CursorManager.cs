using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 인게임 커서의 이미지, 크기 및 위치를 관리한다. </summary>
[RequireComponent(typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler))]
public class CursorManager : MonoBehaviour
{
    #region ===== 참조 =====

    private Canvas _canvas;
    private RectTransform _canvasRect;

    private Image _cursorImage;
    private RectTransform _cursorRect;

    private Sprite _normalCursor;
    // SJY : 클릭시의 스프라이트
    private Sprite _pressedCursor;

    #endregion ===== 참조 =====

    #region ===== 설정 =====

    private const float CursorReferenceWidth = 64f;
    private const int CursorSortingOrder = 9999;

    private static readonly Vector2 MouthHotspot = new(0.047f, 0.75f);

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private bool _isVisible = true;

    #endregion ===== 상태 =====

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = CursorSortingOrder;
        _canvasRect = GetComponent<RectTransform>();

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;

        _normalCursor ??= Managers.Resource.LoadSprite(ResourceKey.Path.CursorSprite + ResourceKey.Name.Cursor, "CursorNormal");
        _pressedCursor ??= Managers.Resource.LoadSprite(ResourceKey.Path.CursorSprite + ResourceKey.Name.Cursor, "CursorPressed");

        CreateCursorUI();

        UpdateCursorPosition();
        UpdateCursorSprite();

        // 기존 커서 표시 상태 복원
        SetCursorVisible(_isVisible);
    }
    
    private void Update()
    {
        // 기본 OS 커서는 사용하지 않는다.
        if (Cursor.visible)
        {
            Cursor.visible = false;
        }

        if (_cursorRect == null)
        {
            return;
        }

        if (!_isVisible)
        {
            return;
        }

        UpdateCursorPosition();
        UpdateCursorSprite();
    }

    /// <summary> KDH : 인게임 커서의 표시 상태를 설정한다. </summary>
    public void SetCursorVisible(bool visible)
    {
        _isVisible = visible;

        if (_cursorImage != null)
        {
            _cursorImage.enabled = visible;

            if (visible)
            {
                UpdateCursorSprite();
            }
        }

        // 기본 OS 커서는 항상 숨긴다.
        Cursor.visible = false;
    }

    /// <summary> KDH : 전용 Canvas 아래에 커서 이미지를 생성한다. </summary>
    private void CreateCursorUI()
    {
        GameObject cursorObject = new GameObject(ResourceKey.Name.Cursor, typeof(RectTransform));

        cursorObject.transform.SetParent(_canvas.transform, false);

        _cursorImage = cursorObject.GetOrAddComponent<Image>();
        _cursorImage.sprite = _normalCursor;
        _cursorImage.raycastTarget = false;

        _cursorRect = _cursorImage.rectTransform;
        _cursorRect.sizeDelta = Vector2.one * CursorReferenceWidth;
        _cursorRect.anchorMin = new Vector2(0.5f, 0.5f);
        _cursorRect.anchorMax = new Vector2(0.5f, 0.5f);

        // 이미지의 입술 끝을 실제 마우스 클릭 위치에 맞춘다.
        _cursorRect.pivot = MouthHotspot;
    }

    /// <summary> KDH : 마우스 위치에 커서를 배치한다. </summary>
    private void UpdateCursorPosition()
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect,
                Managers.Input.MousePosition, null, out Vector2 localPosition))
        {
            return;
        }

        _cursorRect.anchoredPosition = localPosition;
    }

    /// <summary> SJY : 마우스 왼쪽 버튼 입력 상태에 따라 커서 스프라이트를 변경한다. </summary>
    private void UpdateCursorSprite()
    {
        if (_cursorImage == null || Managers.Input == null)
        {
            return;
        }

        bool isPressed = Managers.Input.Mouse_Left;
        Sprite targetSprite = isPressed && _pressedCursor != null ? _pressedCursor : _normalCursor;

        if (_cursorImage.sprite != targetSprite)
        {
            _cursorImage.sprite = targetSprite;
        }
    }
}