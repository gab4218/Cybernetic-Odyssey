using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PolarBear : EnemyBase
{
    private const int CLAWING = 4;
    private const int RUSHING = 5;

    private Vector2 randomPosition;
    private Animator anim;
    private bool canClaw = true;
    private bool canSlam = true;
    private bool canRush = true;
    private Vector3 rushDirection;

    [SerializeField] private float slamRange;
    [SerializeField] private int slamDamage;
    [SerializeField] private BoxCollider slamCollider;
    [SerializeField] private float slamKnockback = 2.0f;
    [SerializeField] private int clawDamage;
    [SerializeField] private float clawRange;
    [SerializeField] private BoxCollider clawCollider;
    [SerializeField] private float clawKnockback;
    [SerializeField] private float minRushDistance = 15f;
    [SerializeField] private int rushDamage = 70;
    [SerializeField] private float rushKnockback = 20f;
    [SerializeField] private float rushSpeed = 10f;
    
    [SerializeField] private BoxCollider rushCollider;
    
    

    

    protected override void Start()
    {
        base.Start();
        anim = GetComponentInChildren<Animator>();
        slamCollider.enabled = false;
        clawCollider.enabled = false;
        randomPosition = new Vector2
                            (
                                Random.Range(randomMovementDimensions[0].x, randomMovementDimensions[1].x),
                                Random.Range(randomMovementDimensions[0].y, randomMovementDimensions[1].y)
                            );
        findDirection(randomPosition);
    }

    private void Update()
    {
        detectPlayer();
        HPDisplay.text = $"Bear HP: {currentHP}/{maxHP}";
        if (state == SEEKING)
        {
            findDirection();
            
            if (Vector3.Distance(transform.position, playerTranform.position) < slamRange && canSlam)
            {
                state = ATTACKING;
                anim.Play("BearSlam");
                canSlam = false;
            }
           
            if (Vector3.Distance(transform.position, playerTranform.position) < clawRange && canClaw)
            {
                state = CLAWING;
                anim.Play("BearClaw");
                canClaw = false;
            }

            if (Vector3.Distance(transform.position, playerTranform.position) > minRushDistance && canRush)
            {
                RushAttack();
            }


        }
        else if(state == IDLE)
        {
            if (hasReachedDestination(randomPosition))
            {

                randomPosition = new Vector2
                                (
                                    Random.Range(randomMovementDimensions[0].x, randomMovementDimensions[1].x),
                                    Random.Range(randomMovementDimensions[0].y, randomMovementDimensions[1].y)
                                );
            }
            else
            {
                findDirection(randomPosition);
            }
        }
        else if(state == RUSHING)
        {
            
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions pAct = other.GetComponentInParent<PlayerActions>();
        Rigidbody pRB = other.GetComponentInParent<Rigidbody>();

        if (pAct != null)
        {
            state = IDLE;
            if (slamCollider.enabled)
            {
                pAct.takeDamage(slamDamage);
                if (pRB != null)
                {
                    pRB.drag = 0;
                    pRB.AddForce((dir + Vector3.up).normalized * slamKnockback * pAct.getKnockbackMult(), ForceMode.Impulse);
                }
            }
            if (clawCollider.enabled)
            {
                pAct.takeDamage(clawDamage);
                if (pRB != null)
                {
                    pRB.drag = 0;
                    pRB.AddForce((dir + transform.right + Vector3.up).normalized * clawKnockback * pAct.getKnockbackMult(), ForceMode.Impulse);
                }
            }
            if (rushCollider.enabled)
            {
                pAct.takeDamage(rushDamage);
                if (pRB != null)
                {
                    pRB.drag = 0;
                    pRB.AddForce((dir + 3 * Vector3.up).normalized * rushKnockback * pAct.getKnockbackMult(), ForceMode.Impulse);
                    RushReset();
                }
            }
        }

    }

    private void FixedUpdate()
    {
        if (state == IDLE || state == SEEKING)
        {
            move();
        }
        else if (state == RUSHING)
        {
            moveRush();
        }
    }

    public void slamAttack()
    {
        slamCollider.enabled = true;
    }

    public void slamReset()
    {
        slamCollider.enabled = false;
        state = IDLE;
        Invoke("slamReload", 10f);
    }

    private void slamReload()
    {
        canSlam = true;
    }

    public void clawAttack()
    {
        clawCollider.enabled = true;
    }

    public void clawReset()
    {
        clawCollider.enabled = false;
        state = IDLE;
        Invoke("clawReload", 1f);
    }

    private void clawReload()
    {
        canClaw = true;
    }

    private void RushAttack()
    {
        state = RUSHING;
        rushCollider.enabled = true;
        rushDirection = dir;
        canRush = false; 
    }

    public void RushReset()
    {
        rushCollider.enabled = false;
        state = IDLE;
        Invoke("RushReload", 15f);
    }

    private void RushReload()
    {
        canRush = true;
    }

    private void moveRush()
    {
        findDirection();
        rushDirection = Vector3.Lerp(rushDirection, dir, 0.02f);
        transform.forward = Vector3.Lerp(transform.forward, -rushDirection, 0.1f);
        rb.velocity = new Vector3(rushDirection.x, transform.position.y, rushDirection.z).normalized * rushSpeed;
    }
}
