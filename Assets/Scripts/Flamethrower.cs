using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flamethrower : MonoBehaviour
{
    [SerializeField] int flamethrowerDamage = 1;
    
    PlayerActions playerActions;
    int t = 0;
    bool onEnemy;
    EnemyBase eb;
    private void Start()
    {
        playerActions = FindObjectOfType<PlayerActions>();
    }
    private void OnTriggerEnter(Collider other)
    {
        eb = other.gameObject.GetComponentInParent<EnemyBase>();
        if (eb != null)
        {
            onEnemy = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (onEnemy)
        {
            t += 1;
            if (t % 10 == 0)
            {
                if (eb != null)
                {
                    if (eb.shielded) eb.ShieldDamage(flamethrowerDamage);
                    else eb.takeDamage(flamethrowerDamage, playerActions.dmgType);
                } 
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponentInParent<EnemyBase>())
        {
            t = 0;
            onEnemy = false;
        }
    }
}
