using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

// Renderer Feature for CRT effect in URP (Unity 6 / URP 17+, Render Graph API).
// Setup:
// 1) Select your URP Renderer asset (usually "Assets/.../UniversalRenderer.asset").
// 2) In the inspector click "Add Renderer Feature" -> "CRT Effect Feature".
// 3) Create a Material with shader "Hidden/URP/CRTEffect" and assign it to the Material field.
// 4) Tweak the effect parameters directly in the Renderer Feature inspector.
public class CRTEffectFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("When in the render pipeline the effect is applied")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        [Header("Material")]
        [Tooltip("Material using shader Hidden/URP/CRTEffect")]
        public Material material;

        [Header("Effect Parameters")]
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

        // Skip preview/reflection cameras etc.
        if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView)
            return;

        renderer.EnqueuePass(crtPass);
    }

    private class CRTRenderPass : ScriptableRenderPass
    {
        private readonly Settings settings;

        private class PassData
        {
            public Material material;
            public TextureHandle source;
        }

        public CRTRenderPass(Settings settings)
        {
            this.settings = settings;
            renderPassEvent = settings.renderPassEvent;
        }

        // Render Graph API: describe which textures are read/written here,
        // and what actually happens inside the pass (SetRenderFunc)
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (settings.material == null)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            // Skip if the camera renders directly to the backbuffer (some edge-case cameras)
            if (resourceData.isActiveTargetBackBuffer)
                return;

            settings.material.SetFloat("_Curvature", settings.curvature);
            settings.material.SetFloat("_ScanlineIntensity", settings.scanlineIntensity);
            settings.material.SetFloat("_ScanlineCount", settings.scanlineCount);
            settings.material.SetFloat("_VignetteIntensity", settings.vignetteIntensity);
            settings.material.SetFloat("_ChromaticAberration", settings.chromaticAberration);
            settings.material.SetFloat("_NoiseIntensity", settings.noiseIntensity);
            settings.material.SetFloat("_Brightness", settings.brightness);

            TextureHandle source = resourceData.activeColorTexture;

            TextureDesc destDesc = renderGraph.GetTextureDesc(source);
            destDesc.name = "_CRTEffectTempTexture";
            destDesc.clearBuffer = false;
            destDesc.depthBufferBits = 0;
            TextureHandle destination = renderGraph.CreateTexture(destDesc);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("CRT Effect", out PassData passData))
            {
                passData.material = settings.material;
                passData.source = source;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1f, 1f, 0f, 0f), data.material, 0);
                });
            }

            // Downstream passes (e.g. UI overlay) should see the processed texture
            resourceData.cameraColor = destination;
        }
    }
}
