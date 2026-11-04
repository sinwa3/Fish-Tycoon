using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary> KDH : PixelationTransition이 활성화된 동안 카메라 화면에 픽셀화 효과를 적용한다. </summary>
public class PixelationRendererFeature : ScriptableRendererFeature
{
    #region ===== 설정 =====

    [SerializeField] private Shader _shader;
    [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private PixelationRenderPass _pass;
    private Material _material;

    #endregion ===== 상태 =====

    #region ===== 초기화 =====

    public override void Create()
    {
        _pass?.Dispose();
        CoreUtils.Destroy(_material);

        _material = _shader != null ? CoreUtils.CreateEngineMaterial(_shader) : null;

        _pass = new PixelationRenderPass(_material)
        {
            renderPassEvent = _renderPassEvent
        };
    }

    #endregion ===== 초기화 =====

    #region ===== 렌더링 =====

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!PixelationTransition.IsRendering || _material == null || _pass == null)
        {
            return;
        }

        CameraType cameraType = renderingData.cameraData.cameraType;

        if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
        {
            return;
        }

        _pass.renderPassEvent = _renderPassEvent;
        renderer.EnqueuePass(_pass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (!PixelationTransition.IsRendering || _material == null || _pass == null)
        {
            return;
        }

        CameraType cameraType = renderingData.cameraData.cameraType;

        if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
        {
            return;
        }

        _pass.SetCameraColorTarget(renderer.cameraColorTargetHandle);
    }

    #endregion ===== 렌더링 =====

    #region ===== 리소스 정리 =====

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        _pass = null;

        CoreUtils.Destroy(_material);
        _material = null;
    }

    #endregion ===== 리소스 정리 =====

    /// <summary> KDH : 카메라 화면을 임시 텍스처에 복사한 후 픽셀화 셰이더를 적용한다. </summary>
    private sealed class PixelationRenderPass : ScriptableRenderPass
    {
        #region ===== 상태 =====

        private static readonly int PixelSize = Shader.PropertyToID("_PixelSize");

        private readonly Material _material;

        private RTHandle _cameraColorTarget;
        private RTHandle _temporaryColorTexture;

        #endregion ===== 상태 =====

        #region ===== 초기화 =====

        public PixelationRenderPass(Material material)
        {
            _material = material;

            profilingSampler = new ProfilingSampler(nameof(PixelationRenderPass));
        }

        public void SetCameraColorTarget(RTHandle cameraColorTarget)
        {
            _cameraColorTarget = cameraColorTarget;
        }

        #endregion ===== 초기화 =====

        #region ===== 렌더링 =====

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            RenderingUtils.ReAllocateIfNeeded(ref _temporaryColorTexture, descriptor, FilterMode.Point, 
                TextureWrapMode.Clamp, name: "_PixelationTransitionColor");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!PixelationTransition.IsRendering || _material == null || _cameraColorTarget == null ||
                _temporaryColorTexture == null)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get();

            _material.SetFloat(PixelSize, PixelationTransition.CurrentPixelSize);

            using (new ProfilingScope(cmd, profilingSampler))
            {
                Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _temporaryColorTexture);
                Blitter.BlitCameraTexture(cmd, _temporaryColorTexture, _cameraColorTarget, _material, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            
            Debug.Log($"[PixelationRendererFeature] Execute: {PixelationTransition.CurrentPixelSize}");
        }

        #endregion ===== 렌더링 =====

        #region ===== 리소스 정리 =====

        public void Dispose()
        {
            _temporaryColorTexture?.Release();
            _temporaryColorTexture = null;
            _cameraColorTarget = null;
        }

        #endregion ===== 리소스 정리 =====
    }
}