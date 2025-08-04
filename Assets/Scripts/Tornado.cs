using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tornado : MonoBehaviour
{

    [SerializeField] private int _damage = 5;
    [SerializeField] private float _forceStrength = 10f;
    [SerializeField] private float _lifespan = 7f;
    [SerializeField] private AudioSource _source;
    private float t = 0;



    private void Awake()
    {
        if (ProgressManager.beatSalamander)
        {
            _damage = (int)(1.2f * _damage);
            _lifespan *= 1.2f;
        }
        AudioSource aS = Instantiate(_source, transform);
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);
    }

    void Update()
    {
        t += Time.deltaTime;

        if (t >= _lifespan)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerActions pa = other.GetComponentInParent<PlayerActions>();

        if (pa != null)
        {
            Rigidbody r = pa.GetComponent<Rigidbody>();
            Vector3 dir = pa.transform.position - transform.position;
            dir.y = 0;
            dir.Normalize();
            dir = Vector3.Cross(dir, Vector3.up) + dir * 0.25f;
            r.AddForce(dir * _forceStrength * 50f);
            pa.takeDamage(_damage);
        }
    }
}
