using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaTemporal : MonoBehaviour
{
    

  
    private Animator _animator;
    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerActions>())
        {
            
            _animator.SetTrigger("activado");
        }
    }
}
