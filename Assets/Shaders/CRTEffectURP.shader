Shader "Hidden/URP/CRTEffect"
{
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
            #pragma vertex Vert
            #pragma fragment Frag

            // No #include statements at all - this makes the shader fully self-contained
            // and immune to Packages/ virtual path resolution issues.

            Texture2D _BlitTexture;
            SamplerState sampler_LinearClamp;

            // Unity binds this global uniform automatically every frame as long as
            // a variable with this exact name/type is declared - no include needed.
            // _Time.y = time in seconds since level load.
            float4 _Time;

            float _Curvature;
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _VignetteIntensity;
            float _ChromaticAberration;
            float _NoiseIntensity;
            float _Brightness;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord   : TEXCOORD0;
            };

            // Build a full-screen triangle manually from the vertex index (0, 1, 2) -
            // this is exactly how Blitter.BlitTexture draws it via DrawProcedural,
            // with no real mesh bound.
            Varyings Vert(Attributes input)
            {
                Varyings output;

                float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
                output.positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);

                // Flip Y - without Core RP's UNITY_UV_STARTS_AT_TOP handling,
                // the render texture ends up upside down on this platform/graphics API
                output.texcoord = float2(uv.x, 1.0 - uv.y);

                return output;
            }

            // Simple pseudo-random noise generator based on UV
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // Barrel-style screen curvature, like a convex CRT tube
            float2 CurveUV(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / max(_Curvature, 0.001);
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = CurveUV(input.texcoord);

                // Outside the curved screen area - black border (like the edge of a CRT monitor)
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return float4(0.0, 0.0, 0.0, 1.0);
                }

                // Chromatic aberration - R/G/B channels slightly offset from each other
                float2 caOffset = (uv - 0.5) * _ChromaticAberration;
                float r = _BlitTexture.Sample(sampler_LinearClamp, uv - caOffset).r;
                float g = _BlitTexture.Sample(sampler_LinearClamp, uv).g;
                float b = _BlitTexture.Sample(sampler_LinearClamp, uv + caOffset).b;
                float3 col = float3(r, g, b);

                // Horizontal scanlines
                float scanline = sin(uv.y * _ScanlineCount * 3.14159265 * 2.0) * 0.5 + 0.5;
                col *= lerp(1.0, scanline, _ScanlineIntensity);

                // Vignette (darkening towards screen edges)
                float2 vignetteUV = uv * (1.0 - uv.yx);
                float vignette = vignetteUV.x * vignetteUV.y * 15.0;
                vignette = saturate(pow(vignette, _VignetteIntensity));
                col *= vignette;

                // Grain/noise, changing every frame
                float noise = (random(uv + frac(_Time.y)) - 0.5) * _NoiseIntensity;
                col += noise;

                col *= _Brightness;

                return float4(saturate(col), 1.0);
            }
            ENDHLSL
        }
    }
}
