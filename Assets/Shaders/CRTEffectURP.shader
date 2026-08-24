Shader "Hidden/URP/CRTEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "CRTPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _Curvature;
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _VignetteIntensity;
            float _ChromaticAberration;
            float _NoiseIntensity;
            float _Brightness;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            // Простой генератор псевдослучайного шума на основе UV
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // Искривление экрана "бочкой", как у выпуклой ЭЛТ-трубки
            float2 CurveUV(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / max(_Curvature, 0.001);
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = CurveUV(IN.uv);

                // За пределами искривлённого экрана — чёрная рамка (как край ЭЛТ-монитора)
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return float4(0.0, 0.0, 0.0, 1.0);
                }

                // Хроматическая аберрация — каналы R/G/B слегка смещены друг относительно друга
                float2 caOffset = (uv - 0.5) * _ChromaticAberration;
                float r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - caOffset).r;
                float g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).g;
                float b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + caOffset).b;
                float3 col = float3(r, g, b);

                // Горизонтальные scanlines
                float scanline = sin(uv.y * _ScanlineCount * 3.14159265 * 2.0) * 0.5 + 0.5;
                col *= lerp(1.0, scanline, _ScanlineIntensity);

                // Виньетка (затемнение по краям экрана)
                float2 vignetteUV = uv * (1.0 - uv.yx);
                float vignette = vignetteUV.x * vignetteUV.y * 15.0;
                vignette = saturate(pow(vignette, _VignetteIntensity));
                col *= vignette;

                // Шум/зерно, меняющееся каждый кадр
                float noise = (random(uv + frac(_Time.y)) - 0.5) * _NoiseIntensity;
                col += noise;

                col *= _Brightness;

                return float4(saturate(col), 1.0);
            }
            ENDHLSL
        }
    }
}
