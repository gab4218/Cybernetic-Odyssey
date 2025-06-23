using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CraftingStation : MonoBehaviour, IInteractable
{
    private Inventory inventory;
    [SerializeField] TMP_Text[] matDisplay;
    private bool isCrafting = false;
    [SerializeField] private GameObject craftingMenu;
    [SerializeField] private Button[] craftingButtons;
    [SerializeField] private GameObject[] pages;
    private ItemCost[] costs;

    private IEnumerator watABit()
    {
        yield return new WaitForEndOfFrame();
        isCrafting = true;
    }

    public void onInteract() //Al interactuar, 
    {
        if (Inventory.hasShotgun) costs[6].hasBeenCrafted = true;
        if (Inventory.hasFlamethrower) costs[7].hasBeenCrafted = true;
        Time.timeScale = 0;
        craftingMenu.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        for (int ii = 0; ii < matDisplay.Length; ii++)
        {
            matDisplay[ii].text = Inventory.materialInventory[ii].ToString();

        }

        for (int i = 0; i < craftingButtons.Length; i++)
        {
            bool[] hasColors = new bool[] { false, false, false };
            for (int j = 0; j < costs[i].cost.Length; j++)
            {
                if (inventory.hasMaterials(j, costs[i].cost[j]))
                {
                    hasColors[j] = true;
                }
                else
                {
                    hasColors[j] = false;
                }
            }
            if ((Inventory.secondarySlots.Contains(costs[i].upgradeType) || Inventory.availableUpgrades.Contains(costs[i].upgradeType)) && i < 6)
            {
                costs[i].hasBeenCrafted = true;
            }
            if (hasColors[0] == true && hasColors[1] == true && hasColors[2] == true && !costs[i].hasBeenCrafted)
            {
                craftingButtons[i].interactable = true;
            }
            else
            {
                craftingButtons[i].interactable = false;
            }
        }
        StartCoroutine(watABit());
    }

    private void Start()
    {
        costs = new ItemCost[craftingButtons.Length];
        inventory = FindObjectOfType<Inventory>();
        craftingMenu.SetActive(false);
        
        for (int i = 0; i < craftingButtons.Length; i++)
        {
            costs[i] = craftingButtons[i].GetComponent<ItemCost>();
            craftingButtons[i].interactable = false;  
        }
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)) && isCrafting)
        {
            closeMenu();
        }
        if (isCrafting)
        {

            for (int ii = 0; ii < matDisplay.Length; ii++)
            {
                matDisplay[ii].text = Inventory.materialInventory[ii].ToString();
            }
            for (int i = 0; i < craftingButtons.Length; i++)
            {
                bool[] hasColors = new bool[] { false, false, false };
                for (int j = 0; j < costs[i].cost.Length; j++)
                {
                    if (inventory.hasMaterials(j, costs[i].cost[j]))
                    {
                        hasColors[j] = true;
                    }
                    else
                    {
                        hasColors[j] = false;
                    }
                }
                if (hasColors[0] == true && hasColors[1] == true && hasColors[2] == true && !costs[i].hasBeenCrafted)
                {
                    craftingButtons[i].interactable = true;
                }
                else
                {
                    craftingButtons[i].interactable = false;
                }
            }
        }
    }



    public void goToPage(int p)
    {
        foreach (GameObject pa in pages)
        {
            pa.SetActive(false);
        }
        pages[p].SetActive(true);

    }

    public void closeMenu()
    {
        Time.timeScale = 1;
        isCrafting = false;
        craftingMenu.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }



}
