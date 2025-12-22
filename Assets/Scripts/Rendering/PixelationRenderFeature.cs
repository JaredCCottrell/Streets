using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Streets.Rendering
{
    /// <summary>
    /// URP Renderer Feature that applies a pixelation post-process effect.
    /// Add this to your URP Renderer asset to enable the effect.
    /// </summary>
    public class PixelationRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class PixelationSettings
        {
            [Tooltip("When to apply the pixelation effect in the render pipeline")]
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

            [Tooltip("Size of each pixel block. Higher = more pixelated")]
            [Range(1, 32)]
            public int pixelSize = 4;

            [Tooltip("Enable/disable the effect")]
            public bool enabled = true;
        }

        public PixelationSettings settings = new PixelationSettings();

        private PixelationRenderPass renderPass;
        private Material pixelationMaterial;

        private static readonly int PixelSizeProperty = Shader.PropertyToID("_PixelSize");

        public override void Create()
        {
            // Create material from shader
            var shader = Shader.Find("Streets/PostProcess/Pixelation");
            if (shader == null)
            {
                Debug.LogError("Pixelation shader not found! Make sure 'Streets/PostProcess/Pixelation' shader exists.");
                return;
            }

            pixelationMaterial = CoreUtils.CreateEngineMaterial(shader);
            renderPass = new PixelationRenderPass(pixelationMaterial);
            renderPass.renderPassEvent = settings.renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.enabled || pixelationMaterial == null)
                return;

            pixelationMaterial.SetFloat(PixelSizeProperty, settings.pixelSize);
            renderer.EnqueuePass(renderPass);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && pixelationMaterial != null)
            {
                CoreUtils.Destroy(pixelationMaterial);
            }
        }

        /// <summary>
        /// Set pixel size at runtime
        /// </summary>
        public void SetPixelSize(int size)
        {
            settings.pixelSize = Mathf.Clamp(size, 1, 32);
        }

        /// <summary>
        /// Enable or disable the effect at runtime
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            settings.enabled = enabled;
        }

        class PixelationRenderPass : ScriptableRenderPass
        {
            private Material material;
            private static readonly int BlitTexturePropertyId = Shader.PropertyToID("_BlitTexture");

            // RenderGraph pass data
            private class PassData
            {
                public TextureHandle source;
                public Material material;
            }

            public PixelationRenderPass(Material material)
            {
                this.material = material;
                profilingSampler = new ProfilingSampler("Pixelation");
                requiresIntermediateTexture = true;
            }

            // New RenderGraph API
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null)
                    return;

                var resourceData = frameData.Get<UniversalResourceData>();
                var cameraData = frameData.Get<UniversalCameraData>();

                if (resourceData.isActiveTargetBackBuffer)
                    return;

                var source = resourceData.activeColorTexture;

                var destinationDesc = renderGraph.GetTextureDesc(source);
                destinationDesc.name = "_PixelationTexture";
                destinationDesc.clearBuffer = false;

                TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation Pass", out var passData, profilingSampler))
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

                // Copy back to active color texture
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation Copy Back", out var passData, profilingSampler))
                {
                    passData.source = destination;
                    passData.material = null;

                    builder.UseTexture(destination, AccessFlags.Read);
                    builder.SetRenderAttachment(source, 0, AccessFlags.Write);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                    });
                }
            }

            // Legacy API fallback
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

                    cmd.GetTemporaryRT(Shader.PropertyToID("_TempPixelationTarget"), descriptor);
                    var tempTarget = new RenderTargetIdentifier("_TempPixelationTarget");

                    cmd.SetGlobalTexture(BlitTexturePropertyId, source);
                    cmd.Blit(source, tempTarget, material, 0);
                    cmd.Blit(tempTarget, source);

                    cmd.ReleaseTemporaryRT(Shader.PropertyToID("_TempPixelationTarget"));
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}
