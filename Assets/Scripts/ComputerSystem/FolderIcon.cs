using System.Collections.Generic;
using UnityEngine;

namespace ComputerSystem
{
    public class FolderIcon : ComputerIcon
    {
        public List<ComputerIcon> Files => _files;
        
        [SerializeField] private List<ComputerIcon> _files = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            _window.OnClosed += OnWindowClosedHandle;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            _window.OnClosed -= OnWindowClosedHandle;
        }

        private void OnWindowClosedHandle(WindowView window)
        {
            foreach (var icon in _files)
            {
                icon.Window.Hide();
            }
        }
    }
}