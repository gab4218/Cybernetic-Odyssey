using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FastTravel : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject menu;
    //AsyncOperation async;
    //bool loadingDone;

    private void Start()
    {
        menu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menu.activeSelf)
            {
                menu.SetActive(false);
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public void onInteract()
    {
        menu.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Cancel()
    {
        menu.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GoToScene(string scene)
    {

        SceneManager.LoadScene(scene);
        
        //StartCoroutine(AsyncSceneLoad(scene));
    }
    /*
    IEnumerator AsyncSceneLoad(string scene)
    {
        async = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        async.allowSceneActivation = false;
        while (!async.isDone)
        {
            if (async.progress >= 0.9f)
                async.allowSceneActivation = true;
            yield return null;
            
        }
        loadingDone = async.isDone;
    }
    */
}
