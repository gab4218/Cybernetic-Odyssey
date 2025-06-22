using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarBear_Anim : MonoBehaviour
{
    
    //Todo este script es usado para llamado de funciones en animaciones, todas las funciones llaman funciones de PolarBear

    //Scripts necesarios y AnimatorController
    private PolarBear pb;
    private Animator anim;
    [SerializeField] private ParticleSystem clawPS;
    [SerializeField] private Transform clawPos;
    [SerializeField] private AudioSource stepSound;

    private void Start()
    {
        //Preparaciones
        pb = GetComponentInParent<PolarBear>();
        anim = GetComponent<Animator>();
    }
    public void backFromSlam() 
    {
        pb.slamReset();
    }

    public void backFromClaw()
    {
        pb.clawReset();
    }

    public void slam()
    {
        pb.slamAttack();
    }
    
    public void claw()
    {
        pb.clawAttack();
        ParticleSystem cps = Instantiate(clawPS, clawPos);
        cps.Play();
    }

    public void step()
    {
        stepSound.pitch = Random.Range(0.6f, 1.4f);
        stepSound.Play();
    }
}
