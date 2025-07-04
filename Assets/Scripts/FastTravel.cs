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
        if (PlayerActions.won)
        {
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 1;
            Cursor.visible = true;
            SceneManager.LoadScene("win");
            return;
        }
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
        AsyncLoadManager.instance.LoadScene(scene);
        Time.timeScale = 1;
        PlayerActions.dead = true;
    }
}
