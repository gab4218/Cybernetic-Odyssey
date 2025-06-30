using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{

    
    public TMP_Text[] matDisplay;
    public TMP_Text[] craftingMatDisplay;
    public static List<int> availableUpgrades = new List<int>();
    //[SerializeField] List<int> aUpg;
    [SerializeField] private AudioClip craftSound;
    [SerializeField] private AudioSource sfxSource;

    //Variables de crafteo e inventario
    PlayerActions playerActions;
    public static int[] secondarySlots = new int[4];
    public static int[] materialInventory = new int[3];
    public static bool hasShotgun = false;
    public static bool hasFlamethrower = false;
    //[SerializeField] int[] sSlots = new int[4], matInv = new int[3];
    
    

    private void Awake()
    {
        //Preparaciones
        playerActions = FindObjectOfType<PlayerActions>();
    }
    private void Update()
    {
        //aUpg = availableUpgrades;
        //sSlots = secondarySlots;
        //matInv = materialInventory;
        displayMats(); //Mostrar materiales y verificar integridad
        CheckForDuplicates();
    }
    public void removeFromInventory(int type, int quantity) //Borrar material del inventario
    {
        materialInventory[type] -= quantity;
    }

    public void addToInventory(int type, int quantity) //Agregar material al inventario
    {
        materialInventory[type] += quantity;
    }

    private IEnumerator PrettyColors(TMP_Text text)
    {
        float t = 0;
        text.color = Color.red;
        while (t < 0.5f)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        text.color = Color.white;
    }

    public void UpgradeColors(Image prettyImage)
    {
        prettyImage.gameObject.SetActive(true);
        prettyImage.color = Color.white;
        StartCoroutine(BackNormal(prettyImage));
    }

    public void unlockUpgrade(ItemCost upgrade) //Desbloquear mejora y remover costo
    {
        for (int i = 0; i < upgrade.cost.Length; i++)
        {
            removeFromInventory(i, upgrade.cost[i]);
            if (upgrade.cost[i] > 0)
            {
                StartCoroutine(PrettyColors(craftingMatDisplay[i]));
            }
        }
        sfxSource.clip = craftSound;
        sfxSource.Play();
        
        upgrade.hasBeenCrafted = true;
        if (upgrade.upgradeType != 0)
        {
            availableUpgrades.Add(upgrade.upgradeType);
        }
        else
        {
            switch (upgrade.majorUpgradeType)
            {
                case 0:
                    hasShotgun = true;
                    break;
                case 1:
                    hasFlamethrower = true;
                    break;
                default:
                    break;
            }
            playerActions.unlockWeapon(upgrade.majorUpgradeType);
        }
        
    }

    private IEnumerator BackNormal(Image img)
    {
        float t = 0;
        while (t < 0.5f)
        {
            img.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), t/0.5f);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        img.gameObject.SetActive(false);

    }
    public void enableUpgrade(int upgrade, int slot) //Habilitar mejora
    {
        secondarySlots[slot] = upgrade;
        playerActions.enableUpgrade(upgrade);
        availableUpgrades.Remove(upgrade);
    }

    public void disableUpgrade(int upgrade, int slot) //Deshabilitar mejora
    {
        secondarySlots[slot] = 0;
        playerActions.disableUpgrade(upgrade);
        availableUpgrades.Add(upgrade);
    }

    public static int[] getEnabledUpgrades() //Devuelve mejoras habilitadas
    {
        return secondarySlots;
    }

    public bool hasMaterials(int type, int quantity) //Verificar que tenga los materiales requeridos para craftear
    {
        return materialInventory[type] >= quantity; 
    }

    void displayMats() //Mostrar materiales
    {
        for (int i = 0; i < matDisplay.Length; i++)
        {
            matDisplay[i].text = materialInventory[i].ToString();
        }
    }

    public void CheckForDuplicates() //Chequear que no haya duplicas para evitar problemas
    {

        if (availableUpgrades.Count == 0) return;
        int[] upgradeTypeCount = new int[availableUpgrades.Max()];
        foreach (int i in availableUpgrades)
        {
            upgradeTypeCount[i-1]++;
        }

        for (int i = 0; i < upgradeTypeCount.Length;i++)
        {
            if (upgradeTypeCount[i] > 1)
            {
                availableUpgrades.Remove(i+1);
            }
        }

    }

}
