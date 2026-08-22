using System;
using TMPro;
using UnityEngine;

namespace ComputerSystem
{
    public class PasswordChecker : MonoBehaviour
    {
        public event Action OnSuccess;
        
        [SerializeField] private string _password;
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
            if(_password ==  input) OnSuccess?.Invoke();
        }
    }
}