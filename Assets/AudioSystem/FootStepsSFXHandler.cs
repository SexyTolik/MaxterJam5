using UnityEngine;

public class FootStepsSFXHandler : MonoBehaviour
{
    
    [Tooltip("Звуки ходьбы")]
    [SerializeField] private AudioClip walkSFX1;
    [SerializeField] private AudioClip walkSFX2;
    public void PlaySFX1()
    {
        SoundFXManager.instance.PlayAudioClip(walkSFX1);
    }
    public void PlaySFX2()
    {
        SoundFXManager.instance.PlayAudioClip(walkSFX2);
    }
}
