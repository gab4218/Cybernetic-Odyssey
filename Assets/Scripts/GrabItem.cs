using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class GrabItem : MonoBehaviour
{
    public int materialType;
    public int materialQuantity;
    private Inventory inventory;
    [SerializeField] private ParticleSystem pickupParticles;
    [SerializeField] private Gradient colorGradient;
    private void Start()
    {
        inventory = FindObjectOfType<Inventory>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerActions>())
        {
            inventory.addToInventory(materialType, materialQuantity);
            ParticleSystem ps = Instantiate(pickupParticles, transform.position, Quaternion.identity);
            ParticleSystem.ColorOverLifetimeModule pc = ps.colorOverLifetime;
            
            pc.color = new ParticleSystem.MinMaxGradient(colorGradient);
            ps.Play();
            Destroy(gameObject);
        }
    }
}
