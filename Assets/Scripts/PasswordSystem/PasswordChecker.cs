using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PasswordSystem
{
    public class PasswordChecker : MonoBehaviour
    {
        public event Action OnSuccess;
        
        [SerializeField] private List<string> _passwords = new ();
        [SerializeField] private TMP_InputField _passwordInput;

        private void OnEnable()
        {
            _passwordInput.onValueChanged.AddListener(CheckPassword);
        }
        private void OnDisable()
        {
            _passwordInput.onValueChanged.RemoveListener(CheckPassword);
        }

        private void CheckPassword(string input)
        {
            foreach (var password in _passwords)
            {
                if (input == password)
                {
                    OnSuccess?.Invoke();
                    return;
                }
            }
        }
    }
}