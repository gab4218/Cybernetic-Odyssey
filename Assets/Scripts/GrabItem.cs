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
    public void onInteract() //Agregar al inventario el material correcto
    {
        inventory.addToInventory(materialType);
        Destroy(gameObject);
    }
}
