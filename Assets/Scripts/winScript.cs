using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class winScript : MonoBehaviour
{
    [SerializeField] Animation _anim;
    
    public void StartTransition()
    {
        _anim.Play();
    }
    public void EndTransition()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1;
        SceneManager.LoadScene("win");
    }
}
