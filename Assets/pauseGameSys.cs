using UnityEngine;
using UnityEngine.SceneManagement;

public class pauseGameSys : MonoBehaviour
{
    public GameObject pauseWindow;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseWindow.SetActive(true);
        }   
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
