using Unity.Mathematics;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
   public static SoundFXManager instance;
   [SerializeField] private AudioSource soundFXPrefab;

   public AudioClip passSound;
   public AudioClip click;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    public void PlayAudioClip(AudioClip audioClip, Transform position)
    {
        AudioSource source = Instantiate(soundFXPrefab,position.position,quaternion.identity);
        source.clip = audioClip;
        source.Play();
        float clipLenght = audioClip.length;
        Destroy(source,clipLenght);
    }
    public void PlayAudioClip(AudioClip audioClip, Vector3 position)
    {
        AudioSource source = Instantiate(soundFXPrefab,position,quaternion.identity);
        source.clip = audioClip;
        source.Play();
        float clipLenght = audioClip.length;
        Destroy(source,clipLenght);
    }
    public void PlayAudioClip(AudioClip audioClip)
    {
        AudioSource source = Instantiate(soundFXPrefab,Camera.main.transform.position,quaternion.identity);
        source.clip = audioClip;
        source.Play();
        float clipLenght = audioClip.length;
        Destroy(source.gameObject,clipLenght);
    }

    public void PlayClikSound()
    {
        PlayAudioClip(click);
    }
}
