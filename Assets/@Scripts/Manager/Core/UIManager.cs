using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary> KDH : 게임의 HUD, Screen, Popup, Overlay, Loading UI를 생성하고 관리한다. </summary>
public class UIManager
{
    #region ===== 참조 =====
    
    private Camera _worldCamera;
    private Camera _uiCamera;
    
    #endregion ===== 참조 =====
    
    #region ===== 상태 =====

    public BaseHUD CurrentHUD { get; private set; }
    public BaseScreen CurrentScreen { get; private set; }
    public BasePopup CurrentPopup => _popupStack.Count > 0 ? _popupStack.Peek() : null;

    public bool IsSceneUICamera(Camera camera) => _uiCamera != null && camera == _uiCamera;
    
    #endregion ===== 상태 =====

    #region ===== Popup =====

    private readonly Stack<BasePopup> _popupStack = new();
    
    private const int PopupSortingInterval = 2;
    private int _nextPopupSortingOrder = 2;

    #endregion ===== Popup =====

    #region ===== Overlay =====

    private ToastMessage _toastMessage;

    private LoadingUI _loadingUI;
    public bool IsLoading => _loadingUI != null && _loadingUI.gameObject.activeSelf;

    #endregion ===== Overlay =====

    #region ===== Root =====

    private GameObject _root;

    private GameObject Root
    {
        get
        {
            if (_root == null)
            {
                _root = new GameObject("UI_Root");
            }

            return _root;
        }
    }

    private GameObject _persistentRoot;

    private GameObject PersistentRoot
    {
        get
        {
            if (_persistentRoot == null)
            {
                _persistentRoot = new GameObject("UI_PersistentRoot");
                UnityEngine.Object.DontDestroyOnLoad(_persistentRoot);
            }

            return _persistentRoot;
        }
    }

    #endregion ===== Root =====

    /// <summary> KDH : 현재 활성화된 UI에 따라 입력 처리를 수행한다. </summary>
    public void OnUpdate()
    {
        bool isInputBlocked = IsLoading || CurrentPopup != null;

        if (CurrentScreen != null)
        {
            isInputBlocked = true;
        }
        else if (CurrentHUD == null || !CurrentHUD.IsVisible)
        {
            isInputBlocked = true;
        }

        Managers.Input.SetInputBlocked(this, isInputBlocked);

        if (isInputBlocked)
        {
            if (CurrentPopup != null && CurrentPopup.IsRaycastEnabled)
            {
                CurrentPopup.OnInputKey();
            }
            else if (!IsLoading && CurrentScreen != null)
            {
                CurrentScreen.OnInputKey();
            }

            return;
        }

        CurrentHUD.OnInputKey();
    }

    #region ===== Canvas =====

    /// <summary> KDH : UI Canvas를 설정하고 UI 종류에 맞는 정렬 순서를 적용한다. </summary>
    public void SetupCanvas(BaseUI baseUI)
    {
        if (baseUI == null)
        {
            return;
        }

        Canvas canvas = baseUI.gameObject.GetOrAddComponent<Canvas>();

        BindCanvasCamera(canvas, baseUI is BaseHUD or BasePopup or BaseScreen);
        canvas.overrideSorting = true;

        if (baseUI is BaseHUD baseHUD)
        {
            CurrentHUD = baseHUD;
        }

        if (baseUI is BasePopup)
        {
            canvas.sortingOrder = _nextPopupSortingOrder;
            _nextPopupSortingOrder += PopupSortingInterval;
            return;
        }

        canvas.sortingOrder = baseUI.SortingOrder;
    }
    
    /// <summary> KDH : 월드와 UI의 화면 영역을 같은 정규화 좌표로 맞춘다. </summary>
    public void SyncSceneUIViewport(Camera worldCamera)
    {
        if (_uiCamera != null && _worldCamera == worldCamera)
        {
            _uiCamera.rect = worldCamera.rect;
        }
    }
    
    /// <summary> KDH : 씬의 HUD와 카메라의 초기화가 끝난 후 연결을 확인한다. </summary>
    public void RefreshHUDCamera()
    {
        if (CurrentHUD != null)
        {
            BindCanvasCamera(CurrentHUD.GetComponent<Canvas>(), true);
        }
    }
    
    /// <summary> KDH : Canvas에 UI 카메라를 바인딩하고, 카메라 유무에 따라 렌더링 모드를 설정한다. </summary>
    private void BindCanvasCamera(Canvas canvas, bool useSceneCamera)
    {
        Camera camera = useSceneCamera ? GetSceneUICamera() : null;
        canvas.renderMode = camera != null ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = camera;
        if (camera != null)
        {
            // 카메라의 Near Clip Plane보다 약간 앞에 Canvas 배치
            canvas.planeDistance = camera.nearClipPlane + 0.02f;
        }
    }

