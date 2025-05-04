using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    //Comments were written in English because when I code, I like to think in English, as it's closer in nature to C# and (at least professionally) it's more common
    //[Traduccion] Los comentarios fueron escritos en Ingles porque, cuando hago codigo, me gusta pensar en Ingles, ya que es fundamentalmente mas parecido a C# y (al menos profesionalmente) es mas comun

    
    private Inventory inventory; //Player's Inventory script
    public int oldUpgrade; //Selected upgrade

    private TMPro.TMP_Dropdown upgradeDropdown; //Reference to own dropdown menu

    [SerializeField] private List<string> possibleOptions; //List of possible options as text mapped to their integer values
    [SerializeField] private int id; //Slot identifier, used for inventory
    [SerializeField] TMPro.TMP_Dropdown[] otherDropdowns; //References to other dropdown menus

    public bool isRunning = true; //Bool so that InventoryManager update only runs once


    public TMPro.TMP_Dropdown.OptionData Tina; //Named this way because of something said in a call I was in, fixed my code, used to look for and maintain selected value when modifying option list
    
    public bool isChangedByPerson = true; //Created to stop stack overflow from occurring when modifying selected value via code
    void Start()
    {
        //Preparations
        inventory = FindObjectOfType<Inventory>();
        upgradeDropdown = GetComponent<TMPro.TMP_Dropdown>();
        oldUpgrade = possibleOptions.IndexOf(upgradeDropdown.captionText.text);


    }
    private void OnEnable()
    {
        isRunning = true;
    }

    private void Update()
    {
        if (Time.timeScale == 0 && isRunning) //If menu is opened, updates options
        {
            isRunning = false;
            List<TMPro.TMP_Dropdown.OptionData> newOptions = new List<TMPro.TMP_Dropdown.OptionData>(); //Create empty list of new options

            newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[0])); //Add empty


            oldUpgrade = possibleOptions.IndexOf(upgradeDropdown.captionText.text); //Store value of selected upgrade

            Tina = upgradeDropdown.options[upgradeDropdown.value]; //Store selected upgrade


            if (inventory.availableUpgrades.Count > 0) //If there are available upgrades do the following:
            {
                for (int i = 0; i < inventory.availableUpgrades.Count; i++) //Cycle through every available upgrade
                {

                    if (inventory.availableUpgrades[i] > oldUpgrade && oldUpgrade > 0) //If selected upgrade isn't empty and available upgrade is greater than selected upgrade, add selected upgrade
                    {
                        newOptions.Add(Tina);
                    }

                    newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[inventory.availableUpgrades[i]])); //Add available upgrade
                    
                    
                    if (i == inventory.availableUpgrades.Count - 1 && inventory.availableUpgrades[i] < oldUpgrade) //If every single available upgrade has been checked and the selected upgrade has not yet been added, add selected upgrade
                    {
                        newOptions.Add(Tina);
                    }
                }
            }
            else

            {
                if (oldUpgrade > 0) //If there are no available upgrades and the selected upgrade is not empty, add selected upgrade
                {
                    newOptions.Add(Tina); 
                    
                }
            }
            upgradeDropdown.options = newOptions; //Update options

            if (upgradeDropdown.value != upgradeDropdown.options.IndexOf(Tina)) //If the selected option's index changed, set index to the new one and stop stack overflow error from occurring
            {
                isChangedByPerson = false; //Indicate that the next change will be made by code to prevent stack overflow
                upgradeDropdown.value = upgradeDropdown.options.IndexOf(Tina);
            }
        }
    }


    public TMPro.TMP_Dropdown.OptionData returnSelected() //Return selected value
    {
        return upgradeDropdown.options[upgradeDropdown.value];
    }


    public void selectUpgrade() //Happens every time option is changed
    {
        if (isChangedByPerson) //Check if change is made by a human
        {


            if (upgradeDropdown.value > 0) //If an upgrade is selected, enable it
            {
                inventory.enableUpgrade(possibleOptions.IndexOf(upgradeDropdown.options[upgradeDropdown.value].text), id);
            }
            if (oldUpgrade > 0) //If an upgrade is deselected, disable it
            {
                inventory.disableUpgrade(oldUpgrade, id);
            }


            //Store new selected values
            oldUpgrade = possibleOptions.IndexOf(upgradeDropdown.captionText.text);
            Tina = upgradeDropdown.options[upgradeDropdown.value]; 

            inventory.CheckForDuplicates(); //Failsafe

            foreach (TMPro.TMP_Dropdown DD in otherDropdowns) //Cycle through all other dropdowns
            {
                List<TMPro.TMP_Dropdown.OptionData> newOptions = new List<TMPro.TMP_Dropdown.OptionData>(); //Create empty list of new options
                InventoryManager DDIM = DD.GetComponent<InventoryManager>();
                newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[0])); //Add empty to list

                

                if (inventory.availableUpgrades.Count > 0) //If there are available upgrades, do the following
                {
                    for (int i = 0; i < inventory.availableUpgrades.Count; i++) //Cycle through every available upgrade
                    {

                        if (inventory.availableUpgrades[i] > DDIM.oldUpgrade && DDIM.oldUpgrade > 0) //If selected upgrade isn't empty and available upgrade is greater than selected upgrade, add selected upgrade
                        {
                            newOptions.Add(DDIM.Tina); 
                        }
                        newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[inventory.availableUpgrades[i]])); //Add available upgrade

                        if (i == inventory.availableUpgrades.Count - 1 && inventory.availableUpgrades[i] < DDIM.oldUpgrade) //If every single available upgrade has been checked and the selected upgrade has not yet been added, add selected upgrade
                        {
                            newOptions.Add(DDIM.Tina);
                        }
                    }
                }
                else

                {
                    if (DDIM.oldUpgrade > 0) //If there are no available upgrades and the selected upgrade is not empty, add selected upgrade
                    {
                        newOptions.Add(DDIM.Tina);
                        
                    }
                }


                DD.options = newOptions; //Update options
                if (DD.value != DD.options.IndexOf(DDIM.Tina)) //If the selected option's index changed, set index to the new one and stop stack overflow error from occurring
                {
                    DDIM.isChangedByPerson = false; //Indicate that the next change will be made by code to prevent stack overflow
                    DD.value = DD.options.IndexOf(DDIM.Tina);
                }
                
            }
        }
        else //Indicate that the next change will be made by a human
        {
            isChangedByPerson = true;
        }
        
    }
    
    
}
