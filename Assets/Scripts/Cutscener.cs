using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscener : MonoBehaviour
{
    public static Cutscener instance;
    private Vector3 _startPos;
    private Quaternion _startRotation;
    private float _startFOV;
    [SerializeField] private Animation _cameraAnim;
    [SerializeField] private Animation _anim;
    [SerializeField] private AnimationClip _cameraDefault;
    [SerializeField] private AnimationClip _cameraLeave;
    [SerializeField] private AnimationClip _thisLeave;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private GameObject _gunCam; 
    void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        _startFOV = Camera.main.fieldOfView;
        _startPos = Camera.main.transform.parent.position;
        _startRotation = Camera.main.transform.parent.rotation;
        _anim.Play();
        _cameraAnim.Play();
        CameraController.inCutscene = true;
        _canvas.SetActive(false);
        _gunCam.SetActive(false);
        Camera.main.fieldOfView = _startFOV/2f;
    }

    public void EnterLeaveCutscene()
    {
        Camera.main.transform.parent.position = _startPos;
        Camera.main.transform.parent.rotation = _startRotation;
        Camera.main.fieldOfView = _startFOV / 2f;
        _anim.clip = _thisLeave;
        _anim.Play();
        _cameraAnim.clip = _cameraLeave;
        _cameraAnim.Play();
        CameraController.inCutscene = true;
        _canvas.SetActive(false);
        _gunCam.SetActive(false);

    }

    public void EndCutscene()
    {
        _cameraAnim.clip = _cameraDefault;
        _cameraAnim.Play();
        CameraController.inCutscene = false;
        Camera.main.fieldOfView = _startFOV;
        _canvas.SetActive(true);
        _gunCam.SetActive(true);
    }
}
