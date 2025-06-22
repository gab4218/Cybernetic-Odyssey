using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaTemporal : MonoBehaviour
{


    [SerializeField] private AudioSource audioSource;

    private Animator _animator;
    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerActions>())
        {
            audioSource.Play();
            _animator.SetTrigger("activado");
        }
    }
}
