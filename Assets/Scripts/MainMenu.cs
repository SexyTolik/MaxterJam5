using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _quitButton;

    private void OnEnable()
    {
        _playButton.onClick.AddListener(StartGame);
        _quitButton.onClick.AddListener(QuitGame);
        
    }

    private void OnDisable()
    {
        _playButton.onClick.RemoveListener(StartGame);
        _quitButton.onClick.RemoveListener(QuitGame);
    }

    private void StartGame()
    {
        SceneManager.LoadScene("Room");
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
