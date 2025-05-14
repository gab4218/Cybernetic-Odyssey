using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flamethrower : MonoBehaviour
{
    [SerializeField] int flamethrowerDamage = 1;
    

    float t = 0;
    bool onEnemy;
    EnemyBase eb;
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
            if (t % 3 < Time.fixedDeltaTime)
            {
                eb.takeDamage(flamethrowerDamage, PlayerActions.damageType.Fire);
            }
            t += Time.fixedDeltaTime;
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
