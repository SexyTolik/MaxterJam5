using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

/// <summary>
/// Контроллер перемещения персонажа по клику мыши в 3D.
/// Требует компонент NavMeshAgent на этом же объекте
/// и запечённый NavMesh на сцене (Window -> AI -> Navigation -> Bake).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMoveController : MonoBehaviour
{
    [Header("Настройки клика")]
    [Tooltip("Камера, с которой пускаем луч. Если не указана — берётся Camera.main")]
    [SerializeField] private Camera cam;

    [Tooltip("Слои, по которым можно кликать для перемещения (обычно земля/пол)")]
    [SerializeField] private LayerMask groundLayer = ~0;

    [Tooltip("Максимальная дистанция луча от камеры")]
    [SerializeField] private float rayDistance = 200f;

    [Header("Индикатор клика (опционально)")]
    [Tooltip("Префаб маркера, который будет появляться в точке клика")]
    [SerializeField] private GameObject clickMarkerPrefab;

    [Tooltip("Через сколько секунд маркер исчезнет")]
    [SerializeField] private float markerLifetime = 1f;

    [Header("Анимация (опционально)")]
    [Tooltip("Aanimator персонажа, если нужно переключать состояние ходьбы")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParamName = "Speed";

    [Header("Стабилизация остановки")]
    [Tooltip("Дистанция до цели, при которой считаем, что агент доехал")]
    [SerializeField] private float arrivalThreshold = 0.05f;

    [Tooltip("Минимальная скорость, ниже которой считаем агента остановившимся и обнуляем его velocity вручную")]
    [SerializeField] private float velocitySnapThreshold = 0.02f;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (cam == null)
            cam = Camera.main;

        // ВАЖНО: если у Animator включён "Apply Root Motion", он и NavMeshAgent
        // одновременно двигают transform персонажа — это тоже вызывает дрожание/рябь.
        // Если используешь root motion, отключи agent.updatePosition/updateRotation
        // и синхронизируй агента с OnAnimatorMove() вместо этого.
        if (animator != null && animator.applyRootMotion)
        {
            Debug.LogWarning("ClickToMoveController: у Animator включён Apply Root Motion. " +
                "Это может конфликтовать с NavMeshAgent и вызывать дрожание персонажа. " +
                "Отключи Apply Root Motion либо перейди на управление через OnAnimatorMove().");
        }
    }

    private void Update()
    {
        HandleClickInput();
        HandleArrivalStop();
        UpdateAnimator();
    }

    private void HandleClickInput()
    {
        // ЛКМ — Input System не используется, чтобы не требовать доп. пакет.
        // Если у тебя подключён новый Input System — скажи, дам версию под него.
        if (!Input.GetMouseButtonDown(0))
            return;

        // Не двигаем персонажа, если сейчас открыт детальный просмотр объекта
        // (см. ObjectInspectionController) — иначе клик для закрытия просмотра
        // одновременно отправит персонажа в точку клика
        if (ObjectInspectionController.IsOpen)
            return;

        // Не двигаем персонажа, если кликнули по UI (кнопка, окно и т.д.)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            MoveTo(hit.point);
        }
    }

    private void MoveTo(Vector3 targetPoint)
    {
        // Проверяем, что точка лежит на NavMesh (или рядом с ним)
        if (NavMesh.SamplePosition(targetPoint, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
        {
            agent.isStopped = false; // на случай, если агент был "заглушен" после прошлого прибытия
            agent.SetDestination(navHit.position);
            SpawnClickMarker(navHit.position);
        }
    }

    private void SpawnClickMarker(Vector3 position)
    {
        if (clickMarkerPrefab == null)
            return;

        GameObject marker = Instantiate(clickMarkerPrefab, position, Quaternion.identity);
        Destroy(marker, markerLifetime);
    }

    private void HandleArrivalStop()
    {
        // Пока агент ещё в пути и путь не рассчитан — ничего не делаем
        if (agent.pathPending)
            return;

        bool closeToDestination = agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, arrivalThreshold);
        bool almostStopped = agent.velocity.sqrMagnitude < velocitySnapThreshold * velocitySnapThreshold;

        // Агент доехал, но NavMeshAgent сам по себе никогда не обнуляет velocity точно —
        // он асимптотически тормозит, из-за чего модель персонажа едва заметно
        // подрагивает на месте (те же микро-корректировки позиции каждый кадр).
        // Принудительно "глушим" агента, когда он практически прибыл и почти остановился.
        if (closeToDestination && almostStopped && !agent.pathPending)
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
        }
        else if (agent.hasPath && agent.isStopped)
        {
            // Если назначили новую цель — снова разрешаем движение
            agent.isStopped = false;
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        float speed = agent.velocity.magnitude;
        animator.SetFloat(speedParamName, speed);
    }
}