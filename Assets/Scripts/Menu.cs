using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] AudioClip musicClip;
    [SerializeField] AudioClip gameMusic;

    private void Start()
    {
        if (SoundSingleton.Instance.musicSource.clip != musicClip)
        {
            SoundSingleton.Instance.SetMusic(musicClip);
            
            //jfc
        }
    }
    public void Play(string Juego)
    {
        SoundSingleton.Instance.SetMusic(gameMusic);
        AsyncLoadManager.instance.LoadScene(Juego);
        SoundSingleton.Instance.Boton();
    }

    public void ResetAll()
    {
        Inventory.availableUpgrades = new List<int>();
        Inventory.secondarySlots = new int[4];
        Inventory.materialInventory = new int[3];
        Inventory.hasShotgun = false;
        Inventory.hasFlamethrower = false;
    }

    public void Controls(string controls)
    {
        SceneManager.LoadScene(controls);
        SoundSingleton.Instance.Boton();
    }
    public void Return(string menu)
    {
        SceneManager.LoadScene(menu);
        SoundSingleton.Instance.Boton();
    }

    public void Salir()
    {
        SoundSingleton.Instance.Boton();
        Application.Quit();
        Debug.Log("Saliste");
    }
}
