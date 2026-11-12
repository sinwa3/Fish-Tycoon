using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System;

/// <summary> KDH : 보안 미니게임을 로드하고 렌더텍스처를 이용해 UI에 표시한다. </summary>
public class SecurityMinigameScreen : BaseScreen
{
    public enum MinigameType
    {
        PinLock,
        RotationLock,
    }

    public enum RawImages
    {
        MinigameRenderImage,
    }

    public enum Texts
    {
        InstructionText,
        LockpickCountText,
    }
    
    #region ===== 참조 =====

    private GameObject _minigameRoot;
    private GameObject _minigame;

    private PinLockController _pinLockController;
    private RotationLockController _rotationLockController;

    private Camera _minigameCamera;
    private RenderTexture _renderTexture;

    private Camera _blurCamera;
    private Volume _blurVolume;

    private Action _unlockSuccessCallback;

    #endregion ===== 참조 =====

    #region ===== 상태 =====

    private const int RenderTextureWidth = 1920;
    private const int RenderTextureHeight = 1080;

    #endregion ===== 상태 =====

    protected override void OnAwake()
    {
        Bind<RawImage>(typeof(RawImages));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
    }

    protected override void OnUpdate()
    {
    }

    protected override void Close()
    {
        UnsubscribeEvents();
        
        _pinLockController?.StopMiniGame();
        _rotationLockController?.StopMiniGame();

        _unlockSuccessCallback = null;

        Managers.UI?.CurrentHUD.SetVisible(true);

        ReleaseRenderTexture();
        
        if (_minigameRoot != null)
        {
            Destroy(_minigameRoot);
            _minigameRoot = null;
        }

        base.Close();
    }

    #region ===== 초기화 =====

    /// <summary> KDH, SJY : 선택한 미니게임을 생성하고 입력을 시작한다. 락픽이 없을시 종료</summary>
    public void Set(MinigameType minigameType, Camera blurCamera, Action unlockSuccessCallback = null)
    {
        if (!LockpickInventory.HasLockpick())
        {
            Close();
            Managers.UI?.OpenToastMessage("락픽이 부족합니다.");

            return;
        }

        _unlockSuccessCallback = unlockSuccessCallback;

        _blurCamera = blurCamera;
        
        Managers.UI?.CurrentHUD.SetVisible(false);

        CreateMinigame(minigameType);

        if (_minigame == null)
        {
            Close();
            return;
        }

        CreateMinigameCamera(minigameType);
        
        SetBackgroundBlur();
        
        Get<TMP_Text>(Texts.InstructionText)?.SetText(GetInstructionText(minigameType));

        SubscribeEvents();
        UpdateLockpickCountText();

        switch (minigameType)
        {
            case MinigameType.PinLock:
                _pinLockController?.StartMiniGame();
                break;

            case MinigameType.RotationLock:
                _rotationLockController?.StartMiniGame();
                break;
        }
    }

    /// <summary> KDH : 미니게임 스크린의 배경 블러 효과를 설정한다. </summary>
    private void SetBackgroundBlur()
    {
        if (_blurVolume == null)
        {
            GameObject blurObject = new GameObject("SecurityMinigameBlur");
            blurObject.transform.SetParent(_minigameRoot.transform, false);

            _blurVolume = blurObject.GetOrAddComponent<Volume>();
            _blurVolume.isGlobal = true;
            _blurVolume.priority = 100f;
            _blurVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            DepthOfField depthOfField = _blurVolume.profile.Add<DepthOfField>(true);

            depthOfField.mode.Override(DepthOfFieldMode.Bokeh);
            depthOfField.focusDistance.Override(0.1f);
            depthOfField.focalLength.Override(50f);
            depthOfField.aperture.Override(5.6f);
            depthOfField.bladeCount.Override(5);
            depthOfField.bladeCurvature.Override(1f);
            depthOfField.bladeRotation.Override(0f);
        }

        if (_blurCamera != null)
        {
            var blurCameraData = _blurCamera.GetOrAddComponent<UniversalAdditionalCameraData>();
            blurCameraData.renderPostProcessing = true;
        }
    }
    
    /// <summary> KDH : 컴포넌트를 참조한다. </summary>
    private void ResolveComponents()
    {
        if (_pinLockController == null && _minigameRoot != null && _minigame.TryGetComponent(out _pinLockController))
        {
            _pinLockController.OnUnlockCompleted += HandlePinLockCompleted;
            _pinLockController.OnLockpickBroken += HandlePinLockBroken;
        }

        if (_rotationLockController == null && _minigameRoot != null && _minigame.TryGetComponent(out _rotationLockController))
        {
            _rotationLockController.OnUnlockCompleted += HandleRotationLockCompleted;
            _rotationLockController.OnLockpickBroken += HandleRotationLockBroken;
        }
    }

