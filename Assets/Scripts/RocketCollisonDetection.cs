using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketCollisonDetection : MonoBehaviour
{
    [SerializeField] Collider _explosion;
    [SerializeField] ParticleSystem _explosionPS;
    [SerializeField] AudioSource _explosionAS;
    private void OnCollisionEnter(Collision collision)
    {
        _explosion.enabled = true;
        Instantiate(_explosionPS, transform.position, Quaternion.identity).Play();
        AudioSource AS = Instantiate(_explosionAS, transform.position, transform.rotation);
        AS.Play();
        Destroy(AS.gameObject, AS.clip.length);
        Destroy(gameObject, 0.2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_explosion.enabled)
        {
            EnemyBase eb = other.GetComponentInParent<EnemyBase>();

            if(eb != null)
            {
                if (eb.invincible) return;
                if (!eb.shielded)
                {
                    eb.takeDamage(60, PlayerActions.damageType.Fire);
                }
                else eb.ShieldDamage(150);
            }


            PlayerActions pa = other.GetComponentInParent<PlayerActions>();
            if(pa != null)
            {
                pa.takeDamage(25);
                return;
            }

        }
    }
}
