using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject _go;
    public static bool paused = false;
    void Start()
    {
        _go.SetActive(false);
    }

    private void Update()
    {
        if (Time.timeScale > 0 && !paused && !CameraController.inCutscene)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                paused = true;
            }
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
        }
    }

}
