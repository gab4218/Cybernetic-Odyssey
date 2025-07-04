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

    public void onInteract()
    {
        Time.timeScale = 0;
        isCrafting = true;
        craftingMenu.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        for (int i = 0; i < craftingButtons.Length; i++)
        {
            if (inventory.hasMaterials(costs[i].cost[1], costs[i].cost[0]))
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
                if (inventory.hasMaterials(costs[i].cost[1], costs[i].cost[0]))
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
