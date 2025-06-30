using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaTemporal : MonoBehaviour
{


    [SerializeField] private AudioSource audioSource;
    private CameraController camController;
    private Animator _animator;
    private Coroutine shakeCoroutine;
    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        camController = Camera.main.GetComponentInParent<CameraController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerActions>())
        {
            audioSource.Play();
            _animator.SetTrigger("activado");
            shakeCoroutine = StartCoroutine(camController.Shake(0.5f, 0.1f));
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerActions>())
        {
            if(shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        }
    }
}
