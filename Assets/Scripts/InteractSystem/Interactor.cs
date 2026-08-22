using UnityEngine;

namespace InteractSystem
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private LayerMask _interactableLayer;
        [SerializeField] private float _maxDistance = 5f;
    
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) TryInteract();
        }

        private void TryInteract()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _interactableLayer))
            {
                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    MovePlayerToInteractable();
                    
                    interactable.Interact();
                }
            }
        }

        private void MovePlayerToInteractable()
        {
                
        }
    }
}
