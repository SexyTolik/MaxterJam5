using System.Collections.Generic;
using UnityEngine;

public class JournalUIController : MonoBehaviour
{
    public List<GameObject> JournalLists = new List<GameObject>();
    [SerializeField] private GameObject currentJournal;
    public bool PCUnloked = false;
    public GameObject LABELNEW;

    public UIAnims anims;


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
                if (_page.isActiveAndEnabled)
                {
                    _page.ShowHint();
                }
                else
                {
                    v.SetActive(true);
                    _page.ShowHint();
                    v.SetActive(false);
                }
            }
        }
    }

    public void ResiveZarisovkaEvent(int rebusID)
    {
        if(!PCUnloked && rebusID > 0)
        {
            return;
        }
        foreach(var v in JournalLists)
        {
            JournalPage _page = v.GetComponent<JournalPage>();
            if(_page.PageID == rebusID)
            {
                if (_page.ZarisovkaISComplete)
                {
                    return;
                }
                if (_page.isActiveAndEnabled)
                {
                    _page.ShowZarisovka();
                }
                else
                {
                    v.SetActive(true);
                    _page.ShowZarisovka();
                    v.SetActive(false);
                }
                anims.MarkJournal();
                LABELNEW.SetActive(true);
            }
        }
    }

    public void NextPage()
    {
        if (!PCUnloked)
        {
            return;
        }
       int curindx = JournalLists.IndexOf(currentJournal);
       currentJournal.SetActive(false);
       curindx++;
       curindx = Mathf.Clamp(curindx,0,JournalLists.Count-1);
       currentJournal = JournalLists[curindx];
       currentJournal.SetActive(true);
    }
    public void PreciousPage()
    {
        if (!PCUnloked)
        {
            return;
        }
       int curindx = JournalLists.IndexOf(currentJournal);
       currentJournal.SetActive(false);
       curindx--;
       curindx = Mathf.Clamp(curindx,0,JournalLists.Count);
       currentJournal = JournalLists[curindx];
       currentJournal.SetActive(true);
    }
}
