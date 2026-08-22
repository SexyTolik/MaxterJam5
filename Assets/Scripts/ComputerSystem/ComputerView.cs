using System.Collections.Generic;
using UnityEngine;

namespace ComputerSystem
{
    public class ComputerView : MonoBehaviour
    {
        [SerializeField] private List<ComputerIcon> _icons = new();
        private HashSet<ComputerIcon> _iconSet = new();

        [SerializeField] private bool _isEscapeWorks;
        
        private List<WindowView> _openedWindows = new ();
        
        private void Awake()
        {
            AddIcons(_icons);
        }

        private void OnEnable()
        {
            foreach (var icon in _iconSet)
            {
                icon.Window.OnOpened += OnWindowOpenedHandle;
            }
        }
        private void OnDisable()
        {
            foreach (var icon in _iconSet)
            {
                icon.Window.OnOpened -= OnWindowOpenedHandle;
            }
        }
        
        private void OnWindowOpenedHandle(WindowView window)
        {
            _openedWindows.Add(window);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Update()
        {
            if(!_isEscapeWorks) return;
            if(_openedWindows.Count <= 0) return;
            
            /*if (Input.GetKeyDown(KeyCode.Escape))
            {
                _openedWindows[^0].Hide();
            }*/
        }

        private void AddIcons(List<ComputerIcon> icons)
        {
            foreach (var icon in icons)
            {
                AddIcon(icon);
            }
        }
        private void AddIcon(ComputerIcon icon)
        {
            if (_iconSet.Add(icon))
            {
                if (icon is FolderIcon folderIcon)
                {
                    AddIcons(folderIcon.Icons);
                }
            }
        }
    }
}