using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PolarBear : EnemyBase
{
    private const int CLAWING = 4;
    private const int RUSHING = 5;
    private const int BALL = 6;

    private Vector2 randomPosition;
    private Animator anim;
    private bool canClaw = true;
    private bool canSlam = true;
    private bool canRush = true;
    private bool canBall = true;
    private Vector3 rushDirection;

    [SerializeField] private float slamRange;
    [SerializeField] private int slamDamage;
    [SerializeField] private float slamKnockback = 2.0f;
    [SerializeField] private BoxCollider slamCollider;
    [SerializeField] private int clawDamage;
    [SerializeField] private float clawRange;
    [SerializeField] private float clawKnockback;
    [SerializeField] private BoxCollider clawCollider;
    [SerializeField] private float minRushDistance = 15f;
    [SerializeField] private int rushDamage = 70;
    [SerializeField] private float rushKnockback = 20f;
    [SerializeField] private float rushSpeed = 10f;
    [SerializeField] private float rushStunTime = 1f;
    [SerializeField] private BoxCollider rushCollider;
    [SerializeField] private float ballImpulse = 10f;
    [SerializeField] private float ballDelayMin = 1f;
    [SerializeField] private float ballDelayMax = 3f;
    [SerializeField] private BoxCollider ballTriggerCollider;
    [SerializeField] private MeshFilter bearMeshFilter;
    [SerializeField] private Mesh bearMesh, ballMesh;
    [SerializeField] private SphereCollider ballCollider;

    private BoxCollider[] bearColliders;
    
    

    

    protected override void Start()
    {
        base.Start();
        anim = GetComponentInChildren<Animator>();
        slamCollider.enabled = false;
        clawCollider.enabled = false;
        bearColliders = GetComponentsInChildren<BoxCollider>();
        randomPosition = new Vector2
                            (
                                Random.Range(randomMovementDimensions[0].x, randomMovementDimensions[1].x),
                                Random.Range(randomMovementDimensions[0].y, randomMovementDimensions[1].y)
                            );
        findDirection(randomPosition);
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        detectPlayer();
        
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
        if(state == RUSHING)
        {
            
            if (Vector3.Distance(transform.position, playerTranform.position) > minRushDistance + 2 && !canRush)
            {
                RushReset();
            }
            
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
        else
        {
            if (rushCollider.enabled)
            {
                RushReset();
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
        Stun(rushStunTime);
        Invoke("RushReload", 15f);
    }

    private void RushReload()
    {
        canRush = true;
    }

    public void StartBall()
    {
        if (canBall)
        {
            canBall = false;
            Invoke("DoBall", Random.Range(ballDelayMin, ballDelayMax));
        }
    }
    
    private void DoBall()
    {
        bearMeshFilter.mesh = ballMesh;
        state = BALL;
        foreach (BoxCollider bc in bearColliders)
        {
            bc.enabled = false;
        }
        rb.constraints = RigidbodyConstraints.None;
        ballCollider.enabled = true;
        rb.AddForce(transform.forward * ballImpulse, ForceMode.Impulse);
        Invoke("EndBall", 1f);
    }
    
    private void EndBall()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.rotation = Quaternion.identity;
        transform.rotation = Quaternion.identity;
        bearMeshFilter.mesh = bearMesh;
        state = SEEKING;
        canBall = true;
        foreach (BoxCollider bc in bearColliders)
        {
            if (!bc.isTrigger)
            {
                bc.enabled = true;
            }
        }
        ballTriggerCollider.enabled = true;
        ballCollider.enabled = false;
    }
    private void moveRush()
    {
        findDirection();
        rushDirection = Vector3.Lerp(rushDirection, dir, 0.01f);
        transform.forward = rushDirection;
        rb.velocity = new Vector3(rushDirection.x, transform.position.y, rushDirection.z).normalized * rushSpeed;
    }
}
