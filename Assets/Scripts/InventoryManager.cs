using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    private Inventory inventory;
    public int oldUpgrade;
    private TMPro.TMP_Dropdown upgradeDropdown;
    [SerializeField] private List<string> possibleOptions;
    [SerializeField] private int id;
    private bool isRunning = true;
    [SerializeField] TMPro.TMP_Dropdown[] otherDropdowns;
    TMPro.TMP_Dropdown.OptionData DDTina;
    List<TMPro.TMP_Dropdown.OptionData> newOptions;
    public bool isChangedByPerson = true;
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
            newOptions = new List<TMPro.TMP_Dropdown.OptionData>();

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
        if (isChangedByPerson)
        {


            if (upgradeDropdown.value > 0)
            {
                inventory.enableUpgrade(possibleOptions.IndexOf(upgradeDropdown.options[upgradeDropdown.value].text), id);
            }
            if (oldUpgrade > 0)
            {
                inventory.disableUpgrade(oldUpgrade, id);
            }

            inventory.CheckForDuplicates();

            foreach (TMPro.TMP_Dropdown DD in otherDropdowns)
            {
                InventoryManager DDIM = DD.GetComponent<InventoryManager>();
                newOptions.Clear();
                newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[0])); //Adds empty to list

                DDTina = new TMPro.TMP_Dropdown.OptionData(possibleOptions[DDIM.oldUpgrade]);

                if (inventory.availableUpgrades.Count > 0)
                {
                    for (int i = 0; i < inventory.availableUpgrades.Count; i++)
                    {

                        if ((inventory.availableUpgrades[i] > DDIM.oldUpgrade || (i == inventory.availableUpgrades.Count - 1) && (inventory.availableUpgrades[i] > DDIM.oldUpgrade)) && DDIM.oldUpgrade > 0)
                        {
                            newOptions.Add(DDTina);
                        }
                        newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[inventory.availableUpgrades[i]]));
                        if (i == inventory.availableUpgrades.Count - 1 && inventory.availableUpgrades[i] < DDIM.oldUpgrade)
                        {
                            newOptions.Add(DDTina);
                        }
                    }
                }
                else

                {
                    if (DDIM.oldUpgrade > 0)
                    {
                        newOptions.Add(DDTina);

                    }
                }


                DD.options = newOptions;
                DDIM.isChangedByPerson = false;
                DD.value = DD.options.IndexOf(DDTina);
            }
            oldUpgrade = possibleOptions.IndexOf(upgradeDropdown.captionText.text);
        }
        else
        {
            isChangedByPerson = true;
        }
        
    }
    
    
}
