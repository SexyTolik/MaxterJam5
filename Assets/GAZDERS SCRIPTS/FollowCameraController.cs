using UnityEngine;

/// <summary>
/// Камера следует за персонажем с небольшой задержкой (плавное сглаживание),
/// приближается/отдаляется колесиком мыши и слегка смещает точку обзора
/// в сторону курсора, когда игрок уводит мышь к краю экрана —
/// это расширяет "видимую зону" в направлении, куда смотрит игрок.
/// </summary>
public class FollowCameraController : MonoBehaviour
{
    [Header("Цель")]
    [Tooltip("Персонаж, за которым следует камера")]
    [SerializeField] private Transform target;

    [Header("Следование")]
    [Tooltip("Базовое смещение камеры от цели (в локальных координатах цели)")]
    [SerializeField] private Vector3 baseOffset = new Vector3(0f, 10f, -8f);

    [Tooltip("Время сглаживания движения камеры. Больше значение — больше задержка")]
    [SerializeField] private float followSmoothTime = 0.25f;

    [Header("Зум колесиком мыши (Orthographic Size)")]
    [Tooltip("Насколько сильно один 'тик' колёсика меняет orthographic size")]
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float minOrthoSize = 3f;
    [SerializeField] private float maxOrthoSize = 15f;

    [Tooltip("Скорость сглаживания зума")]
    [SerializeField] private float zoomSmoothTime = 0.15f;

    [Header("Смещение обзора к мыши (look ahead)")]
    [Tooltip("Включить смещение камеры в сторону, куда игрок уводит мышь")]
    [SerializeField] private bool enableMouseLookAhead = true;

    [Tooltip("Максимальное смещение камеры по каждой оси при look ahead")]
    [SerializeField] private float maxLookAheadOffset = 4f;

    [Tooltip("От какой доли расстояния от центра экрана (0-1) начинает работать эффект")]
    [Range(0f, 0.9f)]
    [SerializeField] private float lookAheadDeadZone = 0.2f;

    [Tooltip("Скорость сглаживания смещения обзора")]
    [SerializeField] private float lookAheadSmoothTime = 0.3f;

    [Header("Стабилизация (защита от ряби)")]
    [Tooltip("Если камере осталось сдвинуться меньше этого расстояния — позиция мгновенно защёлкивается, чтобы устранить бесконечное микро-дрожание SmoothDamp")]
    [SerializeField] private float snapThreshold = 0.001f;

    private Camera cam;

    // Фиксированный поворот камеры и её оси, вычисленные один раз в Start.
    // Это критично: если пересчитывать transform.right/up каждый кадр ПОСЛЕ LookAt,
    // возникает петля обратной связи поворот -> смещение -> поворот -> ..., которая
    // на ортографической камере даёт видимую рябь (shimmer) даже когда всё стоит на месте.
    private Quaternion fixedRotation;
    private Vector3 fixedRight;
    private Vector3 fixedUp;

    // Текущий и целевой orthographic size камеры
    private float currentOrthoSize;
    private float targetOrthoSize;
    private float zoomVelocity;

    // Сглаживание позиции камеры
    private Vector3 positionVelocity;

    // Сглаживание look ahead смещения
    private Vector2 currentLookAhead;
    private Vector2 lookAheadVelocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (target == null)
            Debug.LogWarning("FollowCameraController: цель (target) не назначена в инспекторе.");

        if (cam == null || !cam.orthographic)
            Debug.LogWarning("FollowCameraController: камера не в режиме Orthographic. Переключи Projection на Orthographic в компоненте Camera.");

        // Берём стартовый size из самой камеры, чтобы не было скачка при старте
        currentOrthoSize = cam != null ? cam.orthographicSize : 5f;
        targetOrthoSize = Mathf.Clamp(currentOrthoSize, minOrthoSize, maxOrthoSize);

        // Поворот камеры вычисляем ОДИН РАЗ на основе baseOffset и больше не трогаем.
        // Камера смотрит в направлении, противоположном offset (от камеры к цели).
        fixedRotation = Quaternion.LookRotation(-baseOffset.normalized, Vector3.up);
        transform.rotation = fixedRotation;

