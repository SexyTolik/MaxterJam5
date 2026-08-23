using UnityEngine;
using UnityEngine.Audio;
public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    // DONT FORGET CLAMP AUDIO SLIDERS TO 0.0001 & 1
    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(level) * 20f);
    }

    public void SetSFXVolume(float level)
    {
        audioMixer.SetFloat("SoundEffectsVolume", Mathf.Log10(level) * 20f);
    }
}
