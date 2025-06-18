using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{

    //Variables modificables o publicas
    [SerializeField] int numberOfUpgradeSlots = 4;
    public TMP_Text[] matDisplay;
    public static List<int> availableUpgrades = new List<int>();


    //Variables de crafteo e inventario
    PlayerActions playerActions;
    static int[] secondarySlots;
    public static int[] materialInventory = new int[3];
    public static bool hasShotgun = false;
    public static bool hasFlamethrower = false;
    
    
    

    private void Awake()
    {
        //Preparaciones
        secondarySlots = new int[numberOfUpgradeSlots];
        playerActions = FindObjectOfType<PlayerActions>();
    }
    private void Update()
    {
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

    public void unlockUpgrade(ItemCost upgrade) //Desbloquear mejora y remover costo
    {
        for (int i = 0; i < upgrade.cost.Length; i++)
        {
            removeFromInventory(i, upgrade.cost[i]);
        }
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

    public int[] getEnabledUpgrades() //Devuelve mejoras habilitadas
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
