using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CraftingStation : MonoBehaviour, IInteractable
{
    private Inventory inventory;
    private bool isCrafting = false;
    [SerializeField] private GameObject craftingMenu;
    [SerializeField] private Button[] craftingButtons;
    private ItemCost[] costs;

    public void onInteract() //Al interactuar, 
    {
        Time.timeScale = 0;
        isCrafting = true;
        craftingMenu.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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
            if (hasColors[0] == true && hasColors[1] == true && hasColors[2] == true)
            {
                craftingButtons[i].interactable = true;
            }
            else
            {
                craftingButtons[i].interactable = false;
            }
        }
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
        if (Input.GetKeyDown(KeyCode.Escape) && isCrafting)
        {
            closeMenu();
        }
        if (isCrafting)
        {
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
                if (hasColors[0] == true && hasColors[1] == true && hasColors[2] == true)
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



    public void closeMenu()
    {
        Time.timeScale = 1;
        isCrafting = false;
        craftingMenu.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }



}
