using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSingleton : MonoBehaviour
{
    public static SoundSingleton Instance { get; private set; }
    [SerializeField] private AudioSource sfxSource;
    public AudioClip button;
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
    }

    private void FixedUpdate()
    {
        
    }

    public void Boton()
    {
        sfxSource.PlayOneShot(button, 1);
    }

    public void OsoMuerte()
    {
        sfxSource.PlayOneShot(osoMuerte, 1);
    }

    public void Viaje()
    {
        sfxSource.PlayOneShot(viaje, 1);
    }
}
