using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComputerButtonsSoundHandler : MonoBehaviour
{
    public AudioClip clikSFX;
    public void playeclik()
    {
        SoundFXManager.instance.PlayAudioClip(clikSFX);
    }
}
