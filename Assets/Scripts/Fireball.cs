using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Fireball : MonoBehaviour
{

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Collider _explosionCollider;
    [SerializeField] private Collider _regularCollider;
    [SerializeField] private ParticleSystem _explodeParticles;
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private int _damage = 25;
    [SerializeField] private float _knockback = 6f;
    [SerializeField] private AudioSource _source;

    private void Start()
    {
        if (ProgressManager.beatSalamander)
        {
            _damage = (int)(1.2f * _damage);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        _explosionCollider.enabled = true;
        _regularCollider.enabled = false;
        Instantiate(_explodeParticles, transform.position, Quaternion.identity);
        AudioSource aS = Instantiate(_source, transform.position, Quaternion.identity);
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);
        Destroy(gameObject, 1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions pa = other.GetComponentInParent<PlayerActions>();
        if(pa != null)
        {
            pa.takeDamage(_damage);
            Rigidbody r = pa.GetComponent<Rigidbody>();
            r.velocity = Vector3.zero;
            Vector3 dir = pa.transform.position - transform.position;
            dir.y = 0;
            dir.Normalize();
            dir += Vector3.up;
            r.AddForce(dir * _knockback);
            _explosionCollider.enabled = false;
        }
    }


}
