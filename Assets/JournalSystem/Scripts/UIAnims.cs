using UnityEngine;

public class UIAnims : MonoBehaviour
{
    public GameObject Journal;
    public GameObject JournalOpenTrasform;
    public GameObject JournalCloseTransfor;

    [SerializeField] private float animationDuration = 0.15f;

    private Vector3 initialScale;
    void Start()
    {
        initialScale = Journal.transform.localScale;
    }
    public void OpenJournal()
    {
        Journal.LeanMove(JournalOpenTrasform.transform.position,1f);
    }
    public void CloseJournal()
    {
        Journal.LeanMove(JournalCloseTransfor.transform.position,1f);
    }

    public void MarkJournal()
    {

    transform.localScale = initialScale;

    LeanTween.scale(Journal.gameObject, initialScale * 1.15f, animationDuration)
        .setEaseOutBack()
        .setOnComplete(() =>
        {
            LeanTween.scale(Journal.gameObject, initialScale, animationDuration)
                .setEaseInBack();
        });
    }
}
