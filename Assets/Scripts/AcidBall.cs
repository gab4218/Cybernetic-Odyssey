using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcidBall : MonoBehaviour
{
    [SerializeField] private Acid _acid;
    [SerializeField] private Collider _regularCollider;
    [SerializeField] private ParticleSystem _explodeParticles;
    [SerializeField] private int _damage = 25;
    [SerializeField] private float _knockback = 4f;

    private void Start()
    {
        if (ProgressManager.beatSpider)
        {
            _damage = (int)(_damage * 1.2f);
            _knockback = (int)(_knockback * 1.2f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(_explodeParticles, transform.position, Quaternion.identity);
        PlayerActions pa = collision.gameObject.GetComponentInParent<PlayerActions>();
        if (pa != null)
        {
            pa.takeDamage(_damage);
            Vector3 d = pa.transform.position - transform.position;
            d.y = 0;
            d.Normalize();
            d += Vector3.up;
            pa.GetComponent<Rigidbody>().AddForce(d * _knockback);
        }
        Instantiate(_acid, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
