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
    private Inventory inventory;
    private void Start()
    {
        inventory = FindObjectOfType<Inventory>();
    }

    private void Update()
    {
        if (inventory.availableUpgrades.Contains(upgradeType) && Time.timeScale == 0)
        {
            hasBeenCrafted = true;
        }
    }
}
