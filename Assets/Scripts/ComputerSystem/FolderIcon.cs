using System.Collections.Generic;
using UnityEngine;

namespace ComputerSystem
{
    public class FolderIcon : ComputerIcon
    {
        public List<ComputerIcon> Icons => _icons;
        
        [SerializeField] private List<ComputerIcon> _icons = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            _windowView.OnClosed += OnWindowClosedHandle;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            _windowView.OnClosed -= OnWindowClosedHandle;
        }

        private void OnWindowClosedHandle(WindowView window)
        {
            foreach (var icon in _icons)
            {
                icon.Window.Hide();
            }
        }
    }
}