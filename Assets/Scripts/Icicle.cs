using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Icicle : MonoBehaviour
{
    private Rigidbody rb; //Rigidbody para prender y apagar gravedad
    [SerializeField] private int icicleDamage; //dmg de la estalactita
    [SerializeField] private CapsuleCollider playerDetect; //Collider que detecta al player
    [SerializeField] private CapsuleCollider icicleCollider; //Collider que hace dmg

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions playerA = other.GetComponentInParent<PlayerActions>(); //agarra script de player
        if (playerA != null)
        {
            rb.useGravity = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerActions playerA = collision.gameObject.GetComponentInParent<PlayerActions>(); //script player
        if(playerA != null)
        {
            playerA.takeDamage(icicleDamage);
            Destroy(gameObject);
        }
        if (collision.gameObject.layer == 30)
        {
            Destroy(gameObject);
        }
    }
}
