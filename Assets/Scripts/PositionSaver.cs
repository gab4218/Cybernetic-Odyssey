using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionSaver : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerActions pa = other.GetComponentInParent<PlayerActions>();
        if (pa != null)
        {
            pa.lastPosition = transform.position;
        }
    }
}
