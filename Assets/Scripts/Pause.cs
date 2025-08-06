using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject _go;
    public static bool paused = false;
    private bool _canPause = true;
    void Start()
    {
        _go.SetActive(false);
    }

    private void Update()
    {
        if (Time.timeScale > 0 && !paused && !CameraController.inCutscene)
        {
            if (_canPause)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Time.timeScale = 0;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    paused = true;
                    _go.SetActive(true);
                }
            }
            else
            {
                _canPause = true;
            }
        }
        else
        {
            _canPause = false;
        }
    }

    public void Resume()
    {
        if (Time.timeScale <= 0 && paused)
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            paused = false;
            _go.SetActive(false);
        }
    }

    public void GoToMenu()
    {
        Time.timeScale = 1;
        PlayerActions.dead = true;
        paused = false;
        SceneManager.LoadScene("Menu");
    }

}
