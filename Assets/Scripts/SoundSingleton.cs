using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSingleton : MonoBehaviour
{
    public static SoundSingleton Instance { get; private set; }
    private AudioSource audioSource;
    public AudioClip button;
    public AudioClip musicMenu;
    public AudioClip osoMuerte;
    public AudioClip viaje;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
    }

    private void FixedUpdate()
    {
        
    }

    public void Boton()
    {
        audioSource.PlayOneShot(button, 1);
    }
}
