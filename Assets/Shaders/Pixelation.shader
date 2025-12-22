Shader "Streets/PostProcess/Pixelation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Float) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "Pixelation"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _PixelSize;

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                // Calculate pixelated UV coordinates
                float2 pixelCount = _BlitTexture_TexelSize.zw / _PixelSize;
                float2 pixelatedUV = floor(uv * pixelCount) / pixelCount;

                // Add half pixel offset to sample from center of each "big pixel"
                pixelatedUV += (0.5 / pixelCount);

                // Sample the texture at the pixelated coordinates
                half4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, pixelatedUV);

                return color;
            }
            ENDHLSL
        }
    }
}
