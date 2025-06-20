using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void Play(string Juego)
    {
        SceneManager.LoadScene(Juego);
        SoundSingleton.Instance.Boton();
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
    public void Restart(string Win)
    {
        SceneManager.LoadScene(Win);
    }

    public void Salir()
    {
        SoundSingleton.Instance.Boton();
        Application.Quit();
        Debug.Log("Saliste");
    }
}
