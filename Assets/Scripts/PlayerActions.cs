using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerActions : MonoBehaviour
{

    //Variables de UI y feedback visual
    [SerializeField] Transform cameraTransform;
    [SerializeField] GameObject inventoryPlaceholder;
    [SerializeField] Image HPDisplay;
    [SerializeField] Image crosshair;
    [SerializeField] Image grappleIMG;
    [SerializeField] Image overloadIMG;
    [SerializeField] Image overheatIMG;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] TrailRenderer bulletPrefab;
    [SerializeField] Gradient[] bulletColors;
    [SerializeField] MeshFilter gunMeshFilter;
    [SerializeField] Mesh pistolMesh, shotgunMesh;
    [SerializeField] Image pistolUnlockIMG, shotgunUnlockIMG, flamethrowerUnlockIMG;
    [SerializeField] Color[] overloadingColors;
    [SerializeField] ParticleSystem flamethrowerFirePS, shotPS, bulletHolePS;
    [SerializeField] Animator gunAnimator;

    [Header("Inputs")] //Teclas de input
    [SerializeField] KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] KeyCode inventoryKey = KeyCode.Tab;
    [SerializeField] KeyCode Key1 = KeyCode.Alpha1, Key2 = KeyCode.Alpha2, Key3 = KeyCode.Alpha3;
    [SerializeField] KeyCode grappleKey = KeyCode.F;
    [SerializeField] KeyCode iceKey = KeyCode.Z, fireKey = KeyCode.X, acidKey = KeyCode.C;
    [SerializeField] KeyCode healKey = KeyCode.Q;

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
    [SerializeField] float weakPointMult = 2;
    [SerializeField] float strongPointMult = 0.5f;
    [SerializeField] float overloadTime = 10f;
    [SerializeField] float overloadCooldown = 20f;
    //Otras variables
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
    private bool canGrapple = false;
    private bool canHeal = false;
    private Animator anim;
    private bool hasShotgun = false;
    private bool hasFlamethrower = false;
    private bool isAllowedToOverload = false;
    private bool isAllowedToHeal = false;
    private bool canOverload = true;
    private List<bool> canHealMats = new List<bool>{ false, false, false };
    private bool haltHeal = false;
    private bool canFlamethrow = true;
    public ParticleSystem partMax;
    public ParticleSystem partMin;
    public ParticleSystem partMid;
    public bool isCrouched = false;
    public bool canGambleCrouch = false;
    private float flamethrowerCurrentTime;
    private ParticleSystem.EmissionModule flamethrowerFire;
    
    public enum damageType
    {
        None,
        Ice,
        Fire,
        Acid
    }

    damageType dmgType = damageType.None;
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
        if (inventory.hasShotgun)
        {
            hasShotgun = true;
        }
        if (inventory.hasFlamethrower)
        {
            hasFlamethrower = true;
        }
        flamethrowerCollider.enabled = false;
        overloadIMG.gameObject.SetActive(false);
        grappleIMG.gameObject.SetActive(false);
        flamethrowerFire = flamethrowerFirePS.emission;
        flamethrowerFire.enabled = false;
        //Preparaciones


    }


    private void Update()
    {

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

        isCrouched = playerMovement.isCrouching;

        facingRay = new Ray(cameraTransform.position, cameraTransform.forward); //Crear rayo en direccion a donde mira el jugador

        HPDisplay.fillAmount = currentHP * 1f / maxHP; //Mostrar HP

        if (Input.GetKeyDown(Key1) && !Input.GetKey(KeyCode.Mouse0)) //Armas
        {
            selectedWeapon = 0;
            fallOffDistace = pistolFallOffMax;
            fallOffStart = pistolFallOffStart;
            readyWeaponTime = pistolCooldown;
            gunMeshFilter.mesh = pistolMesh;
            
        }
        if(Input.GetKeyDown(Key2) && hasShotgun && !Input.GetKey(KeyCode.Mouse0))
        {
            selectedWeapon = 1;
            fallOffDistace = shotgunFallOffMax;
            fallOffStart = shotgunFallOffStart;
            readyWeaponTime = shotgunCooldown;
            gunMeshFilter.mesh = shotgunMesh;
            
        }
        if (Input.GetKeyDown(Key3) && hasFlamethrower && !Input.GetKey(KeyCode.Mouse0))
        {
            selectedWeapon = 2;
            gunMeshFilter.mesh = shotgunMesh;
        }

        if (Input.GetKeyDown(healKey) && currentHP < maxHP && canHeal)
        {
            for (int i = 0; i < 3; i++)
            {
                canHealMats[i] = inventory.hasMaterials(i, 1);
            }
            if (canHealMats.Contains(true))
            {
                inventory.removeFromInventory(canHealMats.IndexOf(true), 1);
                currentHP = Mathf.Min(currentHP + 50, maxHP);
            }
        }

        if (isAllowedToOverload && canOverload)
        {
            if (Input.GetKeyDown(iceKey) && inventory.hasMaterials(0, 2))
            {
                dmgType = damageType.Ice;
                inventory.removeFromInventory(0, 2);
                canOverload = false;
                overloadIMG.gameObject.SetActive(true);
                overloadIMG.color = overloadingColors[0];
                StartCoroutine(WaitOverload());
            }
            if (Input.GetKeyDown(fireKey) && inventory.hasMaterials(1, 2))
            {
                dmgType = damageType.Fire;
                inventory.removeFromInventory(1, 2);
                canOverload = false;
                overloadIMG.gameObject.SetActive(true);
                overloadIMG.color = overloadingColors[1];
                StartCoroutine(WaitOverload());
            }
            if (Input.GetKeyDown(acidKey) && inventory.hasMaterials(2, 2))
            {
                dmgType = damageType.Acid;
                inventory.removeFromInventory(2, 2);
                canOverload = false;
                overloadIMG.gameObject.SetActive(true);
                overloadIMG.color = overloadingColors[2];
                StartCoroutine(WaitOverload());
            }

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

        if (Input.GetKeyDown(shootKey) && canShoot) //Disparar
        {
            switch (selectedWeapon)
            {
                case 0:
                    shoot(facingRay);
                    ParticleSystem ps = Instantiate(shotPS, bulletSpawn);
                    ps.Play();
                    gunAnimator.SetTrigger("shot");
                    break;
                case 1:
                    shootShotgun();
                    ParticleSystem ps1 = Instantiate(shotPS, bulletSpawn);
                    ps1.Play();
                    gunAnimator.SetTrigger("shot");
                    break;
                case 2:
                    if (canFlamethrow)
                    {
                        flamethrowerCollider.enabled = true;
                        flamethrowerFire.enabled = true;
                        gunAnimator.SetBool("flamethrower", true);
                    }

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
            }
        }

        if (flamethrowerCollider.enabled && canFlamethrow)
        {
            if (flamethrowerCurrentTime < flamethrowerOverheatTime)
            {
                flamethrowerCurrentTime += Time.deltaTime;
            }
            else
            {
                flamethrowerCurrentTime = 0;
                canFlamethrow = false;
                Invoke("FlamethrowerOverheatOver", flamethrowerOverheatLength);
            }
        }
        else if(flamethrowerOverheatTime > 0)
        {
            flamethrowerOverheatTime -= Time.deltaTime;
        }
        else
        {
            flamethrowerCurrentTime = 0;
        }

        if (overheatIMG.gameObject.activeSelf && canFlamethrow)
        {
            overheatIMG.fillAmount = (flamethrowerOverheatTime - flamethrowerCurrentTime) / flamethrowerOverheatTime;
            overheatIMG.color = Color.Lerp(Color.white, Color.red, flamethrowerCurrentTime / flamethrowerOverheatTime);
        }


        if (Physics.Raycast(facingRay, out RaycastHit hit, interactDistance))
        {

            if (Input.GetKeyDown(interactKey)) //Interactuar
            {

                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
                {
                    interactable.onInteract();
                    anim.SetTrigger("Interact");
                }
            }
        }


    }

    private void shoot(Ray aimRay)
    {
        if (Physics.Raycast(aimRay, out RaycastHit hit)) //Si dispara a algun lugar valido, hacer feedback visual y calcular multiplicador por distancia
        {
            float dist = Vector3.Distance(transform.position, hit.point);
            
            
            TrailRenderer trail = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
            trail.colorGradient = bulletColors[(int)dmgType];
            StartCoroutine(SpawnTrail(trail, hit));
            canShoot = false;
            Invoke("readyWeapon", readyWeaponTime);
            EnemyBase enemy = hit.collider.gameObject.GetComponentInParent<EnemyBase>();


            
            if (enemy != null && !hit.collider.isTrigger && dist < fallOffDistace) //Si dispara a un enemigo, hacer damage
            {
                if (!enemy.ignoreColliders.Contains(hit.collider))
                {
                    float mult = 1;
                    if (enemy.weakColliders.Contains(hit.collider))
                    {
                        mult = weakPointMult;
                    }
                    else if (enemy.strongColliders.Contains(hit.collider))
                    {
                        mult = strongPointMult;
                    }
                    
                    enemy.takeDamage((int)((dist > fallOffStart? Mathf.RoundToInt(dmgPerPellet * (fallOffDistace - dist) / fallOffDistace) : dmgPerPellet) * mult), dmgType);
                    
                    if (mult == 1)
                    {
                        ParticleSystem partSys = Instantiate (partMid, hit.point, Quaternion.LookRotation(hit.normal));
                        partSys.Play();
                    }
                    else if (mult == weakPointMult)
                    {
                        ParticleSystem partSys = Instantiate(partMax, hit.point, Quaternion.LookRotation(hit.normal));
                        partSys.Play();
                    }
                    else if (mult == strongPointMult)
                    {
                        ParticleSystem partSys = Instantiate(partMin, hit.point, Quaternion.LookRotation(hit.normal));
                        partSys.Play();
                    }
                }
            }
            else
            {
                ParticleSystem bhPS = Instantiate(bulletHolePS, hit.point, Quaternion.LookRotation(-hit.normal));
                bhPS.Play();
            }
        }
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

    private IEnumerator FlamethrowerOverheatOver()
    {
        float t = 0f;
        while (t < flamethrowerOverheatLength)
        {
            t += Time.deltaTime;
            yield return null;
        }
        
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


    public void takeDamage(int dmg) //Recibir damage
    {
        if (canGetHit)
        {
            currentHP -= dmg;
            canGetHit = false;
            Invoke("resetDamage", 0.5f);
            canHeal = false;
            haltHeal = true;
            StopCoroutine(Heal());
            if (currentHP <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void resetDamage() //Para Invoke
    {
        haltHeal = false;
        canGetHit = true;
        StartCoroutine(CheckHeal());
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
            case 4:
                isAllowedToOverload = true;
                break;
            case 5:
                isAllowedToHeal = true;
                break;
            case 6:
                canGambleCrouch = true;
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
            case 4:
                isAllowedToOverload = false;
                break;
            case 5:
                isAllowedToHeal = false;
                break;
            case 6:
                canGambleCrouch = false;
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
        else
        {
            playerMovement.FailGrapple(transform.position + facingRay.direction.normalized * grappleDistance);
        }
    }

    private IEnumerator CheckHeal() //Revisar si se puede curar
    {
        float timer = 0f;
        while (timer < healingTime)
        {
            timer += Time.deltaTime;
            if (haltHeal)
            {
                haltHeal = false;
                yield break;
            }
            yield return null;
        }
        canHeal = true;
        if (isAllowedToHeal) StartCoroutine(Heal());

    }

    private IEnumerator Heal() //Curar
    {
        while (canHeal && currentHP < maxHP)
        {
            currentHP += currentHP > (maxHP-healingRate)? (maxHP-currentHP) : healingRate;
            yield return new WaitForSeconds(0.1f);
        }
        
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

    }

    private IEnumerator WaitOverload()
    {
        float t = 0f;
        while (t < overloadCooldown)
        {

            if (t >= overloadTime && dmgType != damageType.None)
            {
                dmgType = damageType.None;
                overloadIMG.gameObject.SetActive(false);
            }
            t += Time.deltaTime;
            yield return null;
        }

        canOverload = true;
        
    }
    

}
