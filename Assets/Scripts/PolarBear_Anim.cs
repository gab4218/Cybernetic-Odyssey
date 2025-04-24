using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarBear_Anim : MonoBehaviour
{
    private PolarBear pb;
    private Animator anim;
    private void Start()
    {
        pb = GetComponentInParent<PolarBear>();
        anim = GetComponent<Animator>();
    }
    public void returnToIdle()
    {
        anim.Play("BearIdle");
        pb.slamReset();
    }

    public void slam()
    {
        pb.slamAttack();
    }
    

}
