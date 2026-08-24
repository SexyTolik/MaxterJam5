using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// При клике на объект с компонентом InspectableObject:
/// 1) затемняет экран через UI Panel (CanvasGroup, анимация через LeanTween),
/// 2) показывает увеличенную/детальную версию объекта перед камерой
///    с анимацией появления через LeanTween.
/// Повторный клик (или Escape) закрывает просмотр.
/// </summary>
public class ObjectInspectionController : MonoBehaviour
{
    [Header("Клик по объекту")]
    [Tooltip("Камера, с которой пускаем луч. Если не указана — берётся Camera.main")]
    [SerializeField] private Camera cam;

    [Tooltip("Слой(и), на которых находятся объекты, доступные для детального просмотра")]
    [SerializeField] private LayerMask inspectableLayer;

    [Tooltip("Максимальная дистанция луча от камеры")]
    [SerializeField] private float rayDistance = 200f;

    [Header("Затемнение экрана (UI Panel)")]
    [Tooltip("CanvasGroup на UI Panel, которая будет затемнять экран. Panel должна быть растянута на весь экран (Image, чёрная, alpha изначально 0)")]
    [SerializeField] private CanvasGroup darkenPanel;

    [Tooltip("Итоговая прозрачность затемнения (0 - не видно, 1 - полностью чёрный экран)")]
    [Range(0f, 1f)]
    [SerializeField] private float darkenAlpha = 0.75f;

    [Tooltip("Длительность анимации затемнения/осветления")]
    [SerializeField] private float darkenDuration = 0.3f;

    [Header("Детальный просмотр объекта")]
    [Tooltip("Точка перед камерой, где будет появляться детальная версия объекта. " +
             "Рекомендуется сделать пустой дочерний объект камеры со смещением вперёд")]
    [SerializeField] private Transform detailAnchor;

    [Tooltip("Длительность анимации появления (увеличения) детальной версии")]
    [SerializeField] private float scaleInDuration = 0.4f;

    [Tooltip("Тип плавности анимации появления")]
    [SerializeField] private LeanTweenType scaleInEase = LeanTweenType.easeOutBack;

    [Tooltip("Тип плавности анимации закрытия")]
    [SerializeField] private LeanTweenType scaleOutEase = LeanTweenType.easeInBack;

    [Tooltip("Медленно вращать детальную версию объекта вокруг вертикальной оси")]
    [SerializeField] private bool autoRotate = true;

    [Tooltip("Скорость авто-вращения, градусов в секунду")]
    [SerializeField] private float autoRotateSpeed = 25f;

    [Tooltip("Игрок и минимальное растояние до объекта вращения")]
    [SerializeField] private ClickToMoveController Player;
    [SerializeField] private float MinDistance = 4f;

    [Tooltip("Текст описание предмета который осматриваем")]
    [SerializeField] private TMP_Text text;

    /// <summary>
    /// Открыт ли сейчас детальный просмотр. Используется другими скриптами
    /// (например, ClickToMoveController), чтобы не двигать персонажа, пока открыт просмотр.
    /// </summary>
    public static bool IsOpen { get; private set; }

    private GameObject currentDetailInstance;
    private bool isAnimating;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        if (darkenPanel != null)
        {
            darkenPanel.alpha = 0f;
            darkenPanel.blocksRaycasts = false;
            darkenPanel.interactable = false;
            text.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ObjectInspectionController: не назначен darkenPanel (CanvasGroup). Затемнение работать не будет.");
        }

