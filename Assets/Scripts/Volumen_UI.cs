using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Volumen_UI : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider musicSlider;

    private void Awake()
    {
        float t;
        mixer.GetFloat("MasterVolume", out t);
        masterSlider.value = t;
        mixer.GetFloat("MusicVolume", out t);
        musicSlider.value = t;
        mixer.GetFloat("SFXVolume", out t);
        sfxSlider.value = t;
    }

    public void UpdateMasterVolume()
    {
        mixer.SetFloat("MasterVolume", masterSlider.value);
        if (!SoundSingleton.Instance.sfxSource.isPlaying)
        {
            SoundSingleton.Instance.Boton();
        }
    }

    public void UpdateMusicVolume()
    {
        mixer.SetFloat("MusicVolume", musicSlider.value);
    }

    public void UpdateSFXVolume()
    {
        mixer.SetFloat("SFXVolume", sfxSlider.value);
        if (!SoundSingleton.Instance.sfxSource.isPlaying)
        {
            SoundSingleton.Instance.Boton();
        }
    }
}
