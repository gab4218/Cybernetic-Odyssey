using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearUpperCollisionDetection : MonoBehaviour
{
    private PolarBear pb;

    void Start()
    {
        pb = GetComponentInParent<PolarBear>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions pAct = other.GetComponentInParent<PlayerActions>();
        if (pAct!=null)
        {
            pb.StartBall();
        }
    }
}
