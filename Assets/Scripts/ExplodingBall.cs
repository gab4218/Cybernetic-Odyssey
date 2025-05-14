using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingBall : EnemyBase
{
    private Animator anim;
    [SerializeField] private float explosionKnockback; //knockback de la explosion
    [SerializeField] private float explosionDelay; //cuanto tarda en explotar
    [SerializeField] private int explosionDamage; //damage de la explosion
    [SerializeField] private SphereCollider explosionCollider; //explosion
    private float delayCurrent; //guarda timer actual
    private bool canMove; //si se puede mover
    private bool timerOff; //si el timer no corre
    private bool collision; //si esta en el collider
    public ParticleSystem partExp;

    protected override void Start()
    {
        base.Start();
        explosionCollider.enabled = true;
        canMove = true;
        timerOff = true;
        collision = false;
        delayCurrent = explosionDelay;
    }

    void Update()
    {
        detectPlayer(); //busca player
        if (state == SEEKING)
        {
            findDirection(); //va hacia

            if (collision == true && canMove == true && timerOff == true)
            {
                state = ATTACKING;
                Debug.Log("esta attacking");
                //anim.Play("Explode"); //animacion de explotar
            }
        }
        if (collision == false && canMove == true && timerOff == true)
        {
            state = SEEKING;
        }
        navMeshAgent.isStopped = !canMove;
    }

    private void FixedUpdate()
    {
        if (state == ATTACKING)
        {
            canMove = false; //frena para epxlotar
        }
        if (collision == true)
        {
            delayCurrent -= Time.fixedDeltaTime; //empieza timer
        }
        else if (collision == false && delayCurrent < explosionDelay)
        {
            delayCurrent += Time.fixedDeltaTime; //retrocede timer
        }
        if (delayCurrent >= explosionDelay)
        {
            canMove = true; //puede volver a moverse
            delayCurrent = explosionDelay; //failsafe por si se pasa 
        }
    }

    private void OnTriggerStay(Collider other) //cuando esta en el collider de explosion
    {
        
        PlayerActions playerA = other.GetComponentInParent<PlayerActions>(); //agarra script de player
        Rigidbody playerRB = other.GetComponentInParent<Rigidbody>(); //agarra Rigidbody de player

        if (playerA != null)
        {
            collision = true;
            canMove = false; //frena para explotar

            if (delayCurrent <= 0)
            {
                findDirection();
                playerA.takeDamage(explosionDamage); //player toma damage
                playerRB.drag = 0;
                playerRB.AddForce((dir + Vector3.up).normalized * explosionKnockback, ForceMode.Impulse); //toma knockback
                ParticleSystem partSys = Instantiate(partExp, transform.position, Quaternion.identity);
                partSys.Play();
                Destroy(gameObject);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        collision = false;
    }
}
