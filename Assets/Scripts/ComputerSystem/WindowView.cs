using System;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerSystem
{
    public class WindowView : MonoBehaviour
    {
        public event Action<WindowView> OnClosed;
        public event Action<WindowView> OnOpened;
        
        [SerializeField] private Button _closeButton;
        
        private void OnEnable()
        {
            _closeButton.onClick.AddListener(Hide);
        }
        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(Hide);
        }

        public void Show()
        {
            if(gameObject.activeSelf) return;
            
            gameObject.SetActive(true);
            OnOpened?.Invoke(this);
        }
        public void Hide()
        {
            if(!gameObject.activeSelf) return;
            
            gameObject.SetActive(false);
            OnClosed?.Invoke(this);
        }
    }
}
