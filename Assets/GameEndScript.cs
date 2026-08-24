using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEndScript : MonoBehaviour
{
    public Image blackScreen;
    public GameObject endText;
    public void ENDTHEGAME()
    {
        StartCoroutine(gameEnd());
    }

    public IEnumerator gameEnd()
    {
        yield return new WaitForSeconds(15f);
        blackScreen.gameObject.SetActive(true);
       
        yield return new WaitForSeconds(2f);
        endText.SetActive(true);
        int i = 5;
        while (i >= 0)
        {
            i--;
            yield return new WaitForSeconds(1f);
        }
        SceneManager.LoadScene(0);
    }
}