        fixedRight = fixedRotation * Vector3.right;
        fixedUp = fixedRotation * Vector3.up;
    }

    private void Update()
    {
        HandleZoomInput();
    }

    // Движение камеры делаем в LateUpdate, чтобы персонаж успел переместиться за этот кадр
    private void LateUpdate()
    {
        if (target == null)
            return;

        UpdateLookAhead();
        UpdateCameraPosition();
    }

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.0001f)
        {
            targetOrthoSize -= scroll * zoomSpeed;
            targetOrthoSize = Mathf.Clamp(targetOrthoSize, minOrthoSize, maxOrthoSize);
        }

        currentOrthoSize = Mathf.SmoothDamp(currentOrthoSize, targetOrthoSize, ref zoomVelocity, zoomSmoothTime);

        if (cam != null)
            cam.orthographicSize = currentOrthoSize;
    }

    private void UpdateLookAhead()
    {
        Vector2 targetLookAhead = Vector2.zero;

        if (enableMouseLookAhead)
        {
            // Нормализованная позиция мыши от -1 до 1 относительно центра экрана
            Vector2 mouseNorm = new Vector2(
                (Input.mousePosition.x / Screen.width) * 2f - 1f,
                (Input.mousePosition.y / Screen.height) * 2f - 1f
            );

            targetLookAhead = ApplyDeadZone(mouseNorm) * maxLookAheadOffset;
        }

        currentLookAhead = Vector2.SmoothDamp(currentLookAhead, targetLookAhead, ref lookAheadVelocity, lookAheadSmoothTime);

        // Та же защёлкивание, что и для позиции — иначе даже неподвижная мышь
        // может давать остаточное микро-дрожание offset'а
        if ((targetLookAhead - currentLookAhead).sqrMagnitude < snapThreshold * snapThreshold)
        {
            currentLookAhead = targetLookAhead;
            lookAheadVelocity = Vector2.zero;
        }
    }

    private Vector2 ApplyDeadZone(Vector2 value)
    {
        float x = ApplyDeadZoneAxis(value.x);
        float y = ApplyDeadZoneAxis(value.y);
        return new Vector2(x, y);
    }

    private float ApplyDeadZoneAxis(float v)
    {
        float sign = Mathf.Sign(v);
        float abs = Mathf.Abs(v);

        if (abs < lookAheadDeadZone)
            return 0f;

        // Перенормируем оставшийся диапазон в 0..1, чтобы не было скачка на границе deadzone
        float remapped = (abs - lookAheadDeadZone) / (1f - lookAheadDeadZone);
        return sign * Mathf.Clamp01(remapped);
    }

    private void UpdateCameraPosition()
    {
        // Позиция камеры относительно цели всегда фиксирована — зум делается через orthographicSize,
        // а не через приближение/отдаление камеры в пространстве.
        // Используем ЗАКЭШИРОВАННЫЕ оси (fixedRight/fixedUp), а не transform.right/up —
        // иначе возникает петля обратной связи с поворотом камеры и рябь на ортографической камере.
        Vector3 lookAheadWorldOffset = fixedRight * currentLookAhead.x + fixedUp * currentLookAhead.y;

        Vector3 desiredPosition = target.position + baseOffset + lookAheadWorldOffset;

        // SmoothDamp никогда не сходится точно в ноль — на очень малых дистанциях
        // он даёт микро-дрожание каждый кадр. Если разница уже пренебрежимо мала,
        // просто "защёлкиваем" позицию и обнуляем скорость, чтобы камера полностью замерла.
        if ((desiredPosition - transform.position).sqrMagnitude < snapThreshold * snapThreshold)
        {
            transform.position = desiredPosition;
            positionVelocity = Vector3.zero;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref positionVelocity, followSmoothTime);
        }

        // Поворот камеры больше НЕ пересчитывается каждый кадр — он фиксирован в Start().
        // Это убирает рябь, вызванную вращением ортографической камеры на доли градуса.
        transform.rotation = fixedRotation;
    }
}