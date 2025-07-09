using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCost : MonoBehaviour
{
    //Script usado para guardar info de mejoras para craftearlas
    
    public int[] cost;
    public int upgradeType;
    public bool hasBeenCrafted = false;
    public int majorUpgradeType;
    private void Update()
    {
        if (Time.timeScale == 0)
        {
            if (Inventory.availableUpgrades.Contains(upgradeType))
            {
                hasBeenCrafted = true;
            }
            if (upgradeType == 0)
            {
                if (Inventory.hasFlamethrower && majorUpgradeType == 1) hasBeenCrafted = true;
                else if (Inventory.hasShotgun && majorUpgradeType == 0) hasBeenCrafted = true;
                else if (Inventory.hasRocket && majorUpgradeType == 2) hasBeenCrafted = true;
            }
        }
    }
}
