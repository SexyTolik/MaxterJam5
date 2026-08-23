using UnityEngine;

public class UIAnims : MonoBehaviour
{
    public GameObject Journal;
    public GameObject JournalOpenTrasform;
    public GameObject JournalCloseTransfor;
    public void OpenJournal()
    {
        Journal.LeanMove(JournalOpenTrasform.transform.position,1f);
    }
    public void CloseJournal()
    {
        Journal.LeanMove(JournalCloseTransfor.transform.position,1f);
    }
}