    /// <summary> SJY : 퀵슬롯 수량 변경 이벤트 구독 </summary>
    private void SubscribeEvents()
    {
        if (Managers.Data == null)
        {
            return;
        }

        Managers.Data.OnQuickSlotChanged -= UpdateLockpickCountText;
        Managers.Data.OnQuickSlotChanged += UpdateLockpickCountText;
    }

    /// <summary> SJY : 미니게임 종료 시 등록한 이벤트 해제 </summary>
    private void UnsubscribeEvents()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnQuickSlotChanged -= UpdateLockpickCountText;
        }

        if (_pinLockController != null)
        {
            _pinLockController.OnUnlockCompleted -= HandlePinLockCompleted;
            _pinLockController.OnLockpickBroken -= HandlePinLockBroken;
        }

        if (_rotationLockController != null)
        {
            _rotationLockController.OnUnlockCompleted -= HandleRotationLockCompleted;
            _rotationLockController.OnLockpickBroken -= HandleRotationLockBroken;
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 미니게임 =====

    /// <summary> KDH : 미니게임에 따라 사용설명서를 가져온다. </summary>
    private static string GetInstructionText(MinigameType minigameType)
    {
        return minigameType switch
        {
            MinigameType.PinLock => "[핀 자물쇠]  A / D : 핀 선택    S : 핀 튕기기    좌클릭 : 핀 고정",
            MinigameType.RotationLock => "[회전 자물쇠]  마우스 좌우 이동 : 락픽 조절    D : 자물쇠 회전",
            _ => string.Empty,
        };
    }
    
    /// <summary> KDH : 루트와 선택한 미니게임의 오브젝트를 생성한다. </summary>
    private void CreateMinigame(MinigameType minigameType)
    {
        if (_minigameRoot != null)
        {
            _minigameRoot.DestroyGO();
        }

        _minigameRoot = new GameObject("SecurityMinigameRoot");
        _minigameRoot.transform.position = Vector3.one * 10000f;

        _minigame = Managers.Resource.Spawn(ResourceKey.Path.LockGame + minigameType, _minigameRoot.transform);
        _minigame.transform.localPosition = Vector3.zero;

        Utils.SetLayerRecursively(_minigame, LayerKey.Idx.SecurityMinigame);

        if (_minigame.TryGetComponent(out PinLockTest pinLockTest))
        {
            pinLockTest.enabled = false;
        }

        if (_minigame.TryGetComponent(out RotationLockTest rotationLockTest))
        {
            rotationLockTest.enabled = false;
        }

        ResolveComponents();
    }

    #endregion ===== 미니게임 =====

    #region ===== 카메라 =====

    /// <summary> 미니게임 전용 직교 카메라와 조명을 생성한다. </summary>
    private void CreateMinigameCamera(MinigameType minigameType)
    {
        if (_minigameRoot == null)
        {
            return;
        }
        
        GameObject cameraObject = new GameObject("SecurityMinigameCamera");
        cameraObject.transform.SetParent(_minigameRoot.transform, false);

        _minigameCamera = cameraObject.GetOrAddComponent<Camera>();

        _minigameCamera.cullingMask = LayerKey.Mask.SecurityMinigame;
        _minigameCamera.clearFlags = CameraClearFlags.SolidColor;
        _minigameCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        _minigameCamera.targetTexture = CreateRenderTexture();

        _minigameCamera.orthographic = true;
        
        switch (minigameType)
        {
            case MinigameType.PinLock:
                cameraObject.transform.localPosition = new Vector3(-0.07f, 0.82f, -1.5f);
                cameraObject.transform.localEulerAngles = new Vector3(22f, 11f, 0f);
                _minigameCamera.orthographicSize = 2f;
                break;

            case MinigameType.RotationLock:
                cameraObject.transform.localPosition = new Vector3(0f, 0f, -5f);
                cameraObject.transform.localEulerAngles = Vector3.zero;
                _minigameCamera.orthographicSize = 3.5f;
                break;
        }

        GameObject lightObject = new GameObject("SecurityMinigameLight");
        lightObject.transform.SetParent(_minigameRoot.transform, false);
        lightObject.transform.localPosition = new Vector3(-2.5f, 3f, -2f);
        lightObject.transform.localRotation = Quaternion.LookRotation(-lightObject.transform.localPosition);

        Light minigameLight = lightObject.GetOrAddComponent<Light>();
        minigameLight.type = LightType.Spot;
        minigameLight.intensity = 30f;
        minigameLight.range = 8f;
        minigameLight.spotAngle = 179f;
        minigameLight.innerSpotAngle = 179f;
        minigameLight.shadows = LightShadows.None;
    }
    
    /// <summary> KDH : 미니게임 전용 렌더 텍스처를 만들고 RawImage에 연결한다. </summary>
    private RenderTexture CreateRenderTexture()
    {
        RawImage rawImage = Get<RawImage>(RawImages.MinigameRenderImage);

        if (rawImage == null)
        {
            return null;
        }
        
        _renderTexture = new RenderTexture(RenderTextureWidth, RenderTextureHeight, 24, RenderTextureFormat.ARGB32)
        {
            name = "SecurityMinigameRT",
            filterMode = FilterMode.Bilinear,
        };

        _renderTexture.Create();

        rawImage.texture = _renderTexture;

        return _renderTexture;
    }
    
    /// <summary> KDH : 렌더 텍스처를 해제한다. </summary>
    private void ReleaseRenderTexture()
    {
        if (_renderTexture == null)
        {
            return;
        }

        _renderTexture.Release();
        Destroy(_renderTexture);
        _renderTexture = null;
    }

    #endregion ===== 카메라 =====

    #region ==== 락픽 ====

    /// <summary> SJY : 보유한 락픽 수량을 UI에 표시한다. </summary>
    private void UpdateLockpickCountText()
    {
        Get<TMP_Text>(Texts.LockpickCountText)?.SetText($"락픽 : {LockpickInventory.GetCount()}개");
    }

    /// <summary> SJY : 락픽 파손 시 1개 소모하고 남은 락픽을 확인한다. </summary>
    private void HandleLockpickBroken()
    {

        if (!LockpickInventory.TryConsume())
        {
            Close();
            Managers.UI?.OpenToastMessage("락픽이 부족하여 해제를 중단합니다.");

            return;
        }

        UpdateLockpickCountText();

        if (!LockpickInventory.HasLockpick())
        {
            Close();
            Managers.UI?.OpenToastMessage("락픽이 모두 파괴되어 해제를 중단합니다.");

            return;
        }

        Managers.UI?.OpenToastMessage("락픽이 파괴되었습니다!");

        if (_pinLockController != null && _pinLockController.IsReplacing)
        {
            _pinLockController.ReplaceLockpick();
        }
        else if (_rotationLockController != null && _rotationLockController.IsReplacing)
        {
            _rotationLockController.ReplaceLockpick();
        }
    }

    /// <summary> SJY : 잠금 해제 성공 시 락픽 소비 및 퀘스트 진행 </summary>
    private void HandleUnlockSuccess(QuestType questType)
    {
        if (!LockpickInventory.TryConsume())
        {
            CPrint.Warning("[SecurityMinigameScreen] 잠금 해제 성공 후 락픽 소비에 실패했습니다.");

            Close();
            return;
        }

        UpdateLockpickCountText();

        Managers.Quest?.ReportProgress(SceneType.StealthScene, QuestType.PinLock);

        _unlockSuccessCallback?.Invoke();

        Close();
    }

    #endregion ==== 락픽 ====

    #region ===== 이벤트 =====

    /// <summary> KDH : 핀 잠금 해제 완료 시 퀘스트 진행도를 갱신한다. </summary>
    private void HandlePinLockCompleted()
    {
        HandleUnlockSuccess(QuestType.PinLock);
    }

    /// <summary> KDH : 회전 잠금 해제 완료 시 퀘스트 진행도를 갱신한다. </summary>
    private void HandleRotationLockCompleted()
    {
        HandleUnlockSuccess(QuestType.RotationLock);
    }

    /// <summary> KDH : 핀 잠금 도구가 파손되면 교체한다. </summary>
    private void HandlePinLockBroken()
    {
        HandleLockpickBroken();
        //_pinLockController?.ReplaceLockpick();
        //CPrint.Log("핀 잠금 도구 파손");
    }

    /// <summary> KDH : 회전 잠금 도구가 파손되면 교체한다. </summary>
    private void HandleRotationLockBroken()
    {
        HandleLockpickBroken();
        //_rotationLockController?.ReplaceLockpick();
        //CPrint.Log("회전 잠금 도구 파손");
    }
    
    #endregion ===== 이벤트 =====
}