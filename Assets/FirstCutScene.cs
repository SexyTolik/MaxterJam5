using UnityEngine;

public class FirstCutScene : MonoBehaviour
{
    [Header("Камера")]
    public Camera MainCamera;

    [Header("Параметры анимации")]
    [Tooltip("Целевой orthographic size, к которому анимируется камера")]
    public float targetSize = 7.69f;

    [Tooltip("Длительность анимации в секундах")]
    public float duration = 1f;

    [Tooltip("Тип интерполяции (easing) для LeanTween")]
    public LeanTweenType easeType = LeanTweenType.easeInOutQuad;

    private void Start()
    {
        if (MainCamera == null)
        {
            Debug.LogWarning("FirstCutScene: MainCamera не назначена в инспекторе.");
            return;
        }

        float startSize = MainCamera.orthographicSize;

        // LeanTween.value анимирует произвольное float-значение от startSize до targetSize
        // и на каждом шаге через setOnUpdate применяет его к orthographicSize камеры.
        // Так можно использовать любой easeType, а не только линейное приращение вручную.
        LeanTween.value(gameObject, startSize, targetSize, duration)
            .setEase(easeType)
            .setOnUpdate((float value) =>
            {
                MainCamera.orthographicSize = value;
            })
            .setOnComplete(() =>
            {
                Destroy(this);
            });
    }
}