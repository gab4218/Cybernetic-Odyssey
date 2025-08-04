using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpiderling : EnemyBase
{
    [Header("General Stuff")]
    [SerializeField] private Animator _anim;
    [SerializeField] private SkinnedMeshRenderer _mr;

    [Header("Attack")]
    [SerializeField] private Collider _explosionCollider;
    [SerializeField] private Collider _normalCollider;
    [SerializeField] private ParticleSystem _explosion;
    [SerializeField] private int _explosionDamage = 15;
    [SerializeField] private float _lungeDistance = 5f;
    [SerializeField] private float _lungeSpeed = 10f;
    [SerializeField] private AudioSource _source;
    private bool _lunging = false;
    private bool _inactive = true;


    private void Lunge()
    {
        Vector3 d = player.transform.position - transform.position;
        d.y = 0;
        d.Normalize();
        rb.velocity = d * _lungeSpeed + Vector3.up * 2f;
        _lunging = true;
        _anim.SetTrigger("lunge");
    }

    private void Explode()
    {
        _explosionCollider.enabled = true;
        _normalCollider.enabled = false;
        Instantiate(_explosion, transform.position, Quaternion.identity);
        AudioSource aS = Instantiate(_source, transform.position, Quaternion.identity);
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);
        Invoke("EndExplosion", 0.25f);
        _mr.enabled = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(_inactive) _inactive = false;

        if (_lunging)
        {
            Explode();
        }
    }

    private void EndExplosion()
    {
        _explosionCollider.enabled = false;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Acid a))
        {
            if(a.permanent) Destroy(gameObject);
        }
        if (_explosionCollider.enabled)
        {
            if (other.GetComponentInParent<PlayerActions>())
            {
                player.takeDamage(_explosionDamage);
                _explosionCollider.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (Pause.paused) return;
        if (CameraController.inCutscene) return;
        if (_inactive) return;
        if (_lunging) return;

        findDirection();
        Vector3 d = player.transform.position - transform.position;
        d.y = 0;
        d.Normalize();
        d *= speed;
        d.y = rb.velocity.y;

        rb.velocity = d;

        if (Vector3.Distance(transform.position, player.transform.position) < _lungeDistance && !_lunging)
        {
            Lunge();
        }
    }

    protected override void Start()
    {
        base.Start();
        if (ProgressManager.beatSpider)
        {
            maxHP = (int)(maxHP * 1.5f);
            currentHP = maxHP;
            _explosionDamage = (int)(1.2f * _explosionDamage);
        }
    }


}
