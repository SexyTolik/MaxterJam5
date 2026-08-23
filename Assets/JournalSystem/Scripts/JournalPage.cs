using UnityEngine;

public class JournalPage : MonoBehaviour
{
    public int PageID = -1;
    [SerializeField]
    private GameObject Hint;

    void Awake()
    {
        Hint.SetActive(false);
    }

    public void ShowHint()
    {
        Hint.SetActive(true);
    }
}
