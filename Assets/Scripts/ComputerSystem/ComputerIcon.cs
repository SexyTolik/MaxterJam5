using UnityEngine;
using UnityEngine.UI;

namespace ComputerSystem
{
    public class ComputerIcon : MonoBehaviour
    {
        public WindowView Window => _windowView;
        
        [SerializeField] private Button _button;
        [SerializeField] protected WindowView _windowView;

        private void Awake()
        {
            _windowView.Hide();
        }

        protected virtual void OnEnable()
        {
            _button.onClick.AddListener(OpenWindow);
        }
        protected virtual void OnDisable()
        {
            _button.onClick.RemoveListener(OpenWindow);
        }

        private void OpenWindow()
        {
            _windowView.Show();
        }
    }
}