        IsOpen = false;
    }

    private void Update()
    {
        if (IsOpen)
        {
            HandleCloseInput();
        }
        else
        {
            HandleOpenInput();
        }

        if (autoRotate && currentDetailInstance != null)
        {
            currentDetailInstance.transform.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime, Space.World);
        }
    }

    private void HandleOpenInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (isAnimating)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, inspectableLayer))
        {
            InspectableObject inspectable = hit.collider.GetComponentInParent<InspectableObject>();
            if(Vector3.Distance(inspectable.transform.position, Player.transform.position) > MinDistance)
            {
                Player.MoveTo(inspectable.transform.position);
                return;
            }

            if (inspectable != null)
            {
                Open(inspectable);
            }
        }
    }

    private void HandleCloseInput()
    {
        if (isAnimating)
            return;

        bool clickedToClose = Input.GetMouseButtonDown(0);
        bool pressedEscape = Input.GetKeyDown(KeyCode.Escape);

        if (clickedToClose || pressedEscape)
        {
            Close();
        }
    }

    private void Open(InspectableObject inspectable)
    {
        isAnimating = true;
        IsOpen = true;

        SpawnDetailInstance(inspectable);
        AnimateDarken(show: true);
        text.gameObject.SetActive(true);
        text.text = inspectable.Opisanie;
    }

    private void SpawnDetailInstance(InspectableObject inspectable)
    {
        GameObject prefabToShow = inspectable.DetailPrefab != null ? inspectable.DetailPrefab : inspectable.gameObject;

        if (detailAnchor == null)
        {
            Debug.LogWarning("ObjectInspectionController: не назначен detailAnchor. " +
                "Создай пустой дочерний объект у камеры со смещением вперёд и назначь его сюда.");
        }

        // Instantiate сразу с parent = detailAnchor — объект физически прикрепляется
        // к точке и будет двигаться вместе с ней (например, вместе с камерой),
        // а не просто один раз позиционируется в момент создания
        currentDetailInstance = detailAnchor != null
            ? Instantiate(prefabToShow, detailAnchor)
            : Instantiate(prefabToShow);

        // Обнуляем локальные позицию/поворот, чтобы объект встал ровно в точку анкора,
        // а не унаследовал произвольные локальные координаты префаба
        currentDetailInstance.transform.localPosition = Vector3.zero;
        currentDetailInstance.transform.localRotation = Quaternion.identity;

        // Отключаем коллайдеры у детальной копии, чтобы по ней случайно не попал raycast
        // персонажа или клика (иначе клик по детальной версии может, например, снова
        // запустить движение персонажа, если коллайдер попадёт под groundLayer)
        foreach (Collider col in currentDetailInstance.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        Vector3 targetScale = inspectable.DetailScale;
        currentDetailInstance.transform.localScale = Vector3.zero;

        LeanTween.scale(currentDetailInstance, targetScale, scaleInDuration)
            .setEase(scaleInEase)
            .setOnComplete(() => isAnimating = false);
    }

    private void Close()
    {
        isAnimating = true;

        if (currentDetailInstance != null)
        {
            GameObject instanceToDestroy = currentDetailInstance;
            currentDetailInstance = null;

            LeanTween.scale(instanceToDestroy, Vector3.zero, scaleInDuration)
                .setEase(scaleOutEase)
                .setOnComplete(() => Destroy(instanceToDestroy));
        }

        AnimateDarken(show: false);
        text.gameObject.SetActive(false);
    }

    private void AnimateDarken(bool show)
    {
        if (darkenPanel == null)
        {
            OnDarkenComplete(show);
            return;
        }

        float targetAlpha = show ? darkenAlpha : 0f;

        if (show)
        {
            darkenPanel.blocksRaycasts = true;
            darkenPanel.interactable = true;
        }

        // Используем LeanTween.value с ручным колбэком вместо LeanTween.alphaCanvas,
        // т.к. этот способ работает одинаково во всех версиях LeanTween
        LeanTween.value(darkenPanel.gameObject, darkenPanel.alpha, targetAlpha, darkenDuration)
            .setOnUpdate((float value) => darkenPanel.alpha = value)
            .setOnComplete(() => OnDarkenComplete(show));
    }

    private void OnDarkenComplete(bool wasShowing)
    {
        if (!wasShowing)
        {
            if (darkenPanel != null)
            {
                darkenPanel.blocksRaycasts = false;
                darkenPanel.interactable = false;
            }

            IsOpen = false;
        }

        isAnimating = false;
    }
}