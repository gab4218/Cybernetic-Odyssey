using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Volumen_UI : MonoBehaviour
{
    public AudioMixer mixer;
    public GameObject window;
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider musicSlider;

    private void Update()
    {
        UpdateMasterVolume();
        UpdateMusicVolume();
        UpdateSFXVolume();
    }

    public void UpdateMasterVolume()
    {
        mixer.SetFloat("MasterVolume", masterSlider.value);
    }

    public void UpdateMusicVolume()
    {
        mixer.SetFloat("MusicVolume", musicSlider.value);
    }

    public void UpdateSFXVolume()
    {
        mixer.SetFloat("SFXVolume", sfxSlider.value);
    }
}
