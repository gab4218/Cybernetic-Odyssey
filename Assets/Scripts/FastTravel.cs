using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FastTravel : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject menu;
    private bool onMenu = false;
    //AsyncOperation async;
    //bool loadingDone;

    private void Start()
    {
        menu.SetActive(false);
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)) && onMenu)
        {
            if (menu.activeSelf)
            {
                onMenu = false;
                menu.SetActive(false);
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private IEnumerator watABit()
    {
        yield return new WaitForEndOfFrame();
        onMenu = true;
    }
    public void onInteract()
    {
        menu.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(watABit());
    }

    public void Cancel()
    {
        menu.SetActive(false);
        onMenu = false;
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SoundSingleton.Instance.Boton();
    }

    public void GoToScene(string scene)
    {
        SoundSingleton.Instance.Viaje();
        Time.timeScale = 1;
        Debug.Log(scene);
        SceneManager.LoadScene(scene);
    }
}
