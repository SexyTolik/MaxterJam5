using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Renderer Feature для CRT-эффекта в URP.
/// Как подключить:
/// 1) Выбери свой URP Renderer asset (обычно "Assets/.../UniversalRenderer.asset" или "ForwardRenderer.asset").
/// 2) В инспекторе нажми "Add Renderer Feature" -> "CRT Effect Feature".
/// 3) Создай материал с шейдером "Hidden/URP/CRTEffect" (правой кнопкой в Project -> Create -> Material,
///    затем в дропдауне шейдера выбери Hidden/URP/CRTEffect) и назначь его в поле Material.
/// 4) Настрой параметры эффекта прямо в инспекторе Renderer Feature.
/// </summary>
public class CRTEffectFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("На каком этапе рендера применять эффект")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        [Header("Материал")]
        [Tooltip("Материал с шейдером Hidden/URP/CRTEffect")]
        public Material material;

        [Header("Параметры эффекта")]
        [Range(0.5f, 10f)] public float curvature = 4f;
        [Range(0f, 1f)] public float scanlineIntensity = 0.3f;
        [Range(100f, 2000f)] public float scanlineCount = 800f;
        [Range(0f, 3f)] public float vignetteIntensity = 1f;
        [Range(0f, 0.02f)] public float chromaticAberration = 0.003f;
        [Range(0f, 1f)] public float noiseIntensity = 0.05f;
        [Range(0f, 2f)] public float brightness = 1.1f;
    }

    public Settings settings = new Settings();

    private CRTRenderPass crtPass;

    public override void Create()
    {
        crtPass = new CRTRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
            return;

        // Не применяем эффект к камерам превью/рефлексий и т.п.
        if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView)
            return;

        crtPass.SetTarget(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(crtPass);
    }

    protected override void Dispose(bool disposing)
    {
        crtPass?.Dispose();
    }

    private class CRTRenderPass : ScriptableRenderPass
    {
        private const string ProfilerTag = "CRT Effect";

        private readonly Settings settings;
        private RTHandle source;
        private RTHandle tempTexture;

        public CRTRenderPass(Settings settings)
        {
            this.settings = settings;
            renderPassEvent = settings.renderPassEvent;
        }

        public void SetTarget(RTHandle cameraColorTarget)
        {
            source = cameraColorTarget;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;

            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, desc, name: "_CRTEffectTempTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.material == null || source == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get(ProfilerTag);

            using (new ProfilingScope(cmd, new ProfilingSampler(ProfilerTag)))
            {
                settings.material.SetFloat("_Curvature", settings.curvature);
                settings.material.SetFloat("_ScanlineIntensity", settings.scanlineIntensity);
                settings.material.SetFloat("_ScanlineCount", settings.scanlineCount);
                settings.material.SetFloat("_VignetteIntensity", settings.vignetteIntensity);
                settings.material.SetFloat("_ChromaticAberration", settings.chromaticAberration);
                settings.material.SetFloat("_NoiseIntensity", settings.noiseIntensity);
                settings.material.SetFloat("_Brightness", settings.brightness);

                // Рисуем исходное изображение через наш материал во временную текстуру,
                // затем копируем результат обратно в цель камеры
                Blitter.BlitCameraTexture(cmd, source, tempTexture, settings.material, 0);
                Blitter.BlitCameraTexture(cmd, tempTexture, source);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            tempTexture?.Release();
        }
    }
}
