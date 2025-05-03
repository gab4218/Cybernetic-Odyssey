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

    private void Awake()
    {
        secondarySlots = new int[numberOfUpgradeSlots];
        playerActions = FindObjectOfType<PlayerActions>();
    }
    private void Update()
    {
        displayMats();
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
        removeFromInventory(upgrade.cost[1], upgrade.cost[0]);
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

}
