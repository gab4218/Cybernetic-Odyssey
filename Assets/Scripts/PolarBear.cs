using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PolarBear : EnemyBase
{
    
    //Variables de estado extra
    private const int CLAWING = 4;
    private const int RUSHING = 5;
    private const int BALL = 6;

    
    private Vector2 randomPosition;
    private Animator anim;

    //Variables de ataque de la IA
    private bool canClaw = true;
    private bool canSlam = true;
    private bool canRush = true;
    private bool canBall = true;
    private Vector3 rushDirection;

    //Variables del ataque de Slam
    [SerializeField] private float slamRange;
    [SerializeField] private int slamDamage;
    [SerializeField] private float slamKnockback = 2.0f;
    [SerializeField] private BoxCollider slamCollider;

    //Variables del ataque de Claw
    [SerializeField] private int clawDamage;
    [SerializeField] private float clawRange;
    [SerializeField] private float clawKnockback;
    [SerializeField] private BoxCollider clawCollider;

    //Variables del ataque de Rush
    [SerializeField] private float minRushDistance = 15f;
    [SerializeField] private int rushDamage = 70;
    [SerializeField] private float rushKnockback = 20f;
    [SerializeField] private float rushSpeed = 10f;
    [SerializeField] private float rushStunTime = 1f;
    [SerializeField] private BoxCollider rushCollider;

    //Variables del ataque/evasion de Ball
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
        //Repetir acciones de EnemyBase.Start
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
        //Preparaciones generales
    }

    private void Update()
    {
        detectPlayer(); 
        
        if (state == SEEKING) //Si se persigue al jugador, atacarlo cuando sea posible 
        {
            findDirection(); 
            
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
        else if(state == IDLE) //Si no sigue al jugador, moverse a una posicion random
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
        if(state == RUSHING) //Si hace un ataque de Rush, frenar si se aleja mucho del jugador
        {
            
            if (Vector3.Distance(transform.position, playerTranform.position) > detectionDistance && !canRush)
            {
                RushReset();
            } 
            
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions pAct = other.GetComponentInParent<PlayerActions>();
        Rigidbody pRB = other.GetComponentInParent<Rigidbody>();

        if (pAct != null) //Si la colision es con el jugador, volver a estado base y hacer el ataque correcto
        {
            state = IDLE; 
            if (slamCollider.enabled) 
            {
                pAct.takeDamage(slamDamage);
                if (pRB != null)
                {
                    pRB.drag = 0;
                    pRB.AddForce((dir + Vector3.up).normalized * slamKnockback, ForceMode.Impulse);
                }
            }
            if (clawCollider.enabled) 
            {
                pAct.takeDamage(clawDamage);
                if (pRB != null)
                {
                    pRB.drag = 0;
                    pRB.AddForce((dir + transform.right + Vector3.up).normalized * clawKnockback, ForceMode.Impulse);
                }
            }
            if (rushCollider.enabled) 
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
            if (rushCollider.enabled) //Si la colision fue con cualquier otra cosa y esta haciendo Rush, detener Rush
            {
                RushReset();
            }
        }

    }

    private void FixedUpdate()
    {
        if (state == IDLE || state == SEEKING) //Si no ataca de ningun modo, moverse normalmente
        {
            move(true);
        }
        else if (state == RUSHING) //Si hace Rush, moverse como Rush
        {
            moveRush();
        }
    }


    //Las siguientes funciones son usadas para llamado detro de animaciones
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

    private void slamReload() //Para Invoke
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

    private void clawReload() //Para Invoke
    {
        canClaw = true;
    }

    private void RushAttack() //Iniciar ataque de Rush
    {
        state = RUSHING;
        rushCollider.enabled = true;
        rushDirection = dir;
        canRush = false; 
    }

    public void RushReset() //Volver a base de Rush
    {
        rushCollider.enabled = false;
        Stun(rushStunTime);
        Invoke("RushReload", 15f);
    }

    private void RushReload() //Para Invoke
    {
        canRush = true;
    }

    public void StartBall() //Iniciar Ball
    {
        if (canBall)
        {
            canBall = false;
            Invoke("DoBall", Random.Range(ballDelayMin, ballDelayMax));
        }
    }
    
    private void DoBall() //Para Invoke, usado para volver Oso a Ball
    {
        bearMeshFilter.mesh = ballMesh; //Cambiar mesh a Ball
        state = BALL;
        foreach (BoxCollider bc in bearColliders) //Desabilitar todos los colliders
        {
            bc.enabled = false;
        }
        rb.constraints = RigidbodyConstraints.None; //Desbloquear rotacion de la bola
        ballCollider.enabled = true; 
        rb.AddForce(transform.forward * ballImpulse, ForceMode.Impulse); //Empujar
        Invoke("EndBall", 1f);
    }
    
    private void EndBall() //Para Invoke, volver a base de Ball
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.rotation = Quaternion.identity; 
        transform.rotation = Quaternion.identity; //Hacer rotacion Default
        bearMeshFilter.mesh = bearMesh; //Cambiar mesh a Oso
        state = SEEKING;
        canBall = true;
        foreach (BoxCollider bc in bearColliders) //Activar todos los colliders no trigger
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
        rushDirection = Vector3.Lerp(rushDirection, dir, 0.01f); //Girar lentamente
        transform.forward = -rushDirection; 
        rb.velocity = new Vector3(rushDirection.x, transform.position.y, rushDirection.z).normalized * rushSpeed; //Mover
    }
}
