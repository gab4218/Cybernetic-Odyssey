using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


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
    public bool canParry = false;
    public bool invincible = false;

    //Variables basicas modificables en el editor que un enemigo podria tener
    [SerializeField] protected int maxHP;
    [SerializeField] protected float speed;
    [SerializeField] protected float detectionDistance = 15f;
    [SerializeField] protected float escapeDistance = 20f;
    [SerializeField] protected float calmTime = 15f;
    [SerializeField] protected float positionThreshold = 0.5f;
    [SerializeField] protected float randomMovementRadius = 0;
    [SerializeField] protected bool isAerial;
    [SerializeField] protected GameObject strongCollidersGO;
    [SerializeField] protected GameObject weakCollidersGO;
    [SerializeField] protected GameObject ignoreCollidersGO;
    [SerializeField] protected float fireRadius = 2;
    [SerializeField] protected ParticleSystem fireParticleSystem, iceParticleSystem;
    [SerializeField] protected bool canSlow = true;
    [SerializeField] protected int armorHealth = 300; 
    [SerializeField] protected TMP_Text HPDisplay; //Para debug
    [SerializeField] protected bool removeCollider = false;
    [SerializeField] protected GameObject[] crystals;
    [SerializeField] protected bool drops = true;
    //Otras variables comunes de enemigo
    protected ParticleSystem currentFirePS, currentIcePS;
    protected Vector3[] randomMovementDimensions;
    public float weakPointMult = 2;
    public float strongPointMult = 0;
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
    public bool shielded = false;


    public virtual void ShieldDamage(int dmg)
    {
        return;
    }

    protected virtual void Start()
    {
        //Preparaciones
        rb = GetComponentInChildren<Rigidbody>();
        currentHP = maxHP;
        if (HPDisplay != null) //Si se puede mostrar HP, mostrarla
        {
            HPDisplay.text = $"Boss HP: {Mathf.Max(currentHP, 0)}/{maxHP}";
        }
        player = FindObjectOfType<PlayerActions>();
        playerTranform = player.transform;
        state = IDLE;
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null) originalSpeed = navMeshAgent.speed;
        else originalSpeed = speed;
        
        if (ignoreCollidersGO != null)
        {
            ignoreColliders = ignoreCollidersGO.GetComponents<Collider>();
        }

        if (weakCollidersGO != null)
        {
            weakColliders = weakCollidersGO.GetComponents<Collider>();
        }

        if (strongCollidersGO != null)
        {
            strongColliders = strongCollidersGO.GetComponentsInChildren<Collider>();
            if (weakColliders != null)
            {
                List<Collider> c = strongColliders.ToList();
                foreach (Collider col in weakColliders)
                {
                    if (c.Contains(col)) c.Remove(col);
                }
                if (removeCollider && c.Contains(strongCollidersGO.GetComponent<Collider>()))
                {
                    c.Remove(strongCollidersGO.GetComponent<Collider>());
                }


                strongColliders = c.ToArray();
            }
        }

        if (isAerial)
        {
            randomMovementDimensions = new Vector3[]
            {
                new Vector3(transform.position.x - randomMovementRadius, transform.position.y - randomMovementRadius, transform.position.z - randomMovementRadius),
                new Vector3(transform.position.x + randomMovementRadius, transform.position.y + randomMovementRadius, transform.position.z + randomMovementRadius)
            };
        }
        else
        {
            randomMovementDimensions = new Vector3[]
            {
                new Vector3(transform.position.x - randomMovementRadius, transform.position.y, transform.position.z - randomMovementRadius),
                new Vector3(transform.position.x + randomMovementRadius, transform.position.y, transform.position.z + randomMovementRadius)
            };
        }
    }

    protected virtual void OnDestroy()
    {
        if (enemySpawner != null) enemySpawner.enemyCount--;
    }
    protected virtual void detectPlayer() //Detectar jugador
    {
        if ((Vector3.Distance(transform.position, playerTranform.position) <= detectionDistance * (player.isCrouched ? 0.5f : 1) || isAngered) && state == IDLE) //Si el jugador esta dentro del radio de deteccion y estado = idle, cambiar a buscar
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
        if (navMeshAgent != null && navMeshAgent.enabled)
        {

            navMeshAgent.destination = playerTranform.position;
        }
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


    protected void WaitDamage()
    {
        invincible = false;
    }

    public virtual void takeDamage(int dmg, PlayerActions.damageType dmgType)
    {
        invincible = true;
        Invoke("WaitDamage", 0.1f);
        
        currentHP -= (int)(dmg * (dmgType == PlayerActions.damageType.Acid ? 1.5f : 1)); //Restar HP acorde al tipo de damage recibido
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
                if (currentIcePS == null)
                {
                    currentIcePS = Instantiate(iceParticleSystem, transform.position, Quaternion.identity);
                    ParticleSystem.ShapeModule sphere = currentIcePS.shape;
                    sphere.radius = fireRadius;
                }
                if (iceCoroutine != null)
                {
                    StopCoroutine(iceCoroutine);
                }
                if (navMeshAgent != null)
                {
                    navMeshAgent.speed = originalSpeed * slowMult;
                }
                speed = originalSpeed * slowMult;
                iceCoroutine = StartCoroutine(IceTimer());
                slowed = true;

            }
        }
        if (HPDisplay != null) //Si se puede mostrar HP, mostrarla
        {
            HPDisplay.text = $"Boss HP: {Mathf.Max(currentHP, 0)}/{maxHP}";
        }
        if (currentHP <= 0) //Si muerto, destruir
        {
            if (iceCoroutine != null) StopCoroutine(iceCoroutine);
            if (fireCoroutine != null) StopCoroutine(fireCoroutine);
            if (calmCoroutine != null) StopCoroutine(calmCoroutine);
            if (drops)
            {
                for (int i = 0; i < 12; i++)
                {
                    Instantiate(crystals[Random.Range(0, crystals.Length)], transform.position + Random.insideUnitSphere, Quaternion.LookRotation(Vector3.up));
                }
            }

            Destroy(gameObject);
        }
        
        isAngered = true;
        if (calmCoroutine != null)
        {
            StopCoroutine(calmCoroutine);
        }
        calmCoroutine = StartCoroutine(CalmDown());
        
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
    
    public void WeakenArmor(PlayerActions.damageType dmgType)
    {
        if (armorHealth > 0)
        {
            armorHealth -= dmgType == PlayerActions.damageType.Fire ? 3 : 1;
        }
    }

    protected void Stun(float stunTime) //Stunnear por un periodo de tiempo
    {
        state = STUNNED;
        Invoke("Destun", stunTime);
        if (navMeshAgent != null) navMeshAgent.isStopped = true;
    }

    private void Destun() //Usado para Invoke
    {
        state = IDLE;
        if(navMeshAgent!=null) navMeshAgent.isStopped = false;
        
    }

    protected IEnumerator FireDamage()
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
                    HPDisplay.text = $"Boss HP: {Mathf.Max(currentHP, 0)}/{maxHP}";
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

    protected IEnumerator IceTimer()
    {
        float t = 0;
        while (t < slowTime)
        {
            t += Time.deltaTime;
            currentIcePS.gameObject.transform.position = transform.position;
            if (t % fireFrequency < Time.deltaTime)
            {
                currentHP --;
                if (HPDisplay != null) //Si se puede mostrar HP, mostrarla
                {
                    HPDisplay.text = $"Boss HP: {Mathf.Max(currentHP, 0)}/{maxHP}";
                }
                if (currentHP <= 0) //Si muerto, destruir
                {
                    Destroy(gameObject);
                }
            }
            yield return null;
        }
        slowed = false;
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = originalSpeed;
        }
        speed = originalSpeed;
        Destroy(currentIcePS.gameObject);
        currentIcePS = null;
        yield break;
    }
}
