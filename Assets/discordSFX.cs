using UnityEngine;

public class discordSFX : MonoBehaviour
{
    public AudioClip clip;

    public void PlaySFX()
    {
        SoundFXManager.instance.PlayAudioClip(clip);
    }
}
