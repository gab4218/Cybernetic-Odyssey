using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backToMenuFromProto : MonoBehaviour, IInteractable
{
    public void onInteract()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AsyncLoadManager.instance.LoadScene("Menu");
    }

}
