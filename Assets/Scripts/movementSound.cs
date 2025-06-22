using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movementSound : MonoBehaviour
{
    [SerializeField] private AudioSource stepAudioSource;
    [SerializeField] private AudioSource jumpAudioSource;
    public void PlayStep()
    {
        stepAudioSource.pitch = Random.Range(0.6f, 1.4f);
        stepAudioSource.Play();
    }

    public void PlayJump()
    {
        jumpAudioSource.pitch = Random.Range(0.8f, 1.2f);
        jumpAudioSource.Play();
    }

}
