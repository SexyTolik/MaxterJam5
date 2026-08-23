using System;
using System.Collections.Generic;
using ComputerSystem;
using TMPro;
using UnityEngine;

namespace PasswordSystem
{
    public class PasswordWindow : WindowView
    {
        public event Action OnSuccess;
        
        [SerializeField] private List<string> _passwords = new ();
        [SerializeField] private TMP_InputField _passwordInput;

        private bool _passwordEntered = false;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _passwordInput.onValueChanged.AddListener(CheckPassword);
        }
        protected override void OnDisable()
        {
            base.OnEnable();
            _passwordInput.onValueChanged.RemoveListener(CheckPassword);
        }

        public override void Show()
        {
            if (_passwordEntered)
            {
                OnSuccess?.Invoke();
                return;
            }
            base.Show();
        }
        
        private void CheckPassword(string input)
        {
            foreach (var password in _passwords)
            {
                if (input == password)
                {
                    _passwordEntered =  true;
                    OnSuccess?.Invoke();
                    return;
                }
            }
        }
    }
}