using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : EnemyBase
{


    //attacks
    private bool _canShoot; // value == 0
    private bool _canFlamethrow; // value == 1
    private bool _canFirePunch; // value == 2
    private bool _canDropKick; // value == 3
    private bool _canPunchBarrage; // value == 4
    private bool _canShield; // value == 5
    private bool _canEMP; // value == 6
    private bool _canGrenade; // value == 7

    //yuh
    private int _selectedAttack;


    [Header("Final Boss things")]

    [Header("General")]
    [SerializeField] private int _attackQuantity = 8;
    [SerializeField] private Transform _aimTransform;
    [SerializeField] private Transform _bulletSpawnPoint;


    //Cooldowns
    [Header("Cooldowns")]

    [SerializeField] private float _gunCooldown = 10f;
    [SerializeField] private float _flamethrowerCooldown = 15f;
    [SerializeField] private float _firePunchCooldown = 10f;
    [SerializeField] private float _dropKickCooldown = 20f;
    [SerializeField] private float _barrageCooldown = 25f;
    [SerializeField] private float _shieldCooldown = 30f;
    [SerializeField] private float _empCooldown = 30f;
    [SerializeField] private float _attackCooldown = 5f;
    private bool _canAttack = true;




    //gun
    [Header("Gun")]
    [SerializeField] private TrailRenderer _bulletTR;
    [SerializeField] private int _gunDamagePerHit = 3;
    [SerializeField] private float _gunStartupDelay = 1f;
    [SerializeField] private float _gunDamageInterval = 0.1f;
    [SerializeField] private float _gunDuration = 3f;
    [SerializeField] private float _gunStunTime = 1.5f;
    private Ray _gunRay;
    private Coroutine _gunCR;

    //barrage
    [Header("Barrage")]
    [SerializeField] private Collider _barrageCollider;
    [SerializeField] private int _barrageAttackQuantity = 5;
    [SerializeField] private int _barrageHitDamage = 20;
    [SerializeField] private float _barrageDistance = 3f;
    [SerializeField] private float _barrageSpeed = 20f;
    [SerializeField] private float _barrageAttackInterval = 1f;
    [SerializeField] private float _barrageAttackDuration = 0.1f;
    [SerializeField] private float _barrageStunTime = 3f;
    private int _barrageCurrentAttack = 0;
    private Coroutine _barrageCR;
    private bool _inBarrageRange = false;


    //flamethrower
    [Header("Flamethrower")]
    [SerializeField] private Collider _flameCollider;
    [SerializeField] private ParticleSystem _flamePS;
    [SerializeField] private Transform[] _fireCheckpoints;
    [SerializeField] private int _fireDamage = 30;
    [SerializeField] private float _flameSpeed = 7f;
    [SerializeField] private float _flameDuration = 4f;
    [SerializeField] private float _checkpointDetectionDistance = 0.5f;
    [SerializeField] private float _flameStunTime = 2f;
    private Coroutine _flamethrowerCR;

    //punch
    [Header("Punch")]
    [SerializeField] private Collider _punchCollider;
    [SerializeField] private ParticleSystem _punchPS;
    [SerializeField] private LineRenderer _punchLR;
    [SerializeField] private GameObject _punchGameObject;
    [SerializeField] private Transform _punchStartTransform;
    [SerializeField] private int _punchDamage = 40;
    [SerializeField] private float _punchWindupTime = 1.5f;
    [SerializeField] private float _punchShootSpeed = 20f;
    [SerializeField] private float _punchRetractSpeed = 40f;
    [SerializeField] private float _punchStunTime = 2f;
    private Coroutine _punchCR;

    //dropkick
    [Header("Dropkick")]
    [SerializeField] private Collider _dropkickCollider;
    [SerializeField] private ParticleSystem _dropkickPS;
    [SerializeField] private int _dropkickDamage = 75;
    [SerializeField] private float _dropkickSpeed = 30f;
    [SerializeField] private float _dropkickWindupTime = 2f;
    [SerializeField] private float _dropkickStunTime = 3f;
    private Coroutine _dropkickCR;

    //shield
    [Header("Shield")]
    [SerializeField] private MeshRenderer _shieldMR;
    [SerializeField] private int _healingAmount = 100;
    [SerializeField] private int _shieldHP = 300;
    [SerializeField] private float _shieldTime = 5f;
    private float _currentShieldTime;
    private Coroutine _shieldCR;


    //EMP
    [Header("EMP")]
    [SerializeField] private ParticleSystem _empP;
    [SerializeField] private float _empRange = 20f;
    [SerializeField] private float _empWindupTime = 3f;
    [SerializeField] private float _empDuration = 15f;
    private Ray _empRay;
    private Coroutine _empCR;


    //boom
    [Header("Explosive")]
    [SerializeField] Rigidbody _grenadeGameObject;
    [SerializeField] ParticleSystem _grenadePS;
    [SerializeField] int _grenadeDamage = 60;
    [SerializeField] float _grenadeLaunchForce = 15f;
    private Coroutine _grenadeCR;



    //Gun (done?)
    private void AimGun()
    {
        _aimTransform.position = Vector3.Lerp(_aimTransform.position, playerTranform.position, 1 - Mathf.Pow(0.8f, Time.deltaTime));
    }
    private IEnumerator ShootGun()
    {
        float t = 0;
        while (t < _gunStartupDelay)
        {
            t += Time.deltaTime;
            yield return null;
            AimGun();
        }

        t = 0;
        while (t < _gunDuration)
        {
            AimGun();
            float dt = 0;
            while (dt < _gunDamageInterval)
            {
                StartCoroutine(Shoot());
                dt += Time.deltaTime;
                t += Time.deltaTime;
                yield return null;
            }
        }

        Stun(_gunStunTime);
        Invoke("AllowShoot", _gunCooldown);
        _canShoot = false;
        Invoke("AllowAttack", _attackCooldown);
        _gunCR = null;
    }
    private IEnumerator Shoot()
    {
        Vector3 aimDir = _aimTransform.position - transform.position;
        _gunRay = new Ray(_bulletSpawnPoint.position, aimDir);
        RaycastHit hit;
        if (Physics.Raycast(_gunRay, out hit))
        {
            TrailRenderer tr = Instantiate(_bulletTR, _bulletSpawnPoint.position, Quaternion.identity);
            Vector3 startPos = tr.transform.position;
            float t = 0;
            while (t < 1)
            {
                tr.transform.position = Vector3.Lerp(startPos, hit.point, t);
                t += Time.deltaTime * 5f / tr.time;
                yield return null;
            }
            tr.transform.position = hit.point;
            Destroy(tr.gameObject, tr.time);
            if (Physics.Raycast(_gunRay, out RaycastHit hit1))
            {
                if (hit1.collider.TryGetComponent(out PlayerActions pa))
                {
                    pa.takeDamage(_gunDamagePerHit);
                }
            }
        }
        
    }
    private void AllowShoot()
    {
        _canShoot = true;
    }



    //Barrage
    private void MoveBarrage()
    {
        if (Vector3.Distance(transform.position, playerTranform.position) > _barrageDistance)
        {
            dir = playerTranform.position - transform.position;
            dir.Normalize();
            rb.velocity = dir * _barrageSpeed;
        }
        else if (!_inBarrageRange)
        {
            _inBarrageRange = true;
            _barrageCR = StartCoroutine(StartBarrage());
        }
    }

    private IEnumerator StartBarrage()
    {
        for (int i = 0; i < _barrageAttackQuantity; i++)
        {
            float t = 0;
            while (t < _barrageAttackInterval)
            {
                t += Time.deltaTime;
                yield return null;
            }
            StartCoroutine(BarrageHit());
        }
        Stun(_barrageStunTime);
        _canPunchBarrage = false;
        Invoke("AllowBarrage", _barrageCooldown);
    }

    private IEnumerator BarrageHit()
    {
        float t = 0;
        _barrageCollider.enabled = true;
        while (t < _barrageAttackDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        _barrageCollider.enabled = false;
    }

    private void AllowBarrage()
    { 
        _canPunchBarrage = true;
    }
    




    private void AllowAttack()
    {
        _canAttack = true;
    }

    private void SelectAttack()
    {
        if (!_canAttack) return;
        _selectedAttack = Random.Range(0, _attackQuantity);
        _canAttack = false;
        switch (_selectedAttack)
        {
            case 0:
                if (_canShoot) _gunCR = StartCoroutine(ShootGun());
                break;

            case 1:
                break;

            case 2:
                break;

            case 3:
                break;

            case 4:
                break;

            case 5:
                break;

            case 6:
                break;

            case 7:
                break;

            default:
                break;
        }
    }






}
