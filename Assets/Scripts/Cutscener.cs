using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscener : MonoBehaviour
{
    private Vector3 _startPos;
    void Start()
    {
        _startPos = transform.position;
        CameraController.inCutscene = true;
    }

    public void EndCutscene()
    {
        transform.position = _startPos;
        CameraController.inCutscene = false;
    }
}
