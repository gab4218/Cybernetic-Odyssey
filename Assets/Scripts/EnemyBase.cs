using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour
{
 

    //Variables de estado
    protected const int IDLE = 0;
    protected const int SEEKING = 1;
    protected const int ATTACKING = 2;
    protected const int STUNNED = 3;
    public int state;
    protected bool isAngered = false;
    protected bool canCalm = true;
    //Variables basicas modificables en el editor que un enemigo podria tener
    [SerializeField] protected int maxHP;
    [SerializeField] protected float speed;
    [SerializeField] protected float detectionDistance = 15f;
    [SerializeField] protected float escapeDistance = 20f;
    [SerializeField] protected float calmTime = 15f;
    [SerializeField] protected float positionThreshold = 0.5f;
    [SerializeField] protected Vector3[] randomMovementDimensions;
    [SerializeField] protected GameObject strongCollidersGO;
    [SerializeField] protected GameObject weakCollidersGO;
    [SerializeField] protected GameObject ignoreCollidersGO;
    [SerializeField] protected float fireRadius = 2;
    [SerializeField] protected ParticleSystem fireParticleSystem;
    [SerializeField] protected bool canSlow = true;

    ParticleSystem currentFirePS;
    [SerializeField] protected TMP_Text HPDisplay; //Para debug

    //Otras variables comunes de enemigo
    public int currentHP;
    public Collider[] ignoreColliders;
    public Collider[] weakColliders;
    public Collider[] strongColliders;
    public EnemySpawner enemySpawner;
    protected Rigidbody rb;
    protected int fireDamage = 2;
    protected NavMeshAgent navMeshAgent;
    protected float fireFrequency = 0.25f;
    protected float fireTime = 5;
    protected float slowMult = 0.75f;
    protected float slowTime = 5f;
    protected bool slowed = false;
    protected bool onFire = false;
    //Variables para deteccion de jugador
    protected Transform playerTranform;
    protected PlayerActions player;
    protected Vector3 dir;
    protected Coroutine fireCoroutine;
    protected Coroutine calmCoroutine;
    protected Coroutine iceCoroutine;
    protected float originalSpeed;
    


    protected virtual void Start()
    {
        //Preparaciones
        rb = GetComponentInChildren<Rigidbody>();
        currentHP = maxHP;
        player = FindObjectOfType<PlayerActions>();
        playerTranform = player.transform;
        state = IDLE;
        navMeshAgent = GetComponent<NavMeshAgent>();
        originalSpeed = navMeshAgent.speed;
        if (ignoreCollidersGO != null)
        {
            ignoreColliders = ignoreCollidersGO.GetComponents<Collider>();
        }

        if (strongCollidersGO != null)
        {
            strongColliders = strongCollidersGO.GetComponents<Collider>();
        }

        if (weakCollidersGO != null)
        {
            weakColliders = weakCollidersGO.GetComponents<Collider>();
        }
    }

    private void OnDestroy()
    {
        enemySpawner.enemyCount--;
    }
    protected virtual void detectPlayer() //Detectar jugador
    {
        if ((Vector3.Distance(transform.position, playerTranform.position) <= detectionDistance * (player.isCrouched? 0.5f : 1) || isAngered) && state == IDLE) //Si el jugador esta dentro del radio de deteccion y estado = idle, cambiar a buscar
        {
            state = SEEKING;
        }
        else if (state == SEEKING && Vector3.Distance(transform.position, playerTranform.position) >= escapeDistance && !isAngered) //Si el jugador esta fuera del radio de escape y estado = buscar, cambiar a idle
        {
            state = IDLE;
        }
    }

    //Las siguientes funciones fueron nombradas iguales porque me gustan las funciones con multiples overrides

    protected void findDirection() //Si la llamada de la funcion no toma argumentos, mirar a la proyeccion de la posicion del jugador en el plano xz
    {
        dir = playerTranform.position - transform.position;
        dir.y = 0;
        dir.Normalize();
        navMeshAgent.destination = playerTranform.position;
    }
    
    protected void setDestination(Vector2 newSpot) //Si la llamada de la funcion toma un Vector2, mirar a la proyeccion de la posicion pasada en el plano xz
    {
        navMeshAgent.destination = new Vector3(newSpot.x, transform.position.y, newSpot.y);

        dir = navMeshAgent.destination - transform.position;
        dir.Normalize();
    }

    protected void setDestination(Vector3 newSpot) //Si la llamada de la funcion toma un Vector3, mirar a la posicion pasada
    {
        navMeshAgent.destination = newSpot;
        dir = newSpot - transform.position;
        dir.Normalize();
    }

    protected bool hasReachedDestination(Vector2 targetPos) //Si la llamada de la funcion toma un Vector2, chequear si la posicion esta dentro de una tolerancia de la proyeccion en el plano xz del vector
    {
        
        return Vector3.Distance(transform.position, new Vector3(targetPos.x, transform.position.y, targetPos.y)) <= positionThreshold;
        
    }

    protected bool hasReachedDestination(Vector3 targetPos) //Si la llamada de la funcion toma un Vector3, chequear si la posicion esta dentro de una tolerancia del vector
    {
       return Vector3.Distance(transform.position, targetPos) <= positionThreshold;

    }



    public virtual void takeDamage(int dmg, PlayerActions.damageType dmgType) 
    {
        currentHP -= (int)(dmg * (dmgType == PlayerActions.damageType.Acid? 1.1f : 1)); //Restar HP acorde al tipo de damage recibido
        if (dmgType == PlayerActions.damageType.Fire)
        {
            if (currentFirePS == null)
            {
                currentFirePS = Instantiate(fireParticleSystem, transform.position, Quaternion.identity);
                currentFirePS.gameObject.transform.SetParent(transform, true);

                ParticleSystem.ShapeModule sphere = currentFirePS.shape;
                sphere.radius = fireRadius;
            }
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
            }
            fireCoroutine = StartCoroutine(FireDamage());
        }
        if (!slowed && dmgType == PlayerActions.damageType.Ice)
        {
            if (canSlow)
            {
                if (iceCoroutine != null)
                {
                    StopCoroutine(iceCoroutine);
                }
                navMeshAgent.speed = originalSpeed * slowMult;
                iceCoroutine = StartCoroutine(IceTimer());
                slowed = true;

            }
        }
        if (HPDisplay != null) //Si se puede mostrar HP, mostrarla
        {
            HPDisplay.text = $"Bear HP: {Mathf.Max(currentHP,0)}/{maxHP}";
        }
        if (currentHP <= 0) //Si muerto, destruir
        {
            Destroy(gameObject);
        }
        if (player.isCrouched && player.canGambleCrouch)
        {
            if (Random.Range(0, 1f) > 0.5f)
            {
                isAngered = true;
                if (calmCoroutine != null)
                {
                    StopCoroutine(calmCoroutine);
                }
                calmCoroutine = StartCoroutine(CalmDown());
            }
        }
        else
        {
            isAngered = true;
            if (calmCoroutine != null)
            {
                StopCoroutine(calmCoroutine);
            }
            calmCoroutine = StartCoroutine(CalmDown());
        }
    }

    protected IEnumerator CalmDown()
    {

        float t = 0;

        while (t < calmTime && canCalm)
        {
            t += Time.deltaTime;
            yield return null;
        }
        if (canCalm)
        {
            isAngered = false;
        }
        else
        {
            canCalm = true;
        }
        calmCoroutine = null;
    }

    protected void Stun(float stunTime) //Stunnear por un periodo de tiempo
    {
        state = STUNNED;
        Invoke("Destun", stunTime);
        navMeshAgent.isStopped = true;
    }

    private void Destun() //Usado para Invoke
    {
        state = IDLE;
        navMeshAgent.isStopped = false;
    }

    private IEnumerator FireDamage()
    {
        float t = 0;
        while (t < fireTime)
        {
            t += Time.deltaTime;
            
            if (t % fireFrequency < Time.deltaTime)
            {
                currentHP -= fireDamage;
                if (HPDisplay != null) //Si se puede mostrar HP, mostrarla
                {
                    HPDisplay.text = $"Bear HP: {Mathf.Max(currentHP, 0)}/{maxHP}";
                }
                if (currentHP <= 0) //Si muerto, destruir
                {
                    Destroy(gameObject);
                }
            }
            yield return null;
        }
        Destroy(currentFirePS.gameObject);
        currentFirePS = null;
        fireCoroutine = null;

    }

    private IEnumerator IceTimer()
    {
        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        Color oldColor = mr.material.color;
        mr.material.SetColor("_Color", new Vector4(0.8f, 1f, 1f, 1f));
        float t = 0;
        while (t < slowTime)
        {
            t += Time.deltaTime;
            yield return null;
        }
        mr.material.SetColor("_Color", new Vector4(oldColor.r,oldColor.g,oldColor.b,1f));
        slowed = false;
        navMeshAgent.speed = originalSpeed;
        yield break;
    }
}
