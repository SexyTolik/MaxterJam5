using UnityEngine;

public class JournalPage : MonoBehaviour
{
    public int PageID = -1;
    [SerializeField]
    private GameObject Hint;
    [SerializeField]
    private GameObject Zarisovka;

    void Awake()
    {
        Hint.SetActive(false);
        Zarisovka.SetActive(false);
    }

    public void ShowHint()
    {
        Hint.SetActive(true);
    }

    public void ShowZarisovka()
    {
        Zarisovka.SetActive(true);
    }
}
