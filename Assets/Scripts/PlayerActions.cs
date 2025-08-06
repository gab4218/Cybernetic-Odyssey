using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerActions : MonoBehaviour
{

    public static bool dead = false;
    public static bool won = false;
    private bool grabbed = false;
    [Header("UI")] //Variables de UI y feedback visual
    [SerializeField] Material[] flamethrowerMats;
    [SerializeField] Transform cameraTransform;
    [SerializeField] GameObject inventoryPlaceholder;
    [SerializeField] Image HPDisplay;
    [SerializeField] Image crosshair;
    [SerializeField] Image grappleIMG;
    [SerializeField] Image overheatIMG;
    [SerializeField] Image overloadCooldownIMG;
    [SerializeField] Image overloadIMG;
    [SerializeField] Image hitImage;
    [SerializeField] Image uiBGIMG;
    [SerializeField] Sprite[] UIarray;
    [SerializeField] Color hitColor, missColor, critColor;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] TrailRenderer bulletPrefab;
    [SerializeField] Gradient[] bulletColors;
    [SerializeField] MeshFilter gunMeshFilter;
    [SerializeField] Mesh pistolMesh, shotgunMesh, flamethrowerMesh, rocketMesh;
    [SerializeField] Image pistolUnlockIMG, shotgunUnlockIMG, flamethrowerUnlockIMG, rocketUnlockIMG;
    [SerializeField] Sprite[] selectedOverloads;
    [SerializeField] Gradient[] overloadingColors;
    [SerializeField] ParticleSystem flamethrowerFirePS, shotPS, bulletHolePS;
    [SerializeField] Animator gunAnimator;
    [SerializeField] GameObject pistolHand, shotgunHand, flamethrowerHand, rifleHand, clawHand;
    [SerializeField] CameraController camContoller;
    [SerializeField] Image damagedIMG;
    [SerializeField] Image grappleIndicator;
    [SerializeField] RocketCollisonDetection rocket;
    [SerializeField] Image empIMG;
    [Header("Inputs")] //Teclas de input
    [SerializeField] KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] KeyCode inventoryKey = KeyCode.Tab;
    [SerializeField] KeyCode Key1 = KeyCode.Alpha1, Key2 = KeyCode.Alpha2, Key3 = KeyCode.Alpha3;
    [SerializeField] KeyCode grappleKey = KeyCode.F;
    [SerializeField] KeyCode healKey = KeyCode.Q;
    [SerializeField] KeyCode cheatKey = KeyCode.P;
    [SerializeField] Transform cheatTransform;
    [Header("Parameters")] //Parametros posiblemente modificados en el editor
    [SerializeField] float interactDistance;
    [SerializeField] float pistolCooldown = 0.33f;
    [SerializeField] float pistolFallOffStart = 10f;
    [SerializeField] float pistolFallOffMax = 40f;
    [SerializeField] float shotgunCooldown = 0.75f;
    [SerializeField] float shotgunFallOffStart = 2f;
    [SerializeField] float shotgunFallOffMax = 15f;
    [SerializeField] float shotgunPelletCount = 5f;
    [SerializeField] float shotgunPelletSpreadMax = 20f;
    [SerializeField] Collider flamethrowerCollider;
    [SerializeField] float flamethrowerOverheatTime = 5f;
    [SerializeField] float flamethrowerOverheatLength = 10f;
    [SerializeField] int maxHP = 100;
    [SerializeField] int dmgPerPellet = 10;
    [SerializeField] float grappleDistance = 15f;
    [SerializeField] float grappleDelay = 5.0f;
    [SerializeField] float healingTime = 5.0f;
    [SerializeField] int healingRate = 1;
    [SerializeField] float overloadTime = 10f;
    [SerializeField] float overloadCooldown = 20f;
    [SerializeField] LayerMask bounds;
    [SerializeField] private AudioSource badHit, midHit, goodHit, missHit, damagedSound, healSound, grappleSound;
    [SerializeField] private TMP_Text interactText;
    [SerializeField] private int slamDamage = 25;
    [SerializeField] private int parryHealing = 20;
    [SerializeField] private int meleeDamage = 70;
    [SerializeField] private float meleeCooldown = 0.4f;
    [SerializeField] private Collider meleeCollider;
    [SerializeField] private Mesh meleeMesh;
    [SerializeField] private AudioClip meleeClip;
    [SerializeField] private Mesh rifleMesh;
    [SerializeField] private float rifleCooldown = 0.15f;
    [SerializeField] float rifleFallOffStart = 20f;
    [SerializeField] float rifleFallOffMax = 50f;
    [SerializeField] float slowDmgMult = 1.5f;
    [SerializeField] float slowDmgSpeedDiv = 0.75f;
    [SerializeField] float addedKnockback = 1.25f;
    [SerializeField] private GameObject parryScreen;

    //Otras variables
    private Coroutine rifleCR;
    private float fallOffStart = 10f;
    private float fallOffDistace = 40f;
    float readyWeaponTime = 0.33f;
    public int currentHP;
    public Vector3 lastPosition;
    bool canGetHit = true;
    int selectedWeapon = 0;
    bool canShoot = true;
    private Ray facingRay;
    private Inventory inventory;
    private PlayerMovement playerMovement;
    private float rangeMult = 1;
    private bool canGrapple = false;
    private bool canHeal = false;
    private bool canParry = false;
    private Animator anim;
    private bool hasShotgun = false;
    private bool hasRocket = false;
    private bool hasFlamethrower = false;
    private bool hasMelee = false;
    private bool hasRifle = false;
    private bool isAllowedToOverload = false;
    private bool isAllowedToHeal = false;
    private bool canOverload = true;
    private bool canSlam = false;
    private bool allCrits = false;
    private bool slowDmg = false;
    private bool fastDmg = false;
    private bool knockerBacker = false;
    private List<bool> canHealMats = new List<bool>{ false, false, false };
    private bool haltHeal = false;
    private bool canFlamethrow = true;
    public ParticleSystem partMax;
    public ParticleSystem partMin;
    public ParticleSystem partMid;
    public bool isCrouched = false;
    public bool differentFlames = false;
    private float flamethrowerCurrentTime;
    private ParticleSystem.EmissionModule flamethrowerFire;
    Coroutine overheatCR, healCR, checkHealCR;
    private bool canChangeOverload = true;
    public AudioSource audioSource;
    public AudioClip gun;
    public AudioClip shotgun;
    public AudioClip flamethrower;
    public AudioClip rifle;
    public static bool isEMPd = false;


    public float overloadMult = 1;


    public enum damageType
    {
        None,
        Ice,
        Fire,
        Acid
    }

    public damageType dmgType = damageType.Fire;

    int selectedOverload = 0;
    private void Start()
    {
        if (parryScreen != null) parryScreen.SetActive(false);
        won = false;
        grappleIndicator.gameObject.SetActive(false);
        dead = false;
        inventoryPlaceholder.SetActive(false);
        currentHP = maxHP;
        anim = GetComponentInChildren<Animator>();
        inventory = GetComponent<Inventory>();
        playerMovement = GetComponent<PlayerMovement>();
        dmgType = damageType.Fire;
        ParticleSystem.ColorOverLifetimeModule pc2 = flamethrowerFirePS.colorOverLifetime;
        pc2.color = new ParticleSystem.MinMaxGradient(overloadingColors[0]);
        flamethrowerCollider.enabled = false;
        overloadIMG.gameObject.SetActive(false);
        overheatIMG.gameObject.SetActive(false);
        overloadCooldownIMG.gameObject.SetActive(false);
        grappleIMG.gameObject.SetActive(false);
        flamethrowerFire = flamethrowerFirePS.emission;
        flamethrowerFire.enabled = false;
        damagedIMG.color = new Color (damagedIMG.color.r, damagedIMG.color.g, damagedIMG.color.b, 0);
        flamethrowerUnlockIMG?.gameObject.SetActive(false);
        pistolUnlockIMG?.gameObject.SetActive(false);
        rocketUnlockIMG?.gameObject.SetActive(false);
        shotgunUnlockIMG?.gameObject.SetActive(false);
        uiBGIMG.sprite = UIarray[0];
        foreach (int i in Inventory.getEnabledUpgrades()) //Habilitar todas las mejoras activadas al iniciar
        {
            enableUpgrade(i);
        }
        if (Inventory.hasShotgun)
        {
            hasShotgun = true;
            unlockWeapon(0);
        }
        if (Inventory.hasFlamethrower)
        {
            hasFlamethrower = true;
            unlockWeapon(1);
        }

        if (Inventory.hasRocket)
        {
            hasRocket = true;
            unlockWeapon(2);
        }

        if (Inventory.hasMelee)
        {
            hasMelee = true;
            unlockWeapon(3);
        }

        if (Inventory.hasRifle)
        {
            hasRifle = true;
            unlockWeapon(4);
        }

        interactText.gameObject.SetActive(false);
        //Preparaciones


    }

    private IEnumerator WaitOverloadChange()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        canChangeOverload = true;
    }
    private void Update()
    {
        if (Pause.paused) return;
        if (CameraController.inCutscene) return;
        if (DialogueManager.instance != null)
        {
            if (DialogueManager.instance.inDialogue)
            {
                return;
            }
        }
        HPDisplay.fillAmount = currentHP * 1f / maxHP; //Mostrar HP
        if (Input.GetKeyDown(shootKey) && canShoot && Time.timeScale > 0) //Disparar
        {
            switch (selectedWeapon)
            {
                case 0:
                    shoot(facingRay);
                    ParticleSystem ps = Instantiate(shotPS, bulletSpawn);
                    ps.Play();
                    gunAnimator.SetTrigger("shot");
                    audioSource.pitch = Random.Range(0.8f, 1.2f);
                    audioSource.PlayOneShot(gun, 1);
                    break;
                case 1:
                    shootShotgun();
                    ParticleSystem ps1 = Instantiate(shotPS, bulletSpawn);
                    ps1.Play();
                    gunAnimator.SetTrigger("shot");
                    audioSource.pitch = Random.Range(0.8f, 1.2f);
                    audioSource.PlayOneShot(shotgun, 1);
                    break;
                case 2:
                    if (canFlamethrow)
                    {
                        if (differentFlames)
                        {
                            switch (selectedOverload)
                            {
                                case 0:
                                    dmgType = damageType.Ice;
                                    ParticleSystem.ColorOverLifetimeModule pc = flamethrowerFirePS.colorOverLifetime;
                                    pc.color = new ParticleSystem.MinMaxGradient(overloadingColors[1]);
                                    Debug.Log("ice");
                                    break;
                                case 1:
                                    dmgType = damageType.Fire;
                                    ParticleSystem.ColorOverLifetimeModule pc2 = flamethrowerFirePS.colorOverLifetime;
                                    pc2.color = new ParticleSystem.MinMaxGradient(overloadingColors[0]);
                                    Debug.Log("fire");
                                    break;
                                case 2:
                                    dmgType = damageType.Acid;
                                    ParticleSystem.ColorOverLifetimeModule pc3 = flamethrowerFirePS.colorOverLifetime;
                                    pc3.color = new ParticleSystem.MinMaxGradient(overloadingColors[2]);
                                    Debug.Log("acid");
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            dmgType = damageType.Fire;
                            ParticleSystem.ColorOverLifetimeModule pc2 = flamethrowerFirePS.colorOverLifetime;
                            pc2.color = new ParticleSystem.MinMaxGradient(overloadingColors[0]);
                        }
                        flamethrowerCollider.enabled = true;
                        flamethrowerFire.enabled = true;
                        gunAnimator.SetBool("flamethrower", true);
                        audioSource.clip = flamethrower;
                        audioSource.loop = true;
                        audioSource.Play();
                    }

                    break;
                case 3:
                    if (Physics.Raycast(facingRay))
                    {
                        FireRocket();
                        gunAnimator.SetTrigger("shot");
                        Invoke("readyWeapon", readyWeaponTime);
                    }
                    break;
                case 4:
                    MeleeAttack();
                    gunAnimator.SetTrigger("melee");
                    canShoot = false;
                    audioSource.pitch = Random.Range(0.8f, 1.2f);
                    audioSource.PlayOneShot(meleeClip, 1);
                    Invoke("readyWeapon", readyWeaponTime);
                    break;
                case 5:
                    if(rifleCR == null)
                    rifleCR = StartCoroutine(ShootRifle());
                    break;
                default:
                    shoot(facingRay);
                    break;

            }
        }

        if (Input.GetKeyUp(shootKey))
        {
            if (selectedWeapon == 2)
            {
                flamethrowerCollider.enabled = false;
                flamethrowerFire.enabled = false;
                gunAnimator.SetBool("flamethrower", false);
                audioSource.loop = false;
                audioSource.Stop();
            }
            else if (selectedWeapon == 5)
            {
                StopCoroutine(rifleCR);
                rifleCR = null;
            }
        }
        hitImage.color = Color.Lerp(hitImage.color, new Color(hitImage.color.r, hitImage.color.g, hitImage.color.b, 0), 1 - Mathf.Pow(0.05f, Time.deltaTime));
        if (flamethrowerCollider.enabled && canFlamethrow && Time.timeScale > 0)
        {
            if (flamethrowerCurrentTime < flamethrowerOverheatTime)
            {
                flamethrowerCurrentTime += Time.deltaTime;
            }
            else
            {
                flamethrowerCurrentTime = 0;
                canFlamethrow = false;
                flamethrowerCollider.enabled = false;
                flamethrowerFire.enabled = false;
                gunAnimator.SetBool("flamethrower", false);
                audioSource.loop = false;
                audioSource.Stop();
                if (overheatCR != null)
                {
                    StopCoroutine(overheatCR);
                }
                overheatCR = StartCoroutine(FlamethrowerOverheatOver());
            }
        }
        else if (flamethrowerCurrentTime > 0 && Time.timeScale > 0)
        {
            flamethrowerCurrentTime -= Time.deltaTime / 3;
        }
        else if (Time.timeScale > 0)
        {
            flamethrowerCurrentTime = 0;
        }

        if (canSlam)
        {

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (!playerMovement.grounded && !playerMovement.slamming)
                {
                    Slam();
                }
            }
        }

        facingRay = new Ray(cameraTransform.position, cameraTransform.forward); //Crear rayo en direccion a donde mira el jugador
        if (overheatIMG.gameObject.activeSelf && canFlamethrow && Time.timeScale > 0)
        {
            overheatIMG.fillAmount = (flamethrowerOverheatTime - flamethrowerCurrentTime) / flamethrowerOverheatTime;
            overheatIMG.color = Color.Lerp(Color.white, Color.red, flamethrowerCurrentTime / flamethrowerOverheatTime);
        }
        
        if (grabbed)
        {
            return;
        }


        if (Input.GetKeyDown(inventoryKey)) //Inventario
        {
            if (inventoryPlaceholder.activeSelf && Time.timeScale == 0)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                inventoryPlaceholder.SetActive(false);
                Time.timeScale = 1.0f;
            }
            else if (Time.timeScale > 0)
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

        if (Time.timeScale == 0) return;
        if (differentFlames && selectedWeapon == 2)
        {

            if (Input.mouseScrollDelta.y > 0 && canChangeOverload)
            {
                selectedOverload = (selectedOverload + 1) % 3;
                overheatIMG.material = flamethrowerMats[selectedOverload];
                canChangeOverload = false;
                StartCoroutine(WaitOverloadChange());
             
            }
            else if (Input.mouseScrollDelta.y < 0 && canChangeOverload)
            {
                selectedOverload = selectedOverload == 0? 2 : (selectedOverload - 1);
                overheatIMG.material = flamethrowerMats[selectedOverload];
                canChangeOverload = false;
                StartCoroutine(WaitOverloadChange());
                
            }
        }
        else if (selectedWeapon == 2)
        {
            overloadCooldownIMG.color = new Color(0.45f,0.5f,0.6f, 0f);
        }
        
        isCrouched = playerMovement.isCrouching;


        Vector3 v = cameraTransform.forward;
        v.y = 0;
        transform.forward = v.normalized;
        

        if (Input.GetKeyDown(Key1) && !Input.GetKey(KeyCode.Mouse0)) //Armas
        {
            selectedWeapon = 0;
            fallOffDistace = pistolFallOffMax;
            fallOffStart = pistolFallOffStart;
            readyWeaponTime = pistolCooldown / (slowDmg? slowDmgSpeedDiv : 1f);
            readyWeaponTime = readyWeaponTime * (fastDmg ? slowDmgSpeedDiv : 1f);
            gunMeshFilter.mesh = pistolMesh;
            pistolHand.SetActive(true);
            shotgunHand.SetActive(false);
            flamethrowerHand.SetActive(false);
            rifleHand.SetActive(false);
            clawHand.SetActive(false);
        }
        if(Input.GetKeyDown(Key2) && hasShotgun && !Input.GetKey(KeyCode.Mouse0))
        {
            selectedWeapon = 1;
            fallOffDistace = shotgunFallOffMax;
            fallOffStart = shotgunFallOffStart;
            readyWeaponTime = shotgunCooldown / (slowDmg ? slowDmgSpeedDiv : 1f);
            readyWeaponTime = readyWeaponTime * (fastDmg ? slowDmgSpeedDiv : 1f);
            gunMeshFilter.mesh = shotgunMesh;
            pistolHand.SetActive(false);
            shotgunHand.SetActive(true);
            flamethrowerHand.SetActive(false);
            rifleHand.SetActive(false);
            clawHand.SetActive(false);
        }
        if (Input.GetKeyDown(Key3) && hasFlamethrower && !Input.GetKey(KeyCode.Mouse0))
        {
            selectedWeapon = 2;
            gunMeshFilter.mesh = flamethrowerMesh;
            overloadIMG.gameObject.SetActive(false);
            pistolHand.SetActive(false);
            shotgunHand.SetActive(false);
            flamethrowerHand.SetActive(true);
            rifleHand.SetActive(false);
            clawHand.SetActive(false);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4) && hasRocket && !Input.GetKey(KeyCode.Mouse0))
        {
            selectedWeapon = 3;
            gunMeshFilter.mesh = rocketMesh;
            readyWeaponTime = 1.5f / (slowDmg ? slowDmgSpeedDiv : 1f);
            readyWeaponTime = readyWeaponTime * (fastDmg ? slowDmgSpeedDiv : 1f);
            pistolHand.SetActive(false);
            shotgunHand.SetActive(false);
            flamethrowerHand.SetActive(false);
            rifleHand.SetActive(false);
            clawHand.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) && hasMelee && !Input.GetKey(KeyCode.Mouse0))
        {
            selectedWeapon = 4;
            gunMeshFilter.mesh = meleeMesh;
            readyWeaponTime = meleeCooldown / (slowDmg ? slowDmgSpeedDiv : 1f);
            readyWeaponTime = readyWeaponTime * (fastDmg ? slowDmgSpeedDiv : 1f);
            pistolHand.SetActive(false);
            shotgunHand.SetActive(false);
            flamethrowerHand.SetActive(false);
            rifleHand.SetActive(false);
            clawHand.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6) && hasRifle && !Input.GetKey(KeyCode.Mouse0))
        {
            selectedWeapon = 5;
            gunMeshFilter.mesh = rifleMesh;
            readyWeaponTime = rifleCooldown / (slowDmg ? slowDmgSpeedDiv : 1f);
            readyWeaponTime = readyWeaponTime * (fastDmg ? slowDmgSpeedDiv : 1f);
            fallOffDistace = rifleFallOffMax;
            fallOffStart = rifleFallOffStart;
            pistolHand.SetActive(false);
            rifleHand.SetActive(true);
            shotgunHand.SetActive(false);
            flamethrowerHand.SetActive(false);
            clawHand.SetActive(false);
        }


        if (Input.GetKeyDown(healKey) && currentHP < maxHP && canHeal && !isAllowedToHeal)
        {
            for (int i = 0; i < 3; i++)
            {
                canHealMats[i] = inventory.hasMaterials(i, 1);
            }
            if (canHealMats.Contains(true))
            {
                inventory.removeFromInventory(canHealMats.IndexOf(true), 1);
                currentHP = Mathf.Min(currentHP + 50, maxHP);
                AudioSource aS = Instantiate(healSound, transform.position, Quaternion.identity);
                aS.Play();
                Destroy(aS.gameObject, aS.clip.length);
            }
        }
       
        if (Input.GetKeyDown(cheatKey) && cheatTransform != null)
        {
            Cheat();
        }

        if (isAllowedToOverload && canOverload && Input.GetKeyDown(KeyCode.R) && selectedWeapon != 2)
        {
            canOverload = false;
            overloadMult = 1.25f;
            playerMovement.overloadMult = overloadMult;
            overloadIMG.gameObject.SetActive(true);
            StartCoroutine(WaitOverload());
        }

        if (Input.GetKeyDown(KeyCode.Alpha0)) //DELETE LATER (reiniciar escena para debug)
        {
            dead = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(grappleKey) && playerMovement.GetGrappleState())
        {
            playerMovement.StopGrapple();
        }

        if (canGrapple) //Grapple
        {
            if (Physics.Raycast(facingRay, grappleDistance, bounds))
            {
                grappleIndicator.gameObject.SetActive(true);
            }
            else
            {
                grappleIndicator.gameObject.SetActive(false);
            }
            if (Input.GetKeyDown(grappleKey)) ShootGrapple();
        }

        if (transform.position.y < -20) //Morir si Out Of Bounds
        {
            OOBDie();
        }

        


        if (Physics.Raycast(facingRay, out RaycastHit hit, interactDistance) && hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
        {
            interactText.gameObject.SetActive(true);
            if (Input.GetKeyDown(interactKey)) //Interactuar
            {
                interactable.onInteract();
                anim.SetTrigger("Interact");
            }
        }
        else
        {
            interactText.gameObject.SetActive(false);
        }


    }


    private IEnumerator ShootRifle()
    {
        while (true)
        {
            shoot(facingRay);
            ParticleSystem ps1 = Instantiate(shotPS, bulletSpawn);
            ps1.Play();
            gunAnimator.SetTrigger("shot");
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(rifle, 1);
            float t = 0;
            while (t < readyWeaponTime)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }
    }


    public void MeleeAttack()
    {
        meleeCollider.enabled = true;
        Invoke("MeleeDisable", 0.1f);
    }

    private void MeleeDisable()
    {
        meleeCollider.enabled = false;
    }

    private void DeOverload()
    {
        overloadMult = 1;
        playerMovement.overloadMult = overloadMult;
    }

    private void shoot(Ray aimRay)
    {
        if (Physics.Raycast(aimRay, out RaycastHit hit)) //Si dispara a algun lugar valido, hacer feedback visual y calcular multiplicador por distancia
        {
            float dist = Vector3.Distance(transform.position, hit.point);
            
            
            TrailRenderer trail = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
            trail.colorGradient = bulletColors[(int)damageType.None];
            StartCoroutine(SpawnTrail(trail, hit));
            canShoot = false;
            Invoke("readyWeapon", (readyWeaponTime) / overloadMult);
            EnemyBase enemy = hit.collider.gameObject.GetComponentInParent<EnemyBase>();


            
            if (enemy != null && !hit.collider.isTrigger && dist < fallOffDistace) //Si dispara a un enemigo, hacer damage
            {
                if (!enemy.ignoreColliders.Contains(hit.collider))
                {
                    int damage = 0;
                    float mult = 1;
                    if (enemy.weakColliders.Contains(hit.collider))
                    {
                        if (selectedWeapon == 5) mult = 1;
                        else mult = enemy.weakPointMult * (allCrits? 1.5f : 1);
                    
                    }
                    else if (enemy.strongColliders.Contains(hit.collider))
                    {
                        if (selectedWeapon == 0) mult = 1.25f;
                        else mult = enemy.strongPointMult;
                    
                    }
                    else
                    {
                        if (selectedWeapon == 0) mult = 1.25f;
                        else 
                        {
                            if (allCrits && selectedWeapon != 5 && enemy.weakColliders != null)
                            {
                                mult = 0;
                            }
                            else
                            {
                                mult = 1;
                            }
                        }
                    
                    }
                    mult *= slowDmg? slowDmgMult : 1;
                    mult /= fastDmg ? (slowDmgMult / 2f) : 1;
                    damage = (int)(((dist > fallOffStart * rangeMult) ? Mathf.RoundToInt(dmgPerPellet * (fallOffDistace * rangeMult - dist) / (fallOffDistace * rangeMult)) : dmgPerPellet) * mult * overloadMult);
                    if (!enemy.shielded)
                    {
                        if (damage > 0)
                        {
                            enemy.takeDamage(damage, damageType.None);
                            ParticleSystem partSys = Instantiate(mult > 1.5f ? partMax : partMid, hit.point, Quaternion.LookRotation(hit.normal));
                            partSys.Play();
                            AudioSource aS = Instantiate(damage > 10 ? goodHit : midHit, hit.point, Quaternion.identity);
                            aS.Play();
                            Destroy(aS.gameObject, aS.clip.length);
                            hitImage.color = damage > 5 ? critColor : hitColor;

                            if (knockerBacker) enemy.GetComponent<Rigidbody>().AddForce(addedKnockback * facingRay.direction.normalized, ForceMode.Impulse);

                        }
                        else
                        {
                            ParticleSystem partSys = Instantiate(partMin, hit.point, Quaternion.LookRotation(hit.normal));
                            hitImage.color = missColor;
                            partSys.Play();
                            AudioSource aS = Instantiate(badHit, hit.point, Quaternion.identity);
                            aS.Play();
                            Destroy(aS.gameObject, aS.clip.length);
                        }
                    }
                    else
                    {
                        if (damage > 0)
                        {
                            enemy.ShieldDamage(damage);
                            ParticleSystem partSys = Instantiate(damage > 10 ? partMax : partMid, hit.point, Quaternion.LookRotation(hit.normal));
                            partSys.Play();
                            AudioSource aS = Instantiate(damage > 10 ? goodHit : midHit, hit.point, Quaternion.identity);
                            aS.Play();
                            Destroy(aS.gameObject, aS.clip.length);
                            hitImage.color = damage > 5 ? critColor : hitColor;
                        }
                        else
                        {
                            ParticleSystem partSys = Instantiate(partMin, hit.point, Quaternion.LookRotation(hit.normal));
                            hitImage.color = missColor;
                            partSys.Play();
                            AudioSource aS = Instantiate(badHit, hit.point, Quaternion.identity);
                            aS.Play();
                            Destroy(aS.gameObject, aS.clip.length);

                        }
                    }
                }
            }
            else
            {
                ParticleSystem bhPS = Instantiate(bulletHolePS, hit.point, Quaternion.LookRotation(-hit.normal));
                bhPS.Play();
                AudioSource aS = Instantiate(missHit, hit.point, Quaternion.identity);
                aS.gameObject.transform.position = hit.point;
                aS.Play();
                Destroy(aS.gameObject, aS.clip.length);
            }
        }
    }


    public void GetGrabbed()
    {
        grabbed = true;
        playerMovement.grabbed = true;
    }

    public void Ungrab()
    {
        grabbed = false;
        playerMovement.grabbed = false;
    }

    public void Slow(float slowFactor)
    {
        playerMovement.Slow(slowFactor);
    }

    public void RegularSpeed(float slowFactor)
    {
        playerMovement.RegularSpeed(slowFactor);
    }

    private void shootShotgun()
    {
        Vector3 pelletDir;
        for (int i = 0; i < shotgunPelletCount; i++)
        {
            pelletDir = Quaternion.Euler(Random.Range(-shotgunPelletSpreadMax, shotgunPelletSpreadMax), Random.Range(-shotgunPelletSpreadMax, shotgunPelletSpreadMax), Random.Range(-shotgunPelletSpreadMax, shotgunPelletSpreadMax)) * facingRay.direction;
            shoot(new Ray(cameraTransform.position, pelletDir));
        }
    }

    private void FireRocket()
    {
        canShoot = false;
        RocketCollisonDetection r = Instantiate(rocket, bulletSpawn.position, Quaternion.identity);
        r.transform.up = facingRay.direction;
        r.GetComponent<Rigidbody>().velocity = facingRay.direction.normalized * 20f;
    }
    private IEnumerator FlamethrowerOverheatOver()
    {
        float t = 0f;
        while (t < flamethrowerOverheatLength)
        {
            overheatIMG.fillAmount = t / flamethrowerOverheatLength;
            overheatIMG.color = Color.Lerp(Color.red, Color.white, t / flamethrowerOverheatLength);
            t += Time.deltaTime;
            yield return null;
        }
        overheatIMG.fillAmount = 1;
        overheatIMG.color = Color.white;
        canFlamethrow = true;
        
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

    private void Slam()
    {
        playerMovement.Slam();
        canGetHit = false;
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerMovement.slamCollider == null || meleeCollider == null) return;
        if (playerMovement.slamCollider.enabled)
        {
            EnemyBase eb = other.GetComponentInParent<EnemyBase>();
            Invoke("DamageAgain", 0.2f);
            if (eb != null)
            {
                if (eb.ignoreColliders.Contains(other)) return;
                if (eb.invincible) return;

                if (!eb.shielded)
                {
                    eb.takeDamage(slamDamage, damageType.None);
                    Vector3 d = other.transform.position - transform.position;
                    d.y = 0;
                    d.Normalize();
                    eb.GetComponent<Rigidbody>().AddForce(0.5f * d, ForceMode.Impulse);
                }
                else eb.ShieldDamage(slamDamage);
            }
        }
        else if (meleeCollider.enabled)
        {
            EnemyBase eb = other.GetComponentInParent<EnemyBase>();

            if (eb != null)
            {
                if (eb.ignoreColliders.Contains(other)) return;
                if (eb.invincible) return;

                float mult = 1f;
                mult *= slowDmg ? slowDmgMult : 1;
                mult /= fastDmg ? (slowDmgMult / 2f) : 1;
                if (!eb.shielded)
                {

                    eb.takeDamage((int)(meleeDamage * mult), damageType.None);
                    float mul = knockerBacker ? 2 : 1;
                    eb.GetComponent<Rigidbody>().AddForce(mul * facingRay.direction.normalized, ForceMode.Impulse);
                    if (canParry)
                    {
                        if (eb.canParry)
                        {
                            canGetHit = false;
                            Invoke("DamageAgain", 0.2f);
                            currentHP += (currentHP + parryHealing) > maxHP ? (maxHP - currentHP) : parryHealing;
                            camContoller.Shake(0.25f, 0.2f);
                            StartCoroutine(QuickPause(0.2f));
                        }
                    }
                }
                else eb.ShieldDamage(meleeDamage);
            }
        }
    }

    private IEnumerator QuickPause(float t)
    {
        parryScreen.SetActive(true);
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(t);
        Time.timeScale = 1;
        parryScreen.SetActive(false);
    }

    public void takeDamage(int dmg) //Recibir damage
    {
        if (canGetHit)
        {
            damagedIMG.color = new Color(damagedIMG.color.r, damagedIMG.color.g, damagedIMG.color.b, 1);
            currentHP -= (int)(dmg / overloadMult);
            canGetHit = false;
            AudioSource aS = Instantiate(damagedSound, transform);
            aS.pitch = Random.Range(0.75f, 1.25f);
            aS.Play();
            Destroy(aS.gameObject, aS.clip.length);
            Invoke("resetDamage", 0.1f);
            canHeal = false;
            haltHeal = true;

            StartCoroutine(camContoller.Shake(0.25f, 0.2f));


            if (healCR != null)
            {
                StopCoroutine(healCR);
                healCR = null;
            }
            if (checkHealCR != null)
            {
                StopCoroutine(checkHealCR);
                checkHealCR = null;
            }
            if (currentHP <= 0)
            {
                isEMPd = false;
                dead = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void resetDamage() //Para Invoke
    {
        haltHeal = false;
        canGetHit = true;
        checkHealCR = StartCoroutine(CheckHeal());
    }

    private void DamageAgain()
    {
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
                grappleIMG.gameObject.SetActive(true);
                if (!isAllowedToOverload)
                {
                    uiBGIMG.sprite = UIarray[1];
                }
                break;
            case 3:
                playerMovement.ChangeSprint(true);
                break;
            case 4:
                isAllowedToOverload = true;
                overloadCooldownIMG.gameObject.SetActive(true);
                uiBGIMG.sprite = UIarray[2];
                break;
            case 5:
                isAllowedToHeal = true;
                break;
            case 6:
                differentFlames = true;
                break;
            case 7:
                playerMovement.allowedDoubleJump = true;
                break;
            case 8:
                rangeMult = 2;
                break;
            case 9:
                canSlam = true;
                break;
            case 10:
                canParry = true;
                break;
            case 11:
                allCrits = true;
                break;
            case 12:
                slowDmg = true;
                switch (selectedWeapon)
                {
                    case 0:
                        readyWeaponTime = pistolCooldown / slowDmgSpeedDiv;
                        break;
                    case 1:
                        readyWeaponTime = shotgunCooldown / slowDmgSpeedDiv;
                        break;
                    case 3:
                        readyWeaponTime = 1.5f / slowDmgSpeedDiv;
                        break;
                    case 4:
                        readyWeaponTime = meleeCooldown / slowDmgSpeedDiv;
                        break;
                    case 5:
                        readyWeaponTime = rifleCooldown / slowDmgSpeedDiv;
                        break;
                    default:
                        break;
                }

                break;
            case 13:
                knockerBacker = true;
                break;
            case 14:
                fastDmg = true;
                gunAnimator.speed = 1.5f;
                switch (selectedWeapon)
                {
                    case 0:
                        readyWeaponTime = pistolCooldown * slowDmgSpeedDiv;
                        break;
                    case 1:
                        readyWeaponTime = shotgunCooldown * slowDmgSpeedDiv;
                        break;
                    case 3:
                        readyWeaponTime = 1.5f * slowDmgSpeedDiv;
                        break;
                    case 4:
                        readyWeaponTime = meleeCooldown * slowDmgSpeedDiv;
                        break;
                    case 5:
                        readyWeaponTime = rifleCooldown * slowDmgSpeedDiv;
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    public void unlockWeapon(int weapon)
    {
        switch (weapon)
        {
            case 0:
                hasShotgun = true;
                shotgunUnlockIMG.color = Color.white;
                break;
            case 1:
                hasFlamethrower = true;
                flamethrowerUnlockIMG.color = Color.white;
                overheatIMG.gameObject.SetActive(true);
                break;
            case 2:
                hasRocket = true;
                rocketUnlockIMG.color = Color.white;
                break;
            case 3:
                hasMelee = true;
                break;
            case 4:
                hasRifle = true;
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
                grappleIMG.gameObject.SetActive(false);
                grappleIndicator.gameObject.SetActive(false);
                if (!isAllowedToOverload)
                {
                    uiBGIMG.sprite = UIarray[0];
                }
                break;
            case 3:
                playerMovement.ChangeSprint(false);
                break;
            case 4:
                isAllowedToOverload = false;
                overloadCooldownIMG.gameObject.SetActive(false);
                if (grappleIMG.gameObject.activeSelf)
                {
                    uiBGIMG.sprite = UIarray[1];
                }
                else
                {
                    uiBGIMG.sprite = UIarray[0];
                }
                break;
            case 5:
                isAllowedToHeal = false;
                break;
            case 6:
                differentFlames = false;
                break;
            case 7:
                playerMovement.allowedDoubleJump = false;
                break;
            case 8:
                rangeMult = 1;
                break;
            case 9:
                canSlam = false;
                break;
            case 10:
                canParry = false;
                break;
            case 11:
                allCrits = false;
                break;
            case 12:
                slowDmg = false;
                switch (selectedWeapon)
                {
                    case 0:
                        readyWeaponTime = pistolCooldown;
                        break;
                    case 1:
                        readyWeaponTime = shotgunCooldown;
                        break;
                    case 3:
                        readyWeaponTime = 1.5f;
                        break;
                    case 4:
                        readyWeaponTime = meleeCooldown;
                        break;
                    case 5:
                        readyWeaponTime = rifleCooldown;
                        break;
                    default:
                        break;
                }
                break;
            case 13:
                knockerBacker = false;
                break;
            case 14:
                fastDmg = false;
                gunAnimator.speed = 1;
                switch (selectedWeapon)
                {
                    case 0:
                        readyWeaponTime = pistolCooldown;
                        break;
                    case 1:
                        readyWeaponTime = shotgunCooldown;
                        break;
                    case 3:
                        readyWeaponTime = 1.5f;
                        break;
                    case 4:
                        readyWeaponTime = meleeCooldown;
                        break;
                    case 5:
                        readyWeaponTime = rifleCooldown;
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    private void OOBDie() //Morir si OOB
    {
        dead = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShootGrapple() //Disparar Grapple
    {
        canGrapple = false;
        AudioSource aS = Instantiate(grappleSound, transform.position, Quaternion.identity);
        aS.Play();
        grappleIndicator.gameObject.SetActive(false);
        Destroy(aS.gameObject, aS.clip.length);
        if (Physics.Raycast(facingRay, out RaycastHit hit, grappleDistance, bounds))
        {
            if(hit.collider.gameObject != null)
            {
                StartCoroutine(GrappleReload());
                playerMovement.GrappleTo(hit.point);
            }
        }
        else
        {
            playerMovement.FailGrapple(transform.position + facingRay.direction.normalized * grappleDistance);
            Invoke("FailGrappleWait", 0.5f);
        }
        
    }

    private void FailGrappleWait()
    {
        canGrapple = true;
    }
    
    private IEnumerator CheckHeal() //Revisar si se puede curar
    {
        float timer = 0f;
        while (timer < healingTime)
        {
            damagedIMG.color = Color.Lerp(Color.red, new Color(1, 0, 0, 0), timer/healingTime);
            timer += Time.deltaTime;
            if (haltHeal)
            {
                haltHeal = false;
                yield break;
            }
            yield return null;
        }
        canHeal = true;
        damagedIMG.color = new Color(1, 0, 0, 0);
        if (isAllowedToHeal) healCR = StartCoroutine(Heal());

    }

    private IEnumerator Heal() //Curar
    {
        while (canHeal && currentHP < maxHP)
        {
            currentHP += currentHP > (maxHP-healingRate)? (maxHP-currentHP) : healingRate;
            yield return new WaitForSeconds(0.05f);
        }
        healCR = null;
        
    }

    private IEnumerator GrappleReload() //Recargar grapple
    {
        
        float timer = 0f;
        while (timer < grappleDelay)
        {
            grappleIMG.fillAmount = timer/grappleDelay;
            timer += Time.deltaTime;
            yield return null;
        }
        grappleIMG.fillAmount = 1;
        canGrapple = true;

    }

    private IEnumerator WaitOverload()
    {
        float t = 0f;
        while (t < overloadCooldown)
        {
            overloadCooldownIMG.fillAmount = t/overloadCooldown;
            if (overloadIMG.gameObject.activeSelf)
            {
                if (t >= overloadTime)
                {
                    DeOverload();
                    overloadIMG.gameObject.SetActive(false);
                }
            }
            t += Time.deltaTime;
            yield return null;
        }
        if (!inventory.hasMaterials(0, 2) || !inventory.hasMaterials(1, 2) || !inventory.hasMaterials(2, 2))
        {
            overloadCooldownIMG.color = Color.red;
        }
        overloadCooldownIMG.fillAmount = 1;
        canOverload = true;

    }
    
    public IEnumerator GetEMPd(float maxT)
    {
        empIMG.gameObject.SetActive(true);
        foreach (int i in Inventory.getEnabledUpgrades()) //Habilitar todas las mejoras activadas al iniciar
        {
            disableUpgrade(i);
        }

        isEMPd = true;

        float t = 0;
        while (t < maxT)
        {
            t += Time.deltaTime;
            yield return null;
        }

        isEMPd = false;
        empIMG.gameObject.SetActive(false);
        foreach (int i in Inventory.getEnabledUpgrades()) //Habilitar todas las mejoras activadas al iniciar
        {
            enableUpgrade(i);
        }

    }

    private void Cheat()
    {
        inventory.addToInventory(0,100);
        inventory.addToInventory(1, 100);
        inventory.addToInventory(2, 100);
        transform.position = cheatTransform.position;
    }

}
