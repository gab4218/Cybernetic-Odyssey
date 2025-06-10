using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerActions : MonoBehaviour
{

    public static bool dead = false;

    [Header("UI")] //Variables de UI y feedback visual
    [SerializeField] Transform cameraTransform;
    [SerializeField] GameObject inventoryPlaceholder;
    [SerializeField] Image HPDisplay;
    [SerializeField] Image crosshair;
    [SerializeField] Image grappleIMG;
    [SerializeField] Image overheatIMG;
    [SerializeField] Image overloadCooldownIMG;
    [SerializeField] Image overloadIMG;
    [SerializeField] Image hitImage;
    [SerializeField] Color hitColor, missColor, critColor;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] TrailRenderer bulletPrefab;
    [SerializeField] Gradient[] bulletColors;
    [SerializeField] MeshFilter gunMeshFilter;
    [SerializeField] Mesh pistolMesh, shotgunMesh, flamethrowerMesh;
    [SerializeField] Image pistolUnlockIMG, shotgunUnlockIMG, flamethrowerUnlockIMG;
    [SerializeField] Sprite[] selectedOverloads;
    [SerializeField] Color[] overloadingColors;
    [SerializeField] ParticleSystem flamethrowerFirePS, shotPS, bulletHolePS;
    [SerializeField] Animator gunAnimator;
    [SerializeField] CameraController camContoller;
    [SerializeField] Image damagedIMG;

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
    Coroutine overheatCR, healCR, checkHealCR;
    public enum damageType
    {
        None,
        Ice,
        Fire,
        Acid
    }

    damageType dmgType = damageType.None;

    int selectedOverload = 0;
    private void Start()
    {
        dead = false;
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
        overheatIMG.gameObject.SetActive(false);
        overloadCooldownIMG.gameObject.SetActive(false);
        grappleIMG.gameObject.SetActive(false);
        flamethrowerFire = flamethrowerFirePS.emission;
        flamethrowerFire.enabled = false;
        damagedIMG.color = new Color (damagedIMG.color.r, damagedIMG.color.g, damagedIMG.color.b, 0);
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
        if (isAllowedToOverload && canOverload && selectedWeapon != 2)
        {

            if (Input.mouseScrollDelta.y > 0)
            {
                selectedOverload = (selectedOverload + 1) % 3;
                overloadCooldownIMG.sprite = selectedOverloads[selectedOverload];
                overloadCooldownIMG.color = overloadingColors[selectedOverload];
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                selectedOverload = selectedOverload == 0? 2 : (selectedOverload - 1);
                overloadCooldownIMG.sprite = selectedOverloads[selectedOverload];
                overloadCooldownIMG.color = overloadingColors[selectedOverload];
            }
        }
        else if (selectedWeapon == 2)
        {
            overloadCooldownIMG.color = new Color(0.45f,0.5f,0.6f, 0f);
        }
        
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
            overloadCooldownIMG.color = overloadingColors[selectedOverload];
        }
        if(Input.GetKeyDown(Key2) && hasShotgun && !Input.GetKey(KeyCode.Mouse0))
        {
            selectedWeapon = 1;
            fallOffDistace = shotgunFallOffMax;
            fallOffStart = shotgunFallOffStart;
            readyWeaponTime = shotgunCooldown;
            gunMeshFilter.mesh = shotgunMesh;
            overloadCooldownIMG.color = overloadingColors[selectedOverload];
        }
        if (Input.GetKeyDown(Key3) && hasFlamethrower && !Input.GetKey(KeyCode.Mouse0))
        {
            selectedWeapon = 2;
            gunMeshFilter.mesh = flamethrowerMesh;
            dmgType = damageType.None;
            overloadIMG.gameObject.SetActive(false);
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
            }
        }

        if (Input.GetKeyDown(cheatKey))
        {
            Cheat();
        }

        if (isAllowedToOverload && canOverload && Input.GetKeyDown(KeyCode.R) && selectedWeapon != 2)
        {
            switch (selectedOverload)
            {
                case 0:
                    if (inventory.hasMaterials(0, 2))
                    {
                        dmgType = damageType.Ice;
                        inventory.removeFromInventory(0, 2);
                        canOverload = false;
                        overloadIMG.gameObject.SetActive(true);
                        overloadIMG.color = overloadingColors[0];
                        StartCoroutine(WaitOverload());
                    }
                    break;
                case 1:
                    if (inventory.hasMaterials(1, 2))
                    {
                        dmgType = damageType.Fire;
                        inventory.removeFromInventory(1, 2);
                        canOverload = false;
                        overloadIMG.gameObject.SetActive(true);
                        overloadIMG.color = overloadingColors[1];
                        StartCoroutine(WaitOverload());
                    }
                    break;
                case 2:
                    if (inventory.hasMaterials(2, 2))
                    {
                        dmgType = damageType.Acid;
                        inventory.removeFromInventory(2, 2);
                        canOverload = false;
                        overloadIMG.gameObject.SetActive(true);
                        overloadIMG.color = overloadingColors[2];
                        StartCoroutine(WaitOverload());
                    }
                    break;
                default:
                    break;
            }
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
        hitImage.color = Color.Lerp(hitImage.color, new Color(hitImage.color.r, hitImage.color.g, hitImage.color.b, 0), 1 - Mathf.Pow(0.05f,Time.deltaTime));
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
                if (overheatCR != null)
                {
                    StopCoroutine(overheatCR);
                }
                overheatCR = StartCoroutine(FlamethrowerOverheatOver());
            }
        }
        else if(flamethrowerCurrentTime > 0)
        {
            flamethrowerCurrentTime -= Time.deltaTime;
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
                    int damage = 0;
                    float mult = 1;
                    if (enemy.weakColliders.Contains(hit.collider))
                    {
                        mult = enemy.weakPointMult;
                    
                    }
                    else if (enemy.strongColliders.Contains(hit.collider))
                    {
                        mult = enemy.strongPointMult;
                    
                    }
                    else
                    {
                        mult = 1;
                    
                    }
                    damage = (int)((dist > fallOffStart ? Mathf.RoundToInt(dmgPerPellet * (fallOffDistace - dist) / fallOffDistace) : dmgPerPellet) * mult);
                    if (damage > 0)
                    {
                        enemy.takeDamage(damage, dmgType);
                        ParticleSystem partSys = Instantiate(damage > 5? partMax : partMid, hit.point, Quaternion.LookRotation(hit.normal));
                        partSys.Play();
                        hitImage.color = damage > 5? critColor : hitColor;
                    }
                    else
                    {
                        ParticleSystem partSys = Instantiate(partMin, hit.point, Quaternion.LookRotation(hit.normal));
                        hitImage.color = missColor;
                        partSys.Play();
                        if (mult == 0)
                        {
                            enemy.WeakenArmor(dmgType);
                        }

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
            overheatIMG.fillAmount = t / flamethrowerOverheatLength;
            overheatIMG.color = Color.Lerp(Color.white, Color.red, t / flamethrowerOverheatLength);
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


    public void takeDamage(int dmg) //Recibir damage
    {
        if (canGetHit)
        {
            damagedIMG.color = new Color(damagedIMG.color.r, damagedIMG.color.g, damagedIMG.color.b, 1);
            currentHP -= dmg;
            canGetHit = false;
            Invoke("resetDamage", 0.25f);
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
                break;
            case 3:
                playerMovement.ChangeWalljump(true);
                break;
            case 4:
                isAllowedToOverload = true;
                overloadCooldownIMG.gameObject.SetActive(true);
                overloadCooldownIMG.sprite = selectedOverloads[selectedOverload];
                overloadCooldownIMG.color = overloadingColors[selectedOverload];
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
                overheatIMG.gameObject.SetActive(true);
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
                break;
            case 3:
                playerMovement.ChangeWalljump(false);
                break;
            case 4:
                isAllowedToOverload = false;
                overloadCooldownIMG.gameObject.SetActive(false);
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
        dead = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShootGrapple() //Disparar Grapple
    {
        canGrapple = false;
        StartCoroutine(GrappleReload());
        if (Physics.Raycast(facingRay, out RaycastHit hit, grappleDistance, bounds))
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
            yield return new WaitForSeconds(0.1f);
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
            if(overloadIMG.gameObject.activeSelf)overloadCooldownIMG.fillAmount = t/overloadCooldown;
            if (t >= overloadTime && dmgType != damageType.None)
            {
                dmgType = damageType.None;
                overloadIMG.gameObject.SetActive(false);
            }
            t += Time.deltaTime;
            yield return null;
        }
        overloadIMG.fillAmount = 1;
        canOverload = true;
        
        
    }
    
    private void Cheat()
    {
        inventory.addToInventory(0,100);
        inventory.addToInventory(1, 100);
        inventory.addToInventory(2, 100);
        transform.position = cheatTransform.position;
    }

}
