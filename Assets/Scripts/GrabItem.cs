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
    [SerializeField] private float pickupAreaSize = 5;
    private void Start()
    {
        inventory = FindObjectOfType<Inventory>();
    }

    private void Update()
    {
        if (Vector3.Distance(inventory.transform.position, transform.position) < pickupAreaSize)
        {
            transform.position = Vector3.Lerp(transform.position, inventory.transform.position, 1 - Mathf.Pow(0.2f, Time.deltaTime * 2));
        }
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
