using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

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
    [SerializeField] private ParticleSystem slamParticle;
    [SerializeField] private ParticleSystem holeParticle;
    [SerializeField] private Transform slamLocation;
    [SerializeField] private AudioSource slamSound;

    //Variables del ataque de Claw
    [SerializeField] private int clawDamage;
    [SerializeField] private float clawRange;
    [SerializeField] private float clawKnockback;
    [SerializeField] private BoxCollider clawCollider;
    [SerializeField] private AudioSource clawSound;

    //Variables del ataque de Rush
    [SerializeField] private float minRushDistance = 15f;
    [SerializeField] private int rushDamage = 70;
    [SerializeField] private float rushKnockback = 20f;
    [SerializeField] private float rushSpeed = 10f;
    [SerializeField] private float rushStunTime = 1f;
    [SerializeField] private float rushChargeTime = 1.5f;
    [SerializeField] private float maxRushTime = 5f;
    [SerializeField] private BoxCollider rushCollider;
    [SerializeField] private ParticleSystem rushParticle;
    [SerializeField] private ParticleSystem rushChargeParticle, crashParticleSystem;
    [SerializeField] private Transform rushPartTransform, crashPartTransform;
    [SerializeField] private AudioSource rushSound, crashSound;
    private bool canMoveRush = false;
    private ParticleSystem currentRushParticle, currentCrashPS;
    private Coroutine rushCR;

    //Variables del ataque/evasion de Ball
    [SerializeField] private float ballImpulse = 3f;
    [SerializeField] private float ballDelayMin = 1f;
    [SerializeField] private float ballDelayMax = 3f;
    [SerializeField] private BoxCollider ballTriggerCollider;
    [SerializeField] private GameObject bearMesh, ballMesh;
    [SerializeField] private SphereCollider ballCollider;
    [SerializeField] private float ballWaitTime = 10f;
    

    private BoxCollider[] bearColliders;
    //private bool weakened = false;
    [SerializeField] private TMP_Text winText;

    

    protected override void Start()
    {
        //Repetir acciones de EnemyBase.Start
        base.Start();
        if (ProgressManager.beatBear)
        {
            //BORRAR
            //Destroy(gameObject);
            maxHP = (int)(maxHP * 1.5f);
            currentHP = maxHP;
            clawDamage = (int) (1.2f * clawDamage);
            slamDamage = (int) (1.2f * slamDamage);
            rushDamage = (int) (1.2f * rushDamage);
            drops = false;
        }
        anim = GetComponentInChildren<Animator>();
        slamCollider.enabled = false;
        clawCollider.enabled = false;
        bearColliders = GetComponentsInChildren<BoxCollider>();
        winText?.gameObject.SetActive(false);
        randomPosition = new Vector2
                            (
                                Random.Range(randomMovementDimensions[0].x, randomMovementDimensions[1].x),
                                Random.Range(randomMovementDimensions[0].z, randomMovementDimensions[1].z)
                            );
        setDestination(randomPosition);
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        //HPDisplay = GameObject.FindWithTag("BearHP").GetComponent<TMPro.TMP_Text>();
        if (HPDisplay != null) //Si se puede mostrar HP, mostrarla
        {
            HPDisplay.text = $"Boss HP: {Mathf.Max(currentHP, 0)}/{maxHP}";
        }
        //if (ProgressManager.beatBear) Destroy(gameObject);
        //Preparaciones generales
        
    }


    private void Update()
    {
        if (Pause.paused) return;
        if (CameraController.inCutscene) return;
        if (state == BALL) return;
        detectPlayer(); 
        
        if (state == SEEKING) //Si se persigue al jugador, atacarlo cuando sea posible 
        {
            findDirection();
            anim.SetBool("Walking", true);
            if (Vector3.Distance(transform.position, playerTranform.position) < slamRange && canSlam) //Slam
            {
                state = ATTACKING;
                anim.SetTrigger("Slam");
                anim.SetBool("Walking", false);
                canSlam = false;
                navMeshAgent.isStopped = true;
            }
           
            if (Vector3.Distance(transform.position, playerTranform.position) < clawRange && canClaw) //Claw
            {
                state = CLAWING;
                anim.SetTrigger("Claw");
                anim.SetBool("Walking", false);
                canClaw = false;
                navMeshAgent.isStopped = true;
            }
            
            if (Vector3.Distance(transform.position, playerTranform.position) > minRushDistance && canRush) //Rush
            {
                RushAttack();
                anim.SetTrigger("Rush");
                anim.SetBool("Walking", false);
                navMeshAgent.enabled = false;
            }

        }
        else if(state == IDLE) //Si no sigue al jugador, moverse a una posicion random
        {
            if (hasReachedDestination(randomPosition)) 
            {
                randomPosition = new Vector2
                                (
                                    Random.Range(randomMovementDimensions[0].x, randomMovementDimensions[1].x),
                                    Random.Range(randomMovementDimensions[0].z, randomMovementDimensions[1].z)
                                );
            }
            else 
            {
                setDestination(randomPosition);
                anim.SetBool("Walking", true);
            }
        }
        if(state == RUSHING) //Si hace un ataque de Rush, frenar si se aleja mucho del jugador
        {
            
            if (Vector3.Distance(transform.position, playerTranform.position) > escapeDistance && !isAngered)
            {
                RushReset();
            }
            
        }
    }

    protected override void OnDestroy()
    {
        if (ProgressManager.beatBear)
        {
            if (HPDisplay != null) HPDisplay.gameObject.SetActive(false);
            ProgressManager.refoughtBear = true;
            return;
        }
        if (!PlayerActions.dead)
        {
            if (SoundSingleton.Instance != null)
            {
                ProgressManager.beatBear = true;
                SoundSingleton.Instance.OsoMuerte();
            }
            if(HPDisplay != null) HPDisplay.gameObject.SetActive(false);
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
                    ParticleSystem ps = Instantiate(slamParticle, transform.position + rushCollider.center * 2, Quaternion.identity);
                    ps.gameObject.transform.up = -transform.forward;
                    ps.Play();
                    RushReset();
                    AudioSource aS = Instantiate(crashSound, transform.position, Quaternion.identity);
                    Destroy(aS.gameObject, aS.clip.length);
                }
            }
        }
        else
        {
            if (rushCollider.enabled) //Si la colision fue con cualquier otra cosa y esta haciendo Rush, detener Rush
            {
                ParticleSystem ps = Instantiate(slamParticle, transform.position + rushCollider.center * 2, Quaternion.identity);
                ps.gameObject.transform.up = -transform.forward;
                ps.Play();
                RushReset();
            }
        }

    }

    private void FixedUpdate()
    {
        if (Pause.paused) return;
        if (CameraController.inCutscene) return;
        if (state == RUSHING) //Si hace Rush, moverse como Rush
        {
            moveRush();
        }
    }

    //Las siguientes funciones son usadas para llamado detro de animaciones
    public void slamAttack()
    {
        slamCollider.enabled = true;
        ParticleSystem partSys = Instantiate(slamParticle, slamLocation.position, Quaternion.identity);
        partSys.Play();
        partSys = Instantiate(holeParticle, slamLocation.position, Quaternion.identity);
        partSys.transform.forward = -transform.up;
        partSys.Play();
        AudioSource aS = Instantiate(slamSound, transform.position, Quaternion.identity);
        aS.pitch = Random.Range(0.8f, 1.2f);
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);
    }

    public void slamReset()
    {
        slamCollider.enabled = false;
        Stun(0.2f);
        StartCoroutine(slamReload());
    }

    private IEnumerator slamReload() //Para Invoke
    {
        float t = 0;
        while (t < 10f)
        {
            t += Time.deltaTime;
            yield return null;
        }
        canSlam = true;
        yield break;
    }

    public void clawAttack() 
    {
        clawCollider.enabled = true;
        AudioSource aS = Instantiate(clawSound, transform.position, Quaternion.identity);
        aS.pitch = Random.Range(0.8f, 1.2f);
        canParry = true;
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);
    }

    public void clawReset() 
    {
        clawCollider.enabled = false;
        canParry = false;
        Stun(0.2f);
        Invoke("clawReload", 1.5f);
    }

    private void clawReload() //Para Invoke
    {
        canClaw = true;
    }

    private void RushAttack() //Iniciar ataque de Rush
    {
        rushCollider.enabled = true;
        navMeshAgent.isStopped = true;
        rushDirection = dir;
        currentRushParticle = Instantiate(rushChargeParticle, transform.position, Quaternion.LookRotation(transform.up));
        state = RUSHING;
        canMoveRush = false;
        canParry = true;
        AudioSource aS = Instantiate(rushSound, transform);
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);
        Invoke("StartRush", rushChargeTime);
    }

    private void StartRush()
    {
        if (currentRushParticle != null)
        {
            Destroy(currentRushParticle.gameObject);
        }
        canMoveRush = true;
        currentRushParticle = Instantiate(rushParticle, rushPartTransform);
        canRush = false;
        rushCR = StartCoroutine(RushTimer());
    }

    private IEnumerator RushTimer()
    {
        float t = 0;

        while (t < maxRushTime)
        {
            t += Time.deltaTime;
            yield return null;
        }

        RushReset();
        
        rushCR = null;
        yield break;

    }

    public void RushReset() //Volver a base de Rush
    {
        rushCollider.enabled = false;
        navMeshAgent.enabled = true;
        Destroy(currentRushParticle.gameObject);
        anim.SetTrigger("Crash");
        canParry = false;
        currentCrashPS = Instantiate(crashParticleSystem, crashPartTransform);
        Destroy(currentCrashPS.gameObject, rushStunTime);
        //Invoke("DeleteCrashPS", rushStunTime);
        Stun(rushStunTime);
        if (rushCR != null)
        {
            StopCoroutine(rushCR);
            rushCR = null;
        }
        Invoke("RushReload", 15f);
        canMoveRush = false;
    }

    private void DeleteCrashPS()
    {
        if (currentCrashPS != null) Destroy(currentCrashPS.gameObject);
    }

    private void RushReload() //Para Invoke
    {
        canRush = true;
    }

    public void StartBall() //Iniciar Ball
    {
        if (state != SEEKING && state != IDLE && state != STUNNED) return;
        if (canBall)
        {
            Debug.Log("ball");
            canBall = false;
            DoBall();
        }
    }
    
    private void DoBall() //Para Invoke, usado para volver Oso a Ball
    {
        
        state = BALL;
        anim.SetBool("ball", true);
        foreach (BoxCollider bc in bearColliders) //Desabilitar todos los colliders
        {
            bc.enabled = false;
        }
        rb.constraints = RigidbodyConstraints.None; //Desbloquear rotacion de la bola
        ballCollider.enabled = true;
        navMeshAgent.isStopped = true;
        rb.AddForce(-transform.forward * ballImpulse,ForceMode.Impulse); //Empujar
        
        Invoke("EndBall", 1f);
    }
    
    private void EndBall() //Para Invoke, volver a base de Ball
    {
        rb.rotation = Quaternion.identity; 
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        transform.rotation = Quaternion.identity; //Hacer rotacion Default

        anim.SetBool("ball", false);
        navMeshAgent.isStopped = false;
        Invoke("AllowBall", ballWaitTime);
        state = SEEKING;

        foreach (BoxCollider bc in bearColliders) //Activar todos los colliders no trigger
        {
            if (!bc.isTrigger)
            {
                bc.enabled = true;
            }
        }
        ballCollider.enabled = false;
    }

    private void AllowBall()
    {
        canBall = true;
        ballTriggerCollider.enabled = true;
    }

    private void moveRush()
    {
        if (canMoveRush)
        {
            findDirection();
            rushDirection = Vector3.Lerp(transform.forward, dir, 1 - Mathf.Pow(0.7f, Time.deltaTime)); //Girar lentamente
            transform.forward = rushDirection;
            rb.velocity = new Vector3(rushDirection.x, 0, rushDirection.z).normalized * rushSpeed * (slowed? slowMult : 1) + Vector3.up * rb.velocity.y; //Mover
        }
        else
        {
            findDirection();
            rushDirection = Vector3.Lerp(transform.forward, dir, 1 - Mathf.Pow(0.1f, Time.deltaTime)); //Girar lentamente
            transform.forward = rushDirection;
        }
    }
}