    /// <summary> KDH : 월드 렌더링 이후 화면 UI를 렌더링할 전용 카메라를 생성하고 씬과 함께 관리한다. </summary>
    private Camera GetSceneUICamera()
    {
        // 메인 카메라가 속한 씬을 기준으로 UI 카메라를 관리
        Camera worldCamera = Camera.main;
        Scene scene = worldCamera != null ? worldCamera.gameObject.scene : SceneManager.GetActiveScene();

        // 기존 UI 카메라가 현재 씬과 메인 카메라에 연결되어 있다면 재사용
        if (_uiCamera != null && _uiCamera.gameObject.scene == scene && _worldCamera == worldCamera)
        {
            return _uiCamera;
        }

        // 메인 카메라의 URP 카메라 스택 가져오기
        List<Camera> stack = worldCamera != null ? worldCamera.GetUniversalAdditionalCameraData().cameraStack : null;
        if (worldCamera != null && stack == null) // 메인 카메라가 있지만 카메라 스택을 사용할 수 없다면 생성 중단
        {
            return null;
        }

        // UI 전용 카메라를 생성하고 메인 카메라와 동일한 씬에 배치
        GameObject cameraObj = new GameObject("UI Camera", typeof(Camera), typeof(UniversalAdditionalCameraData));
        SceneManager.MoveGameObjectToScene(cameraObj, scene);
        
        // 월드 오브젝트와 겹치지 않도록 UI 카메라를 월드에서 멀리 배치
        cameraObj.transform.position = Vector3.back * 10000f;
        
        // UI 렌더링에 사용할 직교 투영 카메라 설정
        Camera camera = cameraObj.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 1f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 2f;
        // 기존 UI 프리팹에 혼용된 Default와 UI 레이어만 렌더링한다.
        camera.cullingMask = LayerMask.GetMask("Default", "UI");
        // 카메라의 기본 화면 초기화 설정
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        if (worldCamera != null)
        {
            // 메인 카메라와 동일한 디스플레이 및 화면 영역 사용
            camera.targetDisplay = worldCamera.targetDisplay;
            camera.rect = worldCamera.rect;
        }

        // URP 카메라 스택 사용 여부에 따라 Overlay 또는 Base 카메라로 설정
        UniversalAdditionalCameraData uiData = camera.GetUniversalAdditionalCameraData();
        uiData.renderType = stack != null ? CameraRenderType.Overlay : CameraRenderType.Base;
        
        // UI 전용 카메라에서 불필요한 후처리 및 추가 텍스처 사용 비활성화
        uiData.renderPostProcessing = false;
        uiData.volumeLayerMask = 0;
        uiData.requiresColorTexture = false;
        uiData.requiresDepthTexture = false;
        
        // 메인 카메라의 스택에 UI 카메라를 추가하여 월드 렌더링 이후 UI 렌더링
        stack?.Add(camera);

        // 생성한 UI 카메라와 연결된 메인 카메라 참조 저장
        _worldCamera = worldCamera;
        _uiCamera = camera;
        
        return camera;
    }
    
    #endregion ===== Canvas =====

    #region ===== Screen =====

    /// <summary> KDH : 지정된 타입의 Screen UI를 연다. </summary>
    public T OpenScreen<T>() where T : BaseScreen
    {
        if (CurrentScreen is T)
        {
            return null;
        }

        if (CurrentScreen != null)
        {
            CloseAll();
        }

        string resourceName = typeof(T).Name;
        GameObject uiObject = Managers.Resource.Spawn(ResourceKey.Path.ScreenUI + resourceName);

        if (uiObject == null)
        {
            CPrint.Error($"Screen UI를 찾을 수 없습니다. [{resourceName}]");
            return null;
        }

        uiObject.transform.SetParent(Root.transform, false);

        T screen = uiObject.GetOrAddComponent<T>();
        CurrentScreen = screen;

        return screen;
    }

    /// <summary> KDH : 현재 열려 있는 Screen UI를 닫는다. </summary>
    private void CloseCurrentScreen()
    {
        if (CurrentScreen == null)
        {
            return;
        }

        CurrentScreen.gameObject.DestroyGO();
        CurrentScreen = null;
        
        Managers.Input.SetCursorLock(Managers.Scene.CurrentScene.DefaultCursorLock);
    }

    #endregion ===== Screen =====

    #region ===== Popup =====

    /// <summary> KDH : 지정된 타입의 Popup UI를 연다. </summary>
    public T OpenPopup<T>() where T : BasePopup
    {
        string resourceName = typeof(T).Name;
        GameObject uiObject = Managers.Resource.Spawn(ResourceKey.Path.PopupUI + resourceName);

        if (uiObject == null)
        {
            CPrint.Error($"Popup UI를 찾을 수 없습니다. [{resourceName}]");
            return null;
        }

        Transform parent = CurrentScreen != null ? CurrentScreen.transform : Root.transform;

        uiObject.transform.SetParent(parent, false);
        uiObject.transform.localScale = Vector3.one;

        T popup = uiObject.GetOrAddComponent<T>();
        _popupStack.Push(popup);

        Managers.Input.SetCursorLock(false);

        return popup;
    }

    /// <summary> KDH : 가장 최근에 열린 Popup을 닫는다. </summary>
    private void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
        {
            return;
        }

        BasePopup popup = _popupStack.Pop();

