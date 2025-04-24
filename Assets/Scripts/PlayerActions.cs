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
    [Header("Inputs")]
    [SerializeField] KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] KeyCode inventoryKey = KeyCode.I;
    [Header("Parameters")]
    [SerializeField] float interactDistance;
    [SerializeField] int maxHP = 100;
    [SerializeField] int dmgPerPellet = 1;
    [SerializeField] float readyWeaponTime = 0.5f;
    [SerializeField] int dmgTypeCount = 4;
    
    bool canGetHit = true;
    int damageType = 0;
    public int currentHP;
    private float knockbackMult = 1f;
    bool canShoot = true;
    private Ray facingRay;
    private Inventory inventory;
    private void Start()
    {
        inventoryPlaceholder.SetActive(false);
        currentHP = maxHP;
        inventory = GetComponent<Inventory>();
        foreach (int i in inventory.getEnabledUpgrades())
        {
            enableUpgrade(i);
        }
    }


    private void Update()
    {
        facingRay = new Ray(cameraTransform.position, cameraTransform.forward);
        HPDisplay.text = $"{currentHP}/{maxHP}";
        if (Input.mouseScrollDelta.y > 0)
        {
            damageType = (damageType + 1) % dmgTypeCount;
        }
        else if(Input.mouseScrollDelta.y < 0)
        {
            damageType = (damageType > 0? damageType - 1 : dmgTypeCount-1)% dmgTypeCount;
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


            default:
                break;
        }
    }

}
