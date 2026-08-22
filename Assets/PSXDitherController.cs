using UnityEngine;

/// <summary>
/// Удобная обёртка над материалом PSXDither.shader — даёт нормальный dropdown
/// для выбора режима дизеринга вместо ручного ввода числа в инспекторе материала.
///
/// Повесь на любой GameObject в сцене (например, на камеру) и перетащи материал,
/// созданный на основе Hidden/PSX/PSXDither, в поле targetMaterial.
/// </summary>
public class PSXDitherController : MonoBehaviour
{
    public enum DitherMode
    {
        None = 0,
        Bayer2x2 = 1,
        Bayer4x4 = 2,
        Bayer8x8 = 3,
        WhiteNoise = 4
    }

    [Header("Материал (Hidden/PSX/PSXDither)")]
    [SerializeField] private Material targetMaterial;

    [Header("Разрешение")]
    [Tooltip("Целевое вертикальное разрешение — классика PSX это 240 (реже 480 для 'высокого' режима PSX)")]
    [Range(60, 480)]
    [SerializeField] private int targetResolutionY = 240;

    [Header("Дизеринг")]
    [SerializeField] private DitherMode ditherMode = DitherMode.Bayer4x4;
    [Range(0f, 2f)]
    [SerializeField] private float ditherStrength = 1f;

    [Header("Цвет")]
    [Tooltip("Количество уровней квантования на канал. PSX ~ 32 (5 бит на канал)")]
    [Range(2, 256)]
    [SerializeField] private int colorLevels = 32;

    private static readonly int TargetResolutionYId = Shader.PropertyToID("_TargetResolutionY");
    private static readonly int DitherModeId = Shader.PropertyToID("_DitherMode");
    private static readonly int DitherStrengthId = Shader.PropertyToID("_DitherStrength");
    private static readonly int ColorLevelsId = Shader.PropertyToID("_ColorLevels");

    private void OnEnable()
    {
        ApplyToMaterial();
    }

    private void OnValidate()
    {
        ApplyToMaterial();
    }

    private void ApplyToMaterial()
    {
        if (targetMaterial == null) return;

        targetMaterial.SetFloat(TargetResolutionYId, targetResolutionY);
        targetMaterial.SetFloat(DitherModeId, (int)ditherMode);
        targetMaterial.SetFloat(DitherStrengthId, ditherStrength);
        targetMaterial.SetFloat(ColorLevelsId, colorLevels);
    }

    /// <summary>Переключить режим дизеринга из кода (например, по нажатию клавиши для сравнения режимов).</summary>
    public void SetDitherMode(DitherMode mode)
    {
        ditherMode = mode;
        ApplyToMaterial();
    }

    /// <summary>Переключить целевое разрешение из кода (например, слайдер настроек графики).</summary>
    public void SetTargetResolutionY(int resolutionY)
    {
        targetResolutionY = Mathf.Clamp(resolutionY, 60, 480);
        ApplyToMaterial();
    }
}
