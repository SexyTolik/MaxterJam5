using PasswordSystem;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerSystem
{
    public class ComputerIcon : MonoBehaviour
    {
        public WindowView Window => _window;
        
        [SerializeField] private Button _button;
        [SerializeField] protected WindowView _window;
        [SerializeField] private PasswordWindow _passwordWindow;
        
        private void Awake()
        {
            if(_passwordWindow) _passwordWindow.Hide();
            _window.Hide();
        }

        protected virtual void OnEnable()
        {
            _button.onClick.AddListener(OpenWindow);
            if(_passwordWindow) _passwordWindow.OnSuccess += OnPasswordEnteredHandle;
        }
        protected virtual void OnDisable()
        {
            _button.onClick.RemoveListener(OpenWindow);
            if(_passwordWindow) _passwordWindow.OnSuccess -= OnPasswordEnteredHandle;
        }

        private void OpenWindow()
        {
            if (_passwordWindow)
            {
                _passwordWindow.Show();
                return;
            }
            
            _window.Show();
        }
        private void OnPasswordEnteredHandle()
        {
            _passwordWindow.Hide();
            _window.Show();
        }
    }
}