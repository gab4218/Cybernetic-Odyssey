using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FastTravel : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject menu, bossButton, lavaButton, acidButton;
    private bool onMenu = false;
    private string _targetScene;
    //AsyncOperation async;
    //bool loadingDone;

    private void Start()
    {
        if (bossButton != null && !ProgressManager.gotMilitaryButton) bossButton.SetActive(false);
        if (acidButton != null && !ProgressManager.gotSpiderButton) acidButton.SetActive(false);
        if (lavaButton != null && !ProgressManager.gotSalButton) lavaButton.SetActive(false);
        menu.SetActive(false);
    }

    public void UnlockBoss()
    {
        if (bossButton != null && ProgressManager.beatSpider) bossButton.SetActive(true);
        ProgressManager.talkedToMilitary = true;
        ProgressManager.gotMilitaryButton = true;
    }

    public void UnlockLava()
    {
        if (lavaButton != null && ProgressManager.beatBear) lavaButton.SetActive(true);
        ProgressManager.gotSalButton = true;
    }

    public void UnlockAcid()
    {
        if (acidButton != null && ProgressManager.beatSalamander) acidButton.SetActive(true);
        ProgressManager.gotSpiderButton = true;
    }

    private void Update()
    {
        if (Pause.paused) return;
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


    public void LoadScene()
    {
        AsyncLoadManager.instance.LoadScene(_targetScene);
        PlayerActions.dead = true;
    }

    public void GoToScene(string scene)
    {
        SoundSingleton.Instance.Viaje();
        Cutscener.instance.EnterLeaveCutscene();
        Time.timeScale = 1;
        _targetScene = scene;
    }
}
