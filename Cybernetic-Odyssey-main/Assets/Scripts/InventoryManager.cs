using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    private Inventory inventory;
    private int oldUpgrade;
    private TMPro.TMP_Dropdown upgradeDropdown;
    [SerializeField] private List<string> possibleOptions;
    [SerializeField] private int id;
    private bool isRunning = true;
    [SerializeField] TMPro.TMP_Dropdown[] otherDropdowns;

    
    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        upgradeDropdown = GetComponent<TMPro.TMP_Dropdown>();
        oldUpgrade = upgradeDropdown.value;
    }


    private void Update()
    {
        if (Time.timeScale == 0 && isRunning)
        {
            isRunning = false;
            List<TMPro.TMP_Dropdown.OptionData> newOptions = new List<TMPro.TMP_Dropdown.OptionData>();

            newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[0]));
            for (int i = 0; i < inventory.availableUpgrades.Count; i++)
            {
                if (inventory.availableUpgrades.Count <= 0) break;
                if (inventory.availableUpgrades[i] > inventory.availableUpgrades[oldUpgrade] && oldUpgrade > 0)
                {
                    newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[inventory.availableUpgrades[oldUpgrade]]));
                }
                newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[inventory.availableUpgrades[i]]));
            }
            upgradeDropdown.options = newOptions;
        }
        else if(Time.timeScale != 0 && !isRunning)
        {
            isRunning = true;
        }
    }


    public TMPro.TMP_Dropdown.OptionData returnSelected()
    {
        return upgradeDropdown.options[upgradeDropdown.value];
    }


    public void selectUpgrade()
    {
        if (upgradeDropdown.value > 0)
        {
            inventory.enableUpgrade(possibleOptions.IndexOf(upgradeDropdown.options[upgradeDropdown.value].text), id);
        }
        if (oldUpgrade > 0)
        {
            inventory.disableUpgrade(oldUpgrade, id);
        }
        oldUpgrade = upgradeDropdown.value;
        foreach (TMPro.TMP_Dropdown DD in otherDropdowns)
        {
            
            List<TMPro.TMP_Dropdown.OptionData> newOptions = new List<TMPro.TMP_Dropdown.OptionData>();
            newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[0]));

            

            if (inventory.availableUpgrades.Count > 0)
            {
                for (int i = 0; i < inventory.availableUpgrades.Count; i++)
                {
                    if (inventory.availableUpgrades[i] > inventory.availableUpgrades[oldUpgrade] && oldUpgrade > 0)
                    {
                        newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[inventory.availableUpgrades[oldUpgrade]]));
                    }
                    newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[inventory.availableUpgrades[i]]));
                }
            }


            DD.options = newOptions;
        }
    }
    
}
