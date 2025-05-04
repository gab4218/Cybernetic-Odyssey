using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarBear_Anim : MonoBehaviour
{
    //Comments were written in English because when I code, I like to think in English, as it's closer in nature to C# and (at least professionally) it's more common
    //[Traduccion] Los comentarios fueron escritos en Ingles porque, cuando hago codigo, me gusta pensar en Ingles, ya que es fundamentalmente mas parecido a C# y (al menos profesionalmente) es mas comun

    //This entire script it used for in-animation function calling, everything here calls functions from the PolarBear script

    //Required scripts and animator controller
    private PolarBear pb;
    private Animator anim;

    private void Start()
    {
        //Preparations
        pb = GetComponentInParent<PolarBear>();
        anim = GetComponent<Animator>();
    }
    public void returnToIdle() 
    {
        anim.Play("BearIdle");
        pb.slamReset();
        pb.clawReset();
    }

    public void slam()
    {
        pb.slamAttack();
    }
    
    public void claw()
    {
        pb.clawAttack();
    }
}
