using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerActions : MonoBehaviour
{

    //Variables de UI y feedback visual
    [SerializeField] Transform cameraTransform;
    [SerializeField] GameObject inventoryPlaceholder;
    [SerializeField] TMP_Text HPDisplay;
    [SerializeField] TMP_Text dmgTypeDisplay;
    [SerializeField] Image crosshair;
    [SerializeField] Image grappleIMG;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] TrailRenderer bulletPrefab;
    [SerializeField] Gradient[] bulletColors;

    [Header("Inputs")] //Teclas de input
    [SerializeField] KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] KeyCode inventoryKey = KeyCode.I;
    [SerializeField] KeyCode Key1 = KeyCode.Alpha1, Key2 = KeyCode.Alpha2, Key3 = KeyCode.Alpha3;
    [SerializeField] KeyCode grappleKey = KeyCode.F;

    [Header("Parameters")] //Parametros posiblemente modificados en el editor
    [SerializeField] float interactDistance;
    [SerializeField] int maxHP = 100;
    [SerializeField] int dmgPerPellet = 10;
    [SerializeField] float readyWeaponTime = 0.25f;
    [SerializeField] float grappleDistance = 15f;
    [SerializeField] float grappleDelay = 5.0f;
    [SerializeField] float healingTime = 5.0f;
    [SerializeField] int healingRate = 1;
    [SerializeField] private float fallOffStart = 10f;
    [SerializeField] private float fallOffDistace = 50f;

    //Otras variables
    public int currentHP;
    bool canGetHit = true;
    int damageType = 0;
    bool canShoot = true;
    private Ray facingRay;
    private Inventory inventory;
    private PlayerMovement playerMovement;
    private bool canGrapple = false;
    private bool canHeal = false;
    private Animator anim;

    private void Start()
    {
        inventoryPlaceholder.SetActive(false);
        currentHP = maxHP;
        anim = GetComponentInChildren<Animator>();
        inventory = GetComponent<Inventory>();
        playerMovement = GetComponent<PlayerMovement>();
        foreach (int i in inventory.getEnabledUpgrades()) //Habilitar todas las mejoras activadas al iniciar
        {
            enableUpgrade(i);
        }
        //Preparaciones

    }


    private void Update()
    {

        facingRay = new Ray(cameraTransform.position, cameraTransform.forward); //Crear rayo en direccion a donde mira el jugador

        HPDisplay.text = $"{currentHP}/{maxHP}"; //Mostrar HP

        if (Input.GetKeyDown(Key1)) //Tipo de damage
        {
            damageType = 0;
        }
        if(Input.GetKeyDown(Key2))
        {
            damageType = 1;
        }
        if (Input.GetKeyDown(Key3))
        {
            damageType = 2;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha0)) //DELETE LATER (reiniciar escena para debug)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(grappleKey) && playerMovement.GetGrappleState())
        {
            playerMovement.StopGrapple();
        }

        if (Input.GetKeyDown(grappleKey) && canGrapple) //Grapple
        {
            ShootGrapple();
        }

        if (transform.position.y < -20) //Morir si Out Of Bounds
        {
            OOBDie();
        }

        dmgTypeDisplay.text = $"Damage type: {damageType.ToString()}"; //Mostrar tipo de damage

        if (Input.GetKeyDown(shootKey) && canShoot && Time.timeScale > 0) //Disparar
        {
            crosshair.color = Color.red;

            shoot();
        }


        if (Input.GetKeyDown(interactKey) && Time.timeScale > 0) //Interactuar
        {

            if (Physics.Raycast(facingRay, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
                {
                    interactable.onInteract();
                    anim.SetTrigger("Interact");
                }
            }
        }

        if (Input.GetKeyDown(inventoryKey)) //Inventario
        {
            if (inventoryPlaceholder.activeSelf)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                inventoryPlaceholder.SetActive(false);
                Time.timeScale = 1.0f;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                inventoryPlaceholder.SetActive(true);
                Time.timeScale = 0.0f;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) && inventoryPlaceholder.activeSelf) //Salir de inventario
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inventoryPlaceholder.SetActive(false);
            Time.timeScale = 1.0f;
        }

    }

    private void shoot()
    {
        if (Physics.Raycast(facingRay, out RaycastHit hit)) //Si dispara a algun lugar valido, hacer feedback visual y calcular multiplicador por distancia
        {
            float dist = Vector3.Distance(transform.position, hit.point);
            float dmgDistanceMult = Mathf.Max(0, (fallOffDistace - dist)/fallOffDistace);
            
            TrailRenderer trail = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
            trail.colorGradient = bulletColors[damageType];
            StartCoroutine(SpawnTrail(trail, hit));
            canShoot = false;
            Invoke("readyWeapon", readyWeaponTime);
            EnemyBase enemy = hit.collider.gameObject.GetComponentInParent<EnemyBase>();
            if (enemy != null && !hit.collider.isTrigger) //Si dispara a un enemigo, hacer damage
            {
                enemy.takeDamage(dist > fallOffStart? Mathf.RoundToInt(dmgPerPellet * dmgDistanceMult) : dmgPerPellet, damageType);
            }
        }
    }

    IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit Hit) //Feedback visual de disparo
    {
        float time = 0;
        Vector3 startPos = Trail.transform.position;
        while (time < 1)
        {
            Trail.transform.position = Vector3.Lerp(startPos, Hit.point, time);
            time += Time.deltaTime/Trail.time;
            yield return null;
        }
        Trail.transform.position = Hit.point;

        Destroy(Trail.gameObject, Trail.time);

    }

    private void readyWeapon() //Para invoke
    {
        canShoot = true;
        crosshair.color = Color.white;
    }


    public void takeDamage(int dmg) //Recibir damage
    {
        if (canGetHit)
        {
            currentHP -= dmg;
            canGetHit = false;
            Invoke("resetDamage", 0.5f);
            canHeal = false;
            StopCoroutine(CheckHeal());
            StopCoroutine(Heal());
            if (currentHP <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void resetDamage() //Para Invoke
    {
        StartCoroutine(CheckHeal());
        canGetHit = true;
    }

    

    public void enableUpgrade(int upgrade) //Activar efecto de mejora
    {
        switch (upgrade)
        {
            case 1:
                playerMovement.allowedToSlide = true;
                break;
            case 2:
                canGrapple = true;
                grappleIMG.color = new Color(70, 50, 231);
                break;
            case 3:
                playerMovement.ChangeWalljump(true);
                break;

            default:
                break;
        }
    }

    public void disableUpgrade(int upgrade) //Deshabilitar efecto de mejora
    {
        switch (upgrade)
        {
            case 1:
                playerMovement.allowedToSlide = false;
                break;
            case 2:
                canGrapple = false;
                grappleIMG.color = Color.red;
                break;
            case 3:
                playerMovement.ChangeWalljump(false);
                break;
            default:
                break;
        }
    }

    private void OOBDie() //Morir si OOB
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShootGrapple() //Disparar Grapple
    {
        canGrapple = false;
        grappleIMG.color = Color.red;
        StartCoroutine(GrappleReload());
        if (Physics.Raycast(facingRay, out RaycastHit hit, grappleDistance))
        {
            if(hit.collider.gameObject != null)
            {
                playerMovement.GrappleTo(hit.point);
            }
        }
    }

    private IEnumerator CheckHeal() //Revisar si se puede curar
    {
        float timer = 0f;
        while (timer < healingTime)
        {
            timer += Time.deltaTime;

            yield return null;
        }
        canHeal = true;
        StartCoroutine(Heal());
        yield break;
    }

    private IEnumerator Heal() //Curar
    {
        while (canHeal && currentHP < maxHP)
        {
            currentHP += currentHP > (maxHP-healingRate)? (maxHP-currentHP) : healingRate;
            yield return new WaitForSeconds(0.1f);
        }
        yield break;
    }

    private IEnumerator GrappleReload() //Recargar grapple
    {
        
        float timer = 0f;
        while (timer < grappleDelay)
        {
            timer += Time.deltaTime;
            
            yield return null;
        }
        canGrapple = true;
        grappleIMG.color = new Color(70f, 50f, 231f);
        yield break;
    }

    

}
