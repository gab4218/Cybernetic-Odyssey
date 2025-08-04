using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class jumper : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AgentLinkMover al = other.GetComponentInParent<AgentLinkMover>();
        if (al != null)
        {
            al.jump = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        AgentLinkMover al = other.GetComponentInParent<AgentLinkMover>();
        if (al != null)
        {
            al.jump = false;
        }
    }
}
