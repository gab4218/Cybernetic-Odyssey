using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrabItem : MonoBehaviour
{
    public int materialType;
    public int materialQuantity;
    private Inventory inventory;
    private void Start()
    {
        inventory = FindObjectOfType<Inventory>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerActions>())
        {
            inventory.addToInventory(materialType, materialQuantity);
            Destroy(gameObject);
        }
    }
}
