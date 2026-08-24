using PasswordSystem;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerSystem
{
    public class ComputerView : MonoBehaviour
    {
        [SerializeField] private PasswordWindow _passwordWindow;
        [SerializeField] private Button _closeButton;

        public JournalUIController journal;

        private void OnEnable()
        {
            _passwordWindow.OnSuccess += OnPasswordEnteredHandle;
            _closeButton.onClick.AddListener(Hide);
        }
        private void OnDisable()
        {
            _passwordWindow.OnSuccess -= OnPasswordEnteredHandle;
            _closeButton.onClick.RemoveListener(Hide);
        }

        public void Show()
        {
            _passwordWindow.Show();
            gameObject.SetActive(true);
        }
        private void Hide()
        {
            _passwordWindow.Hide();
            gameObject.SetActive(false);
        }
        
        private void OnPasswordEnteredHandle()
        {
            _passwordWindow.Hide();
            if(journal != null)
            {
                journal.PCUnloked = true;
            }
        }

        /*private void AddIcons(List<ComputerIcon> icons)
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
                    AddIcons(folderIcon.Files);
                }
            }
        }*/
    }
}