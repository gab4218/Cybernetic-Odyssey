using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Icicle : MonoBehaviour
{
    private Rigidbody rb; //Rigidbody para prender y apagar gravedad
    [SerializeField] private int icicleDamage; //dmg de la estalactita
    [SerializeField] private CapsuleCollider playerDetect; //Collider que detecta al player
    [SerializeField] private CapsuleCollider icicleCollider; //Collider que hace dmg
    [SerializeField] private GameObject shadow;
    [SerializeField] private AudioSource source;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        Ray r = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            shadow.transform.position = hit.point + Vector3.up * 0.01f;
            shadow.transform.up = hit.normal;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions playerA = other.GetComponentInParent<PlayerActions>(); //agarra script de player
        if (playerA != null)
        {
            rb.useGravity = true;
            AudioSource aS = Instantiate(source, transform);
            aS.Play();
            Destroy(aS, aS.clip.length);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerActions playerA = collision.gameObject.GetComponentInParent<PlayerActions>(); //script player
        if(playerA != null)
        {
            playerA.takeDamage(icicleDamage);
            Destroy(shadow);
            Destroy(gameObject);
        }
        if (collision.gameObject.layer == 30)
        {
            Destroy(shadow);
            Destroy(gameObject);
        }
    }
}
