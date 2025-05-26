using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trees : MonoBehaviour
{
    private Rigidbody rb; //Rigidbody para prender/apagar gravedad

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
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
