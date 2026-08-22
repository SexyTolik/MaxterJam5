// PSX-стиль пост-эффект для URP: понижение разрешения (пиксельный низкополигональный
// вид, как на PS1) + ordered dithering с несколькими паттернами на выбор + снижение
// глубины цвета (квантование каналов, как у PSX 15-битного цвета).
//
// ПОДКЛЮЧЕНИЕ (Unity 6 / URP 14+):
//  1. Universal Renderer Data -> Add Renderer Feature -> "Full Screen Pass Renderer Feature"
//  2. В настройках Feature -> Pass Material -> создай материал с этим шейдером и назначь
//  3. Injection Point -> After Rendering Post Processing (эффект применяется последним)
//
// Свойства настраиваются либо прямо в материале, либо через PSXDitherController.cs
// (даёт удобный dropdown для выбора режима дизеринга вместо ручного ввода числа).
Shader "Hidden/PSX/PSXDither"
{
    Properties
    {
        [IntRange] _TargetResolutionY ("Target Vertical Resolution (PSX ~ 240)", Range(60, 480)) = 240

        [IntRange] _DitherMode ("Dither Mode (0=None 1=Bayer2x2 2=Bayer4x4 3=Bayer8x8 4=WhiteNoise)", Range(0, 4)) = 2
        _DitherStrength ("Dither Strength", Range(0, 2)) = 1

        [IntRange] _ColorLevels ("Color Levels Per Channel (PSX ~ 32)", Range(2, 256)) = 32
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "PSXDitherPass"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            // Blit.hlsl предоставляет: структуру Varyings (position, texcoord),
            // вершинный шейдер Vert(), текстуру _BlitTexture и sampler_LinearClamp/PointClamp.

            CBUFFER_START(UnityPerMaterial)
                float _TargetResolutionY;
                float _DitherMode;
                float _DitherStrength;
                float _ColorLevels;
            CBUFFER_END

            // ---------- Bayer-матрицы (ordered dithering) ----------
            // Значения нормализованы в диапазон [0, 1), классические ordered-dither таблицы.

            static const float Bayer2x2[4] =
            {
                0.0 / 4.0, 2.0 / 4.0,
                3.0 / 4.0, 1.0 / 4.0
            };

            static const float Bayer4x4[16] =
            {
                0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                12.0/16.0, 4.0/16.0, 14.0/16.0,  6.0/16.0,
                3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
                15.0/16.0, 7.0/16.0, 13.0/16.0,  5.0/16.0
            };

            static const float Bayer8x8[64] =
            {
                 0.0/64.0, 32.0/64.0,  8.0/64.0, 40.0/64.0,  2.0/64.0, 34.0/64.0, 10.0/64.0, 42.0/64.0,
                48.0/64.0, 16.0/64.0, 56.0/64.0, 24.0/64.0, 50.0/64.0, 18.0/64.0, 58.0/64.0, 26.0/64.0,
                12.0/64.0, 44.0/64.0,  4.0/64.0, 36.0/64.0, 14.0/64.0, 46.0/64.0,  6.0/64.0, 38.0/64.0,
                60.0/64.0, 28.0/64.0, 52.0/64.0, 20.0/64.0, 62.0/64.0, 30.0/64.0, 54.0/64.0, 22.0/64.0,
                 3.0/64.0, 35.0/64.0, 11.0/64.0, 43.0/64.0,  1.0/64.0, 33.0/64.0,  9.0/64.0, 41.0/64.0,
                51.0/64.0, 19.0/64.0, 59.0/64.0, 27.0/64.0, 49.0/64.0, 17.0/64.0, 57.0/64.0, 25.0/64.0,
                15.0/64.0, 47.0/64.0,  7.0/64.0, 39.0/64.0, 13.0/64.0, 45.0/64.0,  5.0/64.0, 37.0/64.0,
                63.0/64.0, 31.0/64.0, 55.0/64.0, 23.0/64.0, 61.0/64.0, 29.0/64.0, 53.0/64.0, 21.0/64.0
            };

            // Простой процедурный "белый шум" на основе целочисленных пиксельных координат —
            // не требует текстуры blue-noise, но менее равномерный визуально, чем Bayer.
            float WhiteNoise(uint2 pixelCoord)
            {
                float2 seed = float2(pixelCoord) + 0.5;
                return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
            }

            float GetDitherValue(uint2 lowResPixelCoord, float mode)
            {
                if (mode < 0.5) // None
                {
                    return 0.5; // нейтральное значение — дизеринг фактически не влияет
                }
                else if (mode < 1.5) // Bayer 2x2
                {
                    uint2 c = lowResPixelCoord % 2;
                    return Bayer2x2[c.y * 2 + c.x];
                }
                else if (mode < 2.5) // Bayer 4x4
                {
                    uint2 c = lowResPixelCoord % 4;
                    return Bayer4x4[c.y * 4 + c.x];
                }
                else if (mode < 3.5) // Bayer 8x8
                {
                    uint2 c = lowResPixelCoord % 8;
                    return Bayer8x8[c.y * 8 + c.x];
                }
                else // White Noise
                {
                    return WhiteNoise(lowResPixelCoord);
                }
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 screenSize = _ScreenParams.xy;

                // Считаем размер "пикселя" PSX в реальных экранных пикселях, исходя
                // из желаемого вертикального разрешения (аналог рендера в низком
                // разрешении с последующим point-filtered апскейлом).
                float pixelBlockSize = max(1.0, screenSize.y / max(1.0, _TargetResolutionY));

                // Снаппим UV на низкоразрешённую сетку — эффект пикселизации.
                float2 lowResScreenSize = floor(screenSize / pixelBlockSize);
                float2 lowResPixelCoordF = floor(IN.texcoord * lowResScreenSize);
                float2 snappedUV = (lowResPixelCoordF + 0.5) / lowResScreenSize;

                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, snappedUV);

                // Дизеринг считаем по координатам НИЗКОГО разрешения — так паттерн
                // остаётся "чётким" на каждом крупном PSX-пикселе, а не размывается
                // экранным разрешением.
                uint2 lowResPixelCoord = uint2(lowResPixelCoordF);
                float dither = GetDitherValue(lowResPixelCoord, _DitherMode);

                float levels = max(1.0, _ColorLevels - 1.0);

                // Ordered dithering: смещаем цвет шумом ПЕРЕД квантованием, чтобы
                // избежать резких цветовых полос (banding) при малом числе уровней.
                float3 ditheredColor = color.rgb + (dither - 0.5) * (_DitherStrength / levels);
                float3 quantized = floor(ditheredColor * levels + 0.5) / levels;

                return half4(saturate(quantized), color.a);
            }
            ENDHLSL
        }
    }
}
