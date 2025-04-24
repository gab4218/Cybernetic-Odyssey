using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrabItem : MonoBehaviour, IInteractable
{
    public int materialType;
    private Inventory inventory;
    private void Start()
    {
        inventory = FindObjectOfType<Inventory>();
    }
    public void onInteract()
    {
        inventory.addToInventory(materialType);
        Destroy(gameObject);
    }
}
