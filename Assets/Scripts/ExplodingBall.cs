using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExplodingBall : EnemyBase
{
    private Animator anim;
    [SerializeField] private float explosionKnockback; //knockback de la explosion
    [SerializeField] private float explosionDelay; //cuanto tarda en explotar
    [SerializeField] private int explosionDamage; //damage de la explosion
    [SerializeField] private SphereCollider explosionCollider; //explosion
    [SerializeField] private GameObject[] crystals;
    private float delayCurrent; //guarda timer actual
    private bool canMove; //si se puede mover
    private bool timerOff; //si el timer no corre
    private bool collision; //si esta en el collider
    public ParticleSystem partExp;
    public AudioSource source;
    public AudioClip explosion;
    private Color originalColor;
    private MeshRenderer mRenderer;
    

    protected override void Start()
    {
        base.Start();
        explosionCollider.enabled = true;
        canMove = true;
        timerOff = true;
        collision = false;
        delayCurrent = explosionDelay;
        mRenderer = GetComponentInChildren<MeshRenderer>();
        originalColor = mRenderer.materials[1].color;
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
            mRenderer.materials[1].color = Color.Lerp(originalColor, Color.white, 1 - delayCurrent/explosionDelay);
        }
        else if (collision == false && delayCurrent < explosionDelay)
        {
            delayCurrent += Time.fixedDeltaTime; //retrocede timer
            mRenderer.materials[1].color = Color.Lerp(originalColor, Color.white, 1 - delayCurrent / explosionDelay);
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
                AudioSource aS = Instantiate(source, transform.position, Quaternion.identity);
                aS.pitch = Random.Range(0.8f, 1.2f);
                aS.Play();
                Destroy(gameObject);
            }
        }
    }

    public override void takeDamage(int dmg, PlayerActions.damageType dmgType)
    {
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
            HPDisplay.text = $"Bear HP: {Mathf.Max(currentHP, 0)}/{maxHP}";
        }
        if (currentHP <= 0) //Si muerto, destruir
        {
            if (iceCoroutine != null) StopCoroutine(iceCoroutine);
            if(fireCoroutine != null) StopCoroutine(fireCoroutine);
            if(calmCoroutine != null) StopCoroutine(calmCoroutine);

            for (int i = 0; i < 5; i++)
            {
                Instantiate(crystals[Random.Range(0, crystals.Length)], transform.position + Random.insideUnitSphere, Quaternion.LookRotation(Vector3.up));
            }
            Destroy(gameObject);
        }
        if (player.isCrouched && player.isArmored)
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

    private void OnTriggerExit(Collider other)
    {
        collision = false;
    }
}
