using System.Collections.Generic;
using UnityEngine;

public class JournalUIController : MonoBehaviour
{
    public List<GameObject> JournalLists = new List<GameObject>();
    [SerializeField] private GameObject currentJournal;


    void Awake()
    {
        Initilaze();
        currentJournal.SetActive(true);
    }

    private void Initilaze()
    {
        foreach(GameObject v in JournalLists)
        {
            v.SetActive(false);
        }
    }

    public void ResiveHintEvent(int rebusID)
    {
        foreach(var v in JournalLists)
        {
            JournalPage _page = v.GetComponent<JournalPage>();
            if(_page.PageID == rebusID)
            {
                _page.ShowHint();
            }
        }
    }

    public void NextPage()
    {
       int curindx = JournalLists.IndexOf(currentJournal);
       currentJournal.SetActive(false);
       curindx++;
       curindx = Mathf.Clamp(curindx,0,JournalLists.Count-1);
       currentJournal = JournalLists[curindx];
       currentJournal.SetActive(true);
    }
    public void PreciousPage()
    {
       int curindx = JournalLists.IndexOf(currentJournal);
       currentJournal.SetActive(false);
       curindx--;
       curindx = Mathf.Clamp(curindx,0,JournalLists.Count);
       currentJournal = JournalLists[curindx];
       currentJournal.SetActive(true);
    }
}
