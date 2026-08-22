using UnityEngine;

/// <summary>
/// Повесь этот компонент на любой объект в сцене, который должен открывать
/// детальный просмотр по клику (см. ObjectInspectionController).
/// Объект должен иметь Collider и находиться на слое, указанном
/// в поле Inspectable Layer контроллера просмотра.
/// </summary>
public class InspectableObject : MonoBehaviour
{
    [Tooltip("Детальная (более качественная/увеличенная) версия объекта для показа при клике. " +
             "Если не указана — будет создана копия текущего объекта.")]
    [SerializeField] private GameObject detailPrefab;

    [Tooltip("Опционально: масштаб, в котором детальная версия будет показана перед камерой")]
    [SerializeField] private Vector3 detailScale = Vector3.one;

    public GameObject DetailPrefab => detailPrefab;
    public Vector3 DetailScale => detailScale;
}
