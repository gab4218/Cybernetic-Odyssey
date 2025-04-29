using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PolarBear : EnemyBase
{
    private Vector2 randomPosition;
    private Animator anim;


    [SerializeField] private float slamRange;
    [SerializeField] private int slamDamage;
    [SerializeField] private BoxCollider slamCollider;
    [SerializeField] private float slamKnockback = 2.0f;
    [SerializeField] private int clawDamage;
    [SerializeField] private float clawRange;
    [SerializeField] private BoxCollider clawCollider;
    [SerializeField] private float clawKnockback;
    private const int CLAWING = 3;

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
        if (state != IDLE)
        {
            findDirection();
            if (Vector3.Distance(transform.position, playerTranform.position) < slamRange) //put timer or sumn
            {
                state = ATTACKING;
                anim.Play("BearSlam");
            }
            if (Vector3.Distance(transform.position, playerTranform.position) < clawRange)
            {
                state = CLAWING;
                anim.Play("BearClaw");
            }
        }
        else if(hasReachedDestination(randomPosition))
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

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions pAct = other.GetComponentInParent<PlayerActions>();
        Rigidbody pRB = other.GetComponentInParent<Rigidbody>();
        if (slamCollider.enabled && pAct != null)
        {
            pAct.takeDamage(slamDamage);
            if (pRB != null)
            {
                pRB.drag = 0;
                pRB.AddForce((dir + Vector3.up).normalized * slamKnockback * pAct.getKnockbackMult(), ForceMode.Impulse);
            }
        }
        if (clawCollider.enabled && pAct != null)
        {
            pAct.takeDamage(clawDamage);
            if (pRB != null)
            {
                pRB.drag = 0;
                pRB.AddForce((dir - transform.right).normalized * clawKnockback * pAct.getKnockbackMult(), ForceMode.Impulse);
            }
        }
    }

    private void FixedUpdate()
    {
        if (state == IDLE || state == SEEKING)
        {
            move();
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
    }

    public void clawAttack()
    {
        clawCollider.enabled = true;
    }

    public void clawReset()
    {
        clawCollider.enabled = false;
        state = IDLE;
    }
}
