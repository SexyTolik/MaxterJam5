using InteractSystem;
using UnityEngine;

namespace ComputerSystem
{
    public class Computer : MonoBehaviour, IInteractable
    {
        [SerializeField] private ComputerView _view;
        
        public void Interact() => _view.Show();
    }
}