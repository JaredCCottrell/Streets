Shader "Streets/PostProcess/CRT"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            Name "CRT"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Effect parameters
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _Curvature;
            float _VignetteIntensity;
            float _ChromaticAberration;
            float _Brightness;
            float _Flicker;

            // Apply barrel distortion for CRT curvature
            float2 CurveUV(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = uv.yx * uv.yx * _Curvature;
                uv = uv + uv * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            // Check if UV is outside screen bounds
            float IsOutsideScreen(float2 uv)
            {
                float2 outside = step(0.0, uv) * step(uv, 1.0);
                return outside.x * outside.y;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                // Apply screen curvature
                float2 curvedUV = CurveUV(uv);

                // Check if we're outside the curved screen area
                float insideScreen = IsOutsideScreen(curvedUV);

                // Chromatic aberration - separate RGB channels slightly
                float2 caOffset = (curvedUV - 0.5) * _ChromaticAberration * 0.01;
                half r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, curvedUV + caOffset).r;
                half g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, curvedUV).g;
                half b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, curvedUV - caOffset).b;
                half4 color = half4(r, g, b, 1.0);

                // Scanlines
                float scanline = sin(curvedUV.y * _ScanlineCount * 3.14159) * 0.5 + 0.5;
                scanline = pow(scanline, 1.5);
                scanline = lerp(1.0, scanline, _ScanlineIntensity);
                color.rgb *= scanline;

                // Horizontal scanline variation (phosphor simulation)
                float phosphor = sin(curvedUV.x * _BlitTexture_TexelSize.z * 3.14159 * 2.0) * 0.02 + 1.0;
                color.rgb *= phosphor;

                // Vignette
                float2 vignetteUV = curvedUV * (1.0 - curvedUV);
                float vignette = vignetteUV.x * vignetteUV.y * 15.0;
                vignette = pow(vignette, _VignetteIntensity);
                color.rgb *= vignette;

                // Brightness adjustment
                color.rgb *= _Brightness;

                // Flicker (subtle brightness variation)
                float flicker = 1.0 + sin(_Time.y * 60.0) * _Flicker * 0.03;
                color.rgb *= flicker;

                // Black out areas outside the curved screen
                color.rgb *= insideScreen;

                return color;
            }
            ENDHLSL
        }
    }
}
