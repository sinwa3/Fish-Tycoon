using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary> KDH : 현재 상호작용 중인 렌더러를 하나의 마스크로 합친 뒤, 바깥 경계에만 아웃라인을 그린다. </summary>
public class InteractionOutlineRendererFeature : ScriptableRendererFeature
{
    #region ===== 설정 =====

    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField, Range(1, 5)] private int _outlineWidth = 3;
    [SerializeField] private bool _ignoreDepth = true;
    [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private static readonly HashSet<Renderer> ActiveRenderers = new();

    private readonly List<Renderer> _renderers = new();

    private InteractionOutlineRenderPass _pass;

    #endregion ===== 상태 =====

    #region ===== 초기화 =====

    /// <summary> KDH : 플레이 세션을 새로 시작할 때 이전 세션의 정적 등록 정보를 제거한다. </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetActiveRenderers()
    {
        ActiveRenderers.Clear();
    }

    /// <summary> KDH : 지정한 렌더러를 상호작용 아웃라인 대상에 등록하거나 해제한다. </summary>
    public static void SetRendererActive(Renderer renderer, bool isActive)
    {
        if (renderer == null)
        {
            return;
        }

        if (isActive)
        {
            ActiveRenderers.Add(renderer);
            return;
        }

        ActiveRenderers.Remove(renderer);
    }

    public override void Create()
    {
        _pass?.Dispose();

        _pass = new InteractionOutlineRenderPass
        {
            renderPassEvent = _renderPassEvent
        };
    }

    #endregion ===== 초기화 =====

    #region ===== 렌더링 =====

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_pass == null || !TryGetActiveRenderers())
        {
            return;
        }

        CameraType cameraType = renderingData.cameraData.cameraType;

        if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
        {
            return;
        }

        _pass.renderPassEvent = _renderPassEvent;
        _pass.Setup(_renderers, _outlineColor, _outlineWidth, _ignoreDepth);

        renderer.EnqueuePass(_pass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (_pass == null || _renderers.Count == 0)
        {
            return;
        }

        _pass.SetCameraColorTarget(renderer.cameraColorTargetHandle);
    }

    /// <summary> KDH : 유효하고 활성화된 상호작용 대상만 현재 프레임 렌더링 목록에 보관한다. </summary>
    private bool TryGetActiveRenderers()
    {
        _renderers.Clear();

        foreach (Renderer renderer in ActiveRenderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
            {
                continue;
            }

            _renderers.Add(renderer);
        }

        return _renderers.Count > 0;
    }

    #endregion ===== 렌더링 =====

    #region ===== 리소스 정리 =====

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        _pass = null;
    }

    #endregion ===== 리소스 정리 =====

    /// <summary> KDH : 선택된 렌더러의 마스크와 외곽선을 합성하는 렌더 패스다. </summary>
    private sealed class InteractionOutlineRenderPass : ScriptableRenderPass
    {
        #region ===== 상태 =====

        private static readonly int MaskedObjects = Shader.PropertyToID("_MaskedObjects");
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        private static readonly int DrawSides = Shader.PropertyToID("_DrawSides");
        private static readonly int OutlineFadeStart = Shader.PropertyToID("_OutlineFadeStart");
        private static readonly int OutlineFadeEnd = Shader.PropertyToID("_OutlineFadeEnd");
        private static readonly int Spread = Shader.PropertyToID("_Spread");

        private readonly List<Renderer> _renderers = new();

        private Material _maskMaterial;
        private Material _outlineMaterial;

        private RTHandle _cameraColorTarget;
        private RTHandle _colorTexture;
        private RTHandle _maskTexture;

        private Color _outlineColor;
        private int _outlineWidth;
        private bool _ignoreDepth;

        #endregion ===== 상태 =====

        #region ===== 초기화 =====

        public InteractionOutlineRenderPass()
        {
            profilingSampler = new ProfilingSampler(nameof(InteractionOutlineRenderPass));

            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public void Setup(List<Renderer> renderers, Color outlineColor, int outlineWidth, bool ignoreDepth)
        {
            _renderers.Clear();
            _renderers.AddRange(renderers);

            _outlineColor = outlineColor;
            _outlineWidth = outlineWidth;
            _ignoreDepth = ignoreDepth;
        }

        public void SetCameraColorTarget(RTHandle cameraColorTarget)
        {
            _cameraColorTarget = cameraColorTarget;
        }

        /// <summary> KDH : 아웃라인 렌더링에 필요한 머티리얼을 준비한다. </summary>
        private bool EnsureMaterials()
        {
            if (_maskMaterial == null)
            {
                Shader maskShader = Shader.Find("Hidden/ToonShadersPro/URP/MaskObject");

                if (maskShader == null)
                {
                    CPrint.Error("[InteractionOutline] MaskObject 셰이더를 찾지 못했습니다.");
                    return false;
                }

                _maskMaterial = CoreUtils.CreateEngineMaterial(maskShader);
            }

            if (_outlineMaterial == null)
            {
                Shader outlineShader = Shader.Find("Hidden/ToonShadersPro/URP/Outlines");

                if (outlineShader == null)
                {
                    CPrint.Error("[InteractionOutline] Outlines 셰이더를 찾지 못했습니다.");
                    return false;
                }

                _outlineMaterial = CoreUtils.CreateEngineMaterial(outlineShader);
            }

            return true;
        }

        #endregion ===== 초기화 =====

        #region ===== 렌더링 =====

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            RenderingUtils.ReAllocateIfNeeded(ref _colorTexture, descriptor, name: "_InteractionOutlineColor");

            descriptor.graphicsFormat = GraphicsFormat.R16_UNorm;

            RenderingUtils.ReAllocateIfNeeded(ref _maskTexture, descriptor, name: "_InteractionOutlineMask");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_renderers.Count == 0 || _cameraColorTarget == null || _colorTexture == null || _maskTexture == null ||
                !EnsureMaterials())
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, profilingSampler))
            {
                ApplyMaterialSettings();

                // 1. 현재 카메라 화면 복사
                Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _colorTexture);

                // 2. 상호작용 대상들을 하나의 마스크로 렌더링
                DrawMask(cmd);

                // 3. 마스크를 이용해 아웃라인 합성
                _outlineMaterial.SetTexture(MaskedObjects, _maskTexture);

                Blitter.BlitCameraTexture(cmd, _colorTexture, _cameraColorTarget, _outlineMaterial, 1);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <summary> KDH : 아웃라인 머티리얼에 현재 설정을 적용한다. </summary>
        private void ApplyMaterialSettings()
        {
            if (_ignoreDepth)
            {
                _maskMaterial.EnableKeyword("_IGNORE_DEPTH");
            }
            else
            {
                _maskMaterial.DisableKeyword("_IGNORE_DEPTH");
            }

            _outlineMaterial.SetColor(OutlineColor, _outlineColor);
            _outlineMaterial.SetInteger(OutlineWidth, _outlineWidth);
            _outlineMaterial.SetVector(DrawSides, new Vector2(0.0f, 1.0f));

            _outlineMaterial.SetFloat(OutlineFadeStart, 500.0f);
            _outlineMaterial.SetFloat(OutlineFadeEnd, 1000.0f);

            float spread = 1.0f / (32.0f * Mathf.Pow(_outlineWidth / 6.0f, 2));

            _outlineMaterial.SetFloat(Spread, spread);
        }

        /// <summary> KDH : 등록된 렌더러를 하나의 마스크 텍스처에 그린다. </summary>
        private void DrawMask(CommandBuffer cmd)
        {
            CoreUtils.SetRenderTarget(cmd, _maskTexture, ClearFlag.Color, Color.black);

            for (int rendererIndex = 0; rendererIndex < _renderers.Count; rendererIndex++)
            {
                Renderer renderer = _renderers[rendererIndex];

                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                int subMeshCount = renderer.sharedMaterials.Length;

                for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
                {
                    cmd.DrawRenderer(renderer, _maskMaterial, subMeshIndex, 0);
                }
            }
        }
        
        #endregion ===== 렌더링 =====

        #region ===== 리소스 정리 =====

        public void Dispose()
        {
            _colorTexture?.Release();
            _maskTexture?.Release();

            _colorTexture = null;
            _maskTexture = null;
            _cameraColorTarget = null;

            CoreUtils.Destroy(_maskMaterial);
            CoreUtils.Destroy(_outlineMaterial);

            _maskMaterial = null;
            _outlineMaterial = null;
        }

        #endregion ===== 리소스 정리 =====
    }
}