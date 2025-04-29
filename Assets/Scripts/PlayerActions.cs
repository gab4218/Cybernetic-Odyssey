using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerActions : MonoBehaviour
{
    
    [SerializeField] Transform cameraTransform;
    [SerializeField] GameObject inventoryPlaceholder;
    [SerializeField] TMP_Text interactionDisplay;
    [SerializeField] TMP_Text HPDisplay;
    [SerializeField] TMP_Text dmgTypeDisplay;
    [SerializeField] Image crosshair;
    [SerializeField] Image grappleIMG;
    [Header("Inputs")]
    [SerializeField] KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] KeyCode inventoryKey = KeyCode.I;
    [SerializeField] KeyCode Key1 = KeyCode.Alpha1, Key2 = KeyCode.Alpha2, Key3 = KeyCode.Alpha3;
    [SerializeField] KeyCode grappleKey = KeyCode.F;
    [Header("Parameters")]
    [SerializeField] float interactDistance;
    [SerializeField] int maxHP = 100;
    [SerializeField] int dmgPerPellet = 1;
    [SerializeField] float readyWeaponTime = 0.5f;
    [SerializeField] float grappleDistance = 15f;
    [SerializeField] float grappleDelay = 5.0f;

    
    bool canGetHit = true;
    int damageType = 0;
    public int currentHP;
    private float knockbackMult = 1f;
    bool canShoot = true;
    private Ray facingRay;
    private Inventory inventory;
    private PlayerMovement playerMovement;
    private bool canGrapple = true;


    private void Start()
    {
        inventoryPlaceholder.SetActive(false);
        currentHP = maxHP;
        inventory = GetComponent<Inventory>();
        playerMovement = GetComponent<PlayerMovement>();
        foreach (int i in inventory.getEnabledUpgrades())
        {
            enableUpgrade(i);
        }
    }


    private void Update()
    {

        facingRay = new Ray(cameraTransform.position, cameraTransform.forward);
        HPDisplay.text = $"{currentHP}/{maxHP}";
        if (Input.GetKeyDown(Key1))
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
        //DELETE LATER
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(grappleKey) && canGrapple)
        {
            ShootGrapple();
        }

        if (transform.position.y < -20)
        {
            OOBDie();
        }

        dmgTypeDisplay.text = $"Damage type: {damageType.ToString()}";

        if (Input.GetKeyDown(shootKey) && canShoot && Time.timeScale > 0)
        {
            crosshair.color = Color.red;

            if(Physics.Raycast(facingRay, out RaycastHit hit))
            {
                
                canShoot = false;
                Invoke("readyWeapon", readyWeaponTime);
                if(hit.collider != null)
                {
                    interactionDisplay.text = $"Shot missed at x = {hit.point.x}, y = {hit.point.y}, z = {hit.point.z}";
                    EnemyBase enemy = hit.collider.gameObject.GetComponentInParent<EnemyBase>();
                    if (enemy != null)
                    {
                        interactionDisplay.text = $"Shot hit an enemy at x = {hit.point.x}, y = {hit.point.y}, z = {hit.point.z}";
                        enemy.takeDamage(dmgPerPellet, damageType);
                    }
                }
            }
        }


        if (Input.GetKeyDown(interactKey) && Time.timeScale > 0)
        {

            if (Physics.Raycast(facingRay, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
                {
                    interactable.onInteract();
                }
            }
        }

        if (Input.GetKeyDown(inventoryKey))
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
        if (Input.GetKeyDown(KeyCode.Escape) && inventoryPlaceholder.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inventoryPlaceholder.SetActive(false);
            Time.timeScale = 1.0f;
        }

    }

    private void readyWeapon()
    {
        canShoot = true;
        crosshair.color = Color.white;
    }


    public void takeDamage(int dmg)
    {
        if (canGetHit)
        {
            currentHP -= dmg;
            canGetHit = false;
            Invoke("resetDamage", 0.5f);
            if (currentHP <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void resetDamage()
    {
        canGetHit = true;
    }

    public float getKnockbackMult()
    {
        return knockbackMult;
    }

    public void enableUpgrade(int upgrade)
    {
        switch (upgrade)
        {
            case 1:
                knockbackMult = 0.5f;
                break;
            case 2:
                playerMovement.ChangeWalljump(true);
                break;
            case 3:
                canGrapple = true;
                grappleIMG.color = new Color(70, 50, 231);
                break;

            default:
                break;
        }
    }

    public void disableUpgrade(int upgrade)
    {
        switch (upgrade)
        {
            case 1:
                knockbackMult = 1;
                break;
            case 2:
                playerMovement.ChangeWalljump(false);
                break;
            case 3:
                canGrapple = false;
                grappleIMG.color = Color.red;
                break;
            default:
                break;
        }
    }

    private void OOBDie()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShootGrapple()
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

    private IEnumerator GrappleReload()
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
