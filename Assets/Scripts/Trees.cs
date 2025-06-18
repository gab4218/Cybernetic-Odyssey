using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Trees : MonoBehaviour
{
    private Rigidbody rb; //Rigidbody para prender/apagar gravedad
    

    private void Awake()
    {
        //rb = GetComponent<Rigidbody>();
        //rb.useGravity = false;
    
    }

    private void OnTriggerEnter(Collider other)
    {
        PolarBear enemy = other.gameObject.GetComponentInParent<PolarBear>();
        if (enemy != null)
        {
            Destroy(gameObject);
        }
    } 
}
