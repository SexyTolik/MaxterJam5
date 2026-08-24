using Unity.Burst.CompilerServices;
using UnityEngine;

public class JournalPage : MonoBehaviour
{
    public int PageID = -1;
    [SerializeField]
    private GameObject Hint;
    [SerializeField]
    private GameObject Zarisovka;

    public AudioClip ZarisovkaSound;
    public bool ZarisovkaISComplete = false;

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
        if (!ZarisovkaISComplete)
        {
         ZarisovkaISComplete = true;
         Zarisovka.SetActive(true);
         SoundFXManager.instance.PlayAudioClip(ZarisovkaSound);
        }
    }
}
