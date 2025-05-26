using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void Play(string Juego)
    {
        SceneManager.LoadScene(Juego);
    }
    public void Controls(string controls)
    {
        SceneManager.LoadScene(controls);
    }
    public void Return(string menu)
    {
        SceneManager.LoadScene(menu);
    }
    public void Restart(string Win)
    {
        SceneManager.LoadScene(Win);
    }

    public void Salir()
    {
        Application.Quit();
        Debug.Log("Saliste");
    }


}
