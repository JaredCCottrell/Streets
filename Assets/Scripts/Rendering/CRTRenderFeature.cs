using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Streets.Rendering
{
    /// <summary>
    /// URP Renderer Feature that applies a CRT post-process effect.
    /// Includes scanlines, screen curvature, vignette, and chromatic aberration.
    /// </summary>
    public class CRTRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class CRTSettings
        {
            [Tooltip("When to apply the CRT effect in the render pipeline")]
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

            [Tooltip("Enable/disable the effect")]
            public bool enabled = true;

            [Header("Scanlines")]
            [Tooltip("Intensity of the scanline effect")]
            [Range(0f, 1f)]
            public float scanlineIntensity = 0.3f;

            [Tooltip("Number of scanlines on screen")]
            [Range(100, 1000)]
            public int scanlineCount = 400;

            [Header("Screen Curvature")]
            [Tooltip("Amount of CRT screen curvature (barrel distortion)")]
            [Range(0f, 0.5f)]
            public float curvature = 0.1f;

            [Header("Vignette")]
            [Tooltip("Darkening at screen edges")]
            [Range(0f, 2f)]
            public float vignetteIntensity = 0.5f;

            [Header("Chromatic Aberration")]
            [Tooltip("RGB color channel separation")]
            [Range(0f, 5f)]
            public float chromaticAberration = 1f;

            [Header("Display")]
            [Tooltip("Overall brightness")]
            [Range(0.5f, 2f)]
            public float brightness = 1.1f;

            [Tooltip("Screen flicker intensity")]
            [Range(0f, 1f)]
            public float flicker = 0.1f;
        }

        public CRTSettings settings = new CRTSettings();

        private CRTRenderPass renderPass;
        private Material crtMaterial;

        // Shader property IDs
        private static readonly int ScanlineIntensityId = Shader.PropertyToID("_ScanlineIntensity");
        private static readonly int ScanlineCountId = Shader.PropertyToID("_ScanlineCount");
        private static readonly int CurvatureId = Shader.PropertyToID("_Curvature");
        private static readonly int VignetteIntensityId = Shader.PropertyToID("_VignetteIntensity");
        private static readonly int ChromaticAberrationId = Shader.PropertyToID("_ChromaticAberration");
        private static readonly int BrightnessId = Shader.PropertyToID("_Brightness");
        private static readonly int FlickerId = Shader.PropertyToID("_Flicker");

        public override void Create()
        {
            var shader = Shader.Find("Streets/PostProcess/CRT");
            if (shader == null)
            {
                Debug.LogError("CRT shader not found! Make sure 'Streets/PostProcess/CRT' shader exists.");
                return;
            }

            crtMaterial = CoreUtils.CreateEngineMaterial(shader);
            renderPass = new CRTRenderPass(crtMaterial);
            renderPass.renderPassEvent = settings.renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.enabled || crtMaterial == null)
                return;

            // Set shader properties
            crtMaterial.SetFloat(ScanlineIntensityId, settings.scanlineIntensity);
            crtMaterial.SetFloat(ScanlineCountId, settings.scanlineCount);
            crtMaterial.SetFloat(CurvatureId, settings.curvature);
            crtMaterial.SetFloat(VignetteIntensityId, settings.vignetteIntensity);
            crtMaterial.SetFloat(ChromaticAberrationId, settings.chromaticAberration);
            crtMaterial.SetFloat(BrightnessId, settings.brightness);
            crtMaterial.SetFloat(FlickerId, settings.flicker);

            renderer.EnqueuePass(renderPass);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && crtMaterial != null)
            {
                CoreUtils.Destroy(crtMaterial);
            }
        }

        public void SetEnabled(bool enabled) => settings.enabled = enabled;
        public void SetScanlineIntensity(float intensity) => settings.scanlineIntensity = Mathf.Clamp01(intensity);
        public void SetCurvature(float curvature) => settings.curvature = Mathf.Clamp(curvature, 0f, 0.5f);
        public void SetChromaticAberration(float amount) => settings.chromaticAberration = Mathf.Clamp(amount, 0f, 5f);

        class CRTRenderPass : ScriptableRenderPass
        {
            private Material material;

            private class PassData
            {
                public TextureHandle source;
                public Material material;
            }

            public CRTRenderPass(Material material)
            {
                this.material = material;
                profilingSampler = new ProfilingSampler("CRT");
                requiresIntermediateTexture = true;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null)
                    return;

                var resourceData = frameData.Get<UniversalResourceData>();

                if (resourceData.isActiveTargetBackBuffer)
                    return;

                var source = resourceData.activeColorTexture;

                var destinationDesc = renderGraph.GetTextureDesc(source);
                destinationDesc.name = "_CRTTexture";
                destinationDesc.clearBuffer = false;

                TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("CRT Pass", out var passData, profilingSampler))
                {
                    passData.source = source;
                    passData.material = material;

                    builder.UseTexture(source, AccessFlags.Read);
                    builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                    });
                }

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("CRT Copy Back", out var passData, profilingSampler))
                {
                    passData.source = destination;

                    builder.UseTexture(destination, AccessFlags.Read);
                    builder.SetRenderAttachment(source, 0, AccessFlags.Write);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                    });
                }
            }

            [System.Obsolete]
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (material == null)
                    return;

                var cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, profilingSampler))
                {
                    var cameraData = renderingData.cameraData;
                    var source = cameraData.renderer.cameraColorTargetHandle;

                    var descriptor = cameraData.cameraTargetDescriptor;
                    descriptor.depthBufferBits = 0;

                    cmd.GetTemporaryRT(Shader.PropertyToID("_TempCRTTarget"), descriptor);
                    var tempTarget = new RenderTargetIdentifier("_TempCRTTarget");

                    cmd.SetGlobalTexture("_BlitTexture", source);
                    cmd.Blit(source, tempTarget, material, 0);
                    cmd.Blit(tempTarget, source);

                    cmd.ReleaseTemporaryRT(Shader.PropertyToID("_TempCRTTarget"));
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}
