using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PolarBear : EnemyBase
{
    //Comments were written in English because when I code, I like to think in English, as it's closer in nature to C# and (at least professionally) it's more common
    //[Traduccion] Los comentarios fueron escritos en Ingles porque, cuando hago codigo, me gusta pensar en Ingles, ya que es fundamentalmente mas parecido a C# y (al menos profesionalmente) es mas comun
    
    //Extra state Variables
    private const int CLAWING = 4;
    private const int RUSHING = 5;
    private const int BALL = 6;

    
    private Vector2 randomPosition;
    private Animator anim;

    //Attacking AI Variables
    private bool canClaw = true;
    private bool canSlam = true;
    private bool canRush = true;
    private bool canBall = true;
    private Vector3 rushDirection;

    //Slam attack Variables
    [SerializeField] private float slamRange;
    [SerializeField] private int slamDamage;
    [SerializeField] private float slamKnockback = 2.0f;
    [SerializeField] private BoxCollider slamCollider;

    //Claw attack Variables
    [SerializeField] private int clawDamage;
    [SerializeField] private float clawRange;
    [SerializeField] private float clawKnockback;
    [SerializeField] private BoxCollider clawCollider;

    //Rush attack Variables
    [SerializeField] private float minRushDistance = 15f;
    [SerializeField] private int rushDamage = 70;
    [SerializeField] private float rushKnockback = 20f;
    [SerializeField] private float rushSpeed = 10f;
    [SerializeField] private float rushStunTime = 1f;
    [SerializeField] private BoxCollider rushCollider;

    //Ball evade/attack Variables
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
        //Repeat EnemyBase start actions
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
        //General preparation
    }

    private void Update()
    {
        detectPlayer(); 
        
        if (state == SEEKING) //If chasing player
        {
            findDirection(); //Find path to player
            
            if (Vector3.Distance(transform.position, playerTranform.position) < slamRange && canSlam) //Slam
            {
                state = ATTACKING;
                anim.Play("BearSlam");
                canSlam = false;
            }
           
            if (Vector3.Distance(transform.position, playerTranform.position) < clawRange && canClaw) //Claw
            {
                state = CLAWING;
                anim.Play("BearClaw");
                canClaw = false;
            }
            
            if (Vector3.Distance(transform.position, playerTranform.position) > minRushDistance && canRush) //Rush
            {
                RushAttack();
            }


        }
        else if(state == IDLE) //If can't see player
        {
            if (hasReachedDestination(randomPosition)) //If reached destination
            {
                //Find random positions in range to roam arround
                randomPosition = new Vector2
                                (
                                    Random.Range(randomMovementDimensions[0].x, randomMovementDimensions[1].x),
                                    Random.Range(randomMovementDimensions[0].y, randomMovementDimensions[1].y)
                                );
            }
            else 
            {
                findDirection(randomPosition); //Find path to position
            }
        }
        if(state == RUSHING) //If doing a Rush attack
        {
            
            if (Vector3.Distance(transform.position, playerTranform.position) > detectionDistance && !canRush) //Stop if too far from player
            {
                RushReset();
            } 
            
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions pAct = other.GetComponentInParent<PlayerActions>();
        Rigidbody pRB = other.GetComponentInParent<Rigidbody>();

        if (pAct != null) //If Trigger collision is with player
        {
            state = IDLE; //Return to base state
            if (slamCollider.enabled) //If slamming, do Slam
            {
                pAct.takeDamage(slamDamage);
                if (pRB != null)
                {
                    pRB.drag = 0;
                    pRB.AddForce((dir + Vector3.up).normalized * slamKnockback, ForceMode.Impulse);
                }
            }
            if (clawCollider.enabled) //If clawing, do Claw
            {
                pAct.takeDamage(clawDamage);
                if (pRB != null)
                {
                    pRB.drag = 0;
                    pRB.AddForce((dir + transform.right + Vector3.up).normalized * clawKnockback, ForceMode.Impulse);
                }
            }
            if (rushCollider.enabled) //If rushing, do Rush
            {
                pAct.takeDamage(rushDamage);
                if (pRB != null)
                {
                    pRB.drag = 0;
                    pRB.AddForce((dir + 3 * Vector3.up).normalized * rushKnockback, ForceMode.Impulse);
                    RushReset();
                }
            }
        }
        else
        {
            if (rushCollider.enabled) //If Trigger collision is with anything else and is rushing, stop Rush
            {
                RushReset();
            }
        }

    }

    private void FixedUpdate()
    {
        if (state == IDLE || state == SEEKING) //If not attacking in any way, move normally
        {
            move();
        }
        else if (state == RUSHING) //If rushing, move accordingly
        {
            moveRush();
        }
    }

    public void slamAttack() //Used for in-animation function calling
    {
        slamCollider.enabled = true;
    }

    public void slamReset() //Return to base from slam
    {
        slamCollider.enabled = false;
        state = IDLE;
        Invoke("slamReload", 10f);
    }

    private void slamReload() //Used for Invoke
    {
        canSlam = true;
    }

    public void clawAttack() //Used for in-animation function calling
    {
        clawCollider.enabled = true;
    }

    public void clawReset() //Return to base from claw
    {
        clawCollider.enabled = false;
        state = IDLE;
        Invoke("clawReload", 1f);
    }

    private void clawReload() //Used for Invoke
    {
        canClaw = true;
    }

    private void RushAttack() //Start rush attack
    {
        state = RUSHING;
        rushCollider.enabled = true;
        rushDirection = dir;
        canRush = false; 
    }

    public void RushReset() //Return to base from rush
    {
        rushCollider.enabled = false;
        Stun(rushStunTime);
        Invoke("RushReload", 15f);
    }

    private void RushReload() //Used for Invoke
    {
        canRush = true;
    }

    public void StartBall() //Start ball evasion/attack
    {
        if (canBall)
        {
            canBall = false;
            Invoke("DoBall", Random.Range(ballDelayMin, ballDelayMax));
        }
    }
    
    private void DoBall() //Used for Invoke, turns bear into ball
    {
        bearMeshFilter.mesh = ballMesh; //Change mesh to ball
        state = BALL;
        foreach (BoxCollider bc in bearColliders) //Disable all colliders
        {
            bc.enabled = false;
        }
        rb.constraints = RigidbodyConstraints.None; //Make it so ball can roll
        ballCollider.enabled = true; 
        rb.AddForce(transform.forward * ballImpulse, ForceMode.Impulse); //Push ball back
        Invoke("EndBall", 1f);
    }
    
    private void EndBall() //Return to base bear from ball
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.rotation = Quaternion.identity; 
        transform.rotation = Quaternion.identity; //Make rotation original
        bearMeshFilter.mesh = bearMesh; //Change mesh back to bear
        state = SEEKING;
        canBall = true;
        foreach (BoxCollider bc in bearColliders) //Enable all non-trigger colliders
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
        rushDirection = Vector3.Lerp(rushDirection, dir, 0.01f); //Turn slowly
        transform.forward = -rushDirection; //The bear mesh is backwards, so make it face opposite to move direction
        rb.velocity = new Vector3(rushDirection.x, transform.position.y, rushDirection.z).normalized * rushSpeed; //Move the bear
    }
}