        if (popup != null)
        {
            popup.gameObject.DestroyGO();
        }

        _nextPopupSortingOrder -= PopupSortingInterval;

        if (_popupStack.Count == 0)
        {
            Managers.Input.SetCursorLock(Managers.Scene.CurrentScene.DefaultCursorLock);
        }
    }

    /// <summary> KDH : 지정된 Popup이 가장 최근에 열린 Popup인 경우 닫는다. </summary>
    public void ClosePopupUI(BasePopup popup)
    {
        if (_popupStack.Count == 0)
        {
            return;
        }

        if (CurrentPopup != popup)
        {
            CPrint.Error("가장 최근에 열린 Popup이 아닙니다.");
            return;
        }

        ClosePopupUI();
    }

    /// <summary> KDH : 현재 열려 있는 모든 Popup을 닫는다. </summary>
    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
        {
            ClosePopupUI();
        }
    }

    #endregion ===== Popup =====

    #region ===== Overlay =====

    /// <summary> KDH : 토스트 메시지를 활성화한다. </summary>
    public void OpenToastMessage(string message, bool allowDuplicate = false)
    {
        if (!TryCreateToastMessage())
        {
            return;
        }

        _toastMessage.ActiveMessage(message, allowDuplicate);
    }

    /// <summary> KDH : 활성화된 토스트 메세지를 비활성화시킨다. </summary>
    public void CloseToastMessage()
    {
        _toastMessage?.ResetToastMessage();
    }

    /// <summary> KDH : 토스트 메세지 UI가 없으면 생성하고 참조를 저장한다. </summary>
    private bool TryCreateToastMessage()
    {
        if (_toastMessage != null)
        {
            return true;
        }

        GameObject uiObject = Managers.Resource.Spawn(ResourceKey.Path.OverlayUI + nameof(ToastMessage));

        if (uiObject == null)
        {
            CPrint.Error("ToastMessage를 생성하지 못했습니다.");
            return false;
        }

        uiObject.transform.SetParent(PersistentRoot.transform, false);

        _toastMessage = uiObject.GetOrAddComponent<ToastMessage>();

        SetupCanvas(_toastMessage);

        return true;
    }

    /// <summary> KDH : Loading UI를 열고 지정된 페이드 연출을 재생한다. </summary>
    public void OpenLoadingUI(float fadeTime = 0f, Action openAction = null)
    {
        if (IsLoading)
        {
            return;
        }

        if (!TryCreateLoading())
        {
            return;
        }

        _loadingUI.gameObject.SetActive(true);
        _loadingUI.FadeIn(fadeTime, openAction);
    }

    /// <summary> KDH : 현재 Loading UI를 닫고 지정된 페이드 연출을 재생한다. </summary>
    public void CloseLoadingUI(float fadeTime = 0f, Action closeAction = null)
    {
        if (!IsLoading)
        {
            return;
        }

        if (fadeTime <= 0f)
        {
            _loadingUI.gameObject.SetActive(false);
            return;
        }

        _loadingUI.FadeOut(fadeTime, closeAction);
    }

    /// <summary> KDH : 로딩 UI가 없으면 생성하고 참조를 저장한다. </summary>
    private bool TryCreateLoading()
    {
        if (_loadingUI != null)
        {
            return true;
        }

        GameObject uiObject = Managers.Resource.Spawn(ResourceKey.Path.OverlayUI + nameof(LoadingUI));

        if (uiObject == null)
        {
            CPrint.Error("LoadingUI를 생성하지 못했습니다.");
            return false;
        }

        uiObject.transform.SetParent(PersistentRoot.transform, false);

        _loadingUI = uiObject.GetOrAddComponent<LoadingUI>();

        SetupCanvas(_loadingUI);

        return true;
    }

    /// <summary> KDH : 현재 열려 있는 Overlay UI를 닫는다. </summary>
    private void CloseAllOverlay()
    {
        CloseToastMessage();
    }

    #endregion ===== Overlay =====

    #region ===== Slot =====

    /// <summary> KDH : 해당 트랜스폼에 슬롯을 생성한다. </summary>
    public T MakeSlot<T>(Transform parent) where T : SlotUI
    {
        GameObject slotGO = Managers.Resource.Spawn(ResourceKey.Path.SlotUI + typeof(T).Name, parent);

        if (slotGO == null)
        {
            return null;
        }

        slotGO.transform.localScale = Vector3.one;

        return slotGO.GetOrAddComponent<T>();
    }

    #endregion ===== Slot =====

    #region ===== 정리 =====

    /// <summary> KDH : 현재 Screen과 모든 Popup 그리고 Overlay를 닫는다. </summary>
    public void CloseAll()
    {
        CloseAllPopupUI();
        CloseCurrentScreen();
        CloseAllOverlay();
    }

    /// <summary> KDH : 현재 씬에서 사용한 UI 상태를 초기화한다. </summary>
    public void Clear()
    {
        CloseAll();

        CurrentHUD = null;
        _nextPopupSortingOrder = 2;
    }

    #endregion ===== 정리 =====
}