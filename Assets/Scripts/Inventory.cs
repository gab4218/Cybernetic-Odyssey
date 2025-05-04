using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] int numberOfUpgradeSlots = 4;
    [SerializeField] TMP_Text[] matDisplay;

    ItemCost[] craftingCosts;
    Button[] craftingButtons;
    PlayerActions playerActions;
    int[] secondarySlots;
    int[] materialInventory = new int[6];
    public List<int> availableUpgrades = new List<int>();
    int oldUpgrade;
    bool cancheck = true;
    

    private void Awake()
    {
        secondarySlots = new int[numberOfUpgradeSlots];
        playerActions = FindObjectOfType<PlayerActions>();
    }
    private void Update()
    {
        displayMats();
        CheckForDuplicates();
    }
    public void removeFromInventory(int type, int quantity)
    {
        materialInventory[type] -= quantity;
    }

    public void addToInventory(int type)
    {
        materialInventory[type]++;
    }

    public void unlockUpgrade(ItemCost upgrade)
    {
        for (int i = 0; i < upgrade.cost.Length; i++)
        {
            removeFromInventory(i, upgrade.cost[i]);
        }
        upgrade.hasBeenCrafted = true;
        availableUpgrades.Add(upgrade.upgradeType);
    }

    public void enableUpgrade(int upgrade, int slot)
    {
        secondarySlots[slot] = upgrade;
        playerActions.enableUpgrade(upgrade);
        availableUpgrades.Remove(upgrade);
    }

    public void disableUpgrade(int upgrade, int slot)
    {
        secondarySlots[slot] = 0;
        playerActions.disableUpgrade(upgrade);
        availableUpgrades.Add(upgrade);
    }

    public int[] getEnabledUpgrades()
    {
        return secondarySlots;
    }

    public bool hasMaterials(int type, int quantity)
    {
        return materialInventory[type] >= quantity; 
    }

    void displayMats()
    {
        for (int i = 0; i < matDisplay.Length; i++)
        {
            matDisplay[i].text = materialInventory[i].ToString();
        }
    }

    public void CheckForDuplicates()
    {
        oldUpgrade = new int();
        
        foreach (int i in availableUpgrades)
        {
            if (i == oldUpgrade)
            {
                availableUpgrades.Remove(i);
                cancheck = false;
                break;
            }
            else
            {
                oldUpgrade = i;
            }
        }
        if (!cancheck)
        {
            cancheck = true;
            CheckForDuplicates();

        }
    }

}
