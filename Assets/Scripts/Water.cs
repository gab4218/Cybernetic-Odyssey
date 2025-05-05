using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerActions pa = other.GetComponentInParent<PlayerActions>();
        EnemyBase eb = other.GetComponentInParent<EnemyBase>();
        if (pa != null)
        {
            pa.takeDamage(30);
            pa.GetComponent<Rigidbody>().velocity = Vector3.zero;
            pa.gameObject.transform.position = pa.lastPosition;
        }
        else if (eb != null) Destroy(eb.gameObject); 
    }
}
