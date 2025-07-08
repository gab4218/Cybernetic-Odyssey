using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchHitDetection : MonoBehaviour
{
    [SerializeField] private FinalBoss _fb;
    [SerializeField] private Collider _impactCollider;

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions pa = other.GetComponentInParent<PlayerActions>();
        if (pa != null && _impactCollider.enabled)
        {
            Vector3 dir = pa.transform.position - transform.position;
            dir.y = 0;
            dir.Normalize();
            _fb.HitFist(dir);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        _fb.punchWorking = false;
    }
}
