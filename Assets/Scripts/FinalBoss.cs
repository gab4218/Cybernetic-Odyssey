using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalBoss : EnemyBase
{


    //attacks
    private bool _canShoot = true; // value == 0
    private bool _canFlamethrow = true; // value == 1
    private bool _canFirePunch = true; // value == 2
    private bool _canDropKick = true; // value == 3
    private bool _canPunchBarrage = true; // value == 4
    private bool _canShield = true; // value == 5
    private bool _canEMP = true; // value == 6
   
    //yuh
    private int _selectedAttack;
    private Collider[] _myColliders;
    
    [Header("Final Boss things")]

    [Header("General")]
    [SerializeField] private int _attackQuantity = 7;
    [SerializeField] private Transform _aimTransform;
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private Transform _middleTransform;
    [SerializeField] private Animator _anim;


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
    [SerializeField] private ParticleSystem _shotPS;
    [SerializeField] private ParticleSystem _gunChargePS;
    [SerializeField] private LayerMask _mask;
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
    [SerializeField] private ParticleSystem _barragePS;
    [SerializeField] private int _barrageAttackQuantity = 5;
    [SerializeField] private int _barrageHitDamage = 20;
    [SerializeField] private float _barrageDistance = 3f;
    [SerializeField] private float _barrageSpeed = 20f;
    [SerializeField] private float _barrageAttackInterval = 1f;
    [SerializeField] private float _barrageAttackDuration = 0.1f;
    [SerializeField] private float _barrageStunTime = 3f;
    [SerializeField] private float _barrageKnockback = 2;
    private Coroutine _barrageCR;
    private bool _inBarrageRange = false;


    //flamethrower
    [Header("Flamethrower")]
    [SerializeField] private Collider _flameCollider;
    [SerializeField] private ParticleSystem _flamePS;
    [SerializeField] private Transform[] _fireCheckpoints;
    [SerializeField] private int _fireDamage = 5;
    [SerializeField] private float _flameWindup = 1;
    [SerializeField] private float _flameSpeed = 7f;
    [SerializeField] private float _flameDuration = 5f;
    [SerializeField] private float _checkpointDetectionDistance = 0.5f;
    [SerializeField] private float _flamethrowerHeight = 10f;
    [SerializeField] private float _flameStunTime = 2f;
    [SerializeField] private float _flamethrowerKnockback = 3;
    private Coroutine _flamethrowerCR;
    private Transform _selectedFlameCheckpoint;


    //punch
    [Header("Punch")]
    [SerializeField] private SphereCollider _punchCollider;
    [SerializeField] private ParticleSystem _punchPS;
    [SerializeField] private ParticleSystem _punchChargePS;
    [SerializeField] private LineRenderer _punchLR;
    [SerializeField] private Rigidbody _punchRB;
    [SerializeField] private Transform _punchStartTransform;
    [SerializeField] private int _punchDamage = 40;
    [SerializeField] private float _punchWindupTime = 1.5f;
    [SerializeField] private float _punchUpRayLength = 20f;
    [SerializeField] private float _punchHeight = 4f;
    [SerializeField] private float _punchShakeTime = 0.5f;
    [SerializeField] private float _punchShootSpeed = 5f;
    [SerializeField] private float _punchRetractSpeed = 15f;
    [SerializeField] private float _punchStunTime = 2f;
    [SerializeField] private float _punchGrabDelay = 0.5f;
    [SerializeField] private float _punchKnockback = 3;
    private Coroutine _punchCR;
    public bool punchWorking = true;


    //dropkick
    [Header("Dropkick")]
    [SerializeField] private Collider _dropkickDmgCollider;
    [SerializeField] private ParticleSystem _dropkickPS;
    [SerializeField] private int _dropkickDamage = 75;
    [SerializeField] private float _dropkickSpeed = 30f;
    [SerializeField] private float _dropkickWindupTime = 2f;
    [SerializeField] private float _dropkickStunTime = 3f;
    [SerializeField] private float _dropkickKnockback = 4;
    private Coroutine _dropkickCR;


    //shield
    [Header("Shield")]
    [SerializeField] private MeshRenderer _shieldMR;
    [SerializeField] private Collider _shieldCollider;
    [SerializeField] private ParticleSystem _shieldBreakPS;
    [SerializeField] private ParticleSystem _shieldHealPS;
    [SerializeField] private int _healingAmount = 100;
    [SerializeField] private int _shieldHP = 300;
    [SerializeField] private float _shieldTime = 5f;
    [SerializeField] private float _shieldStunTime = 2f;
    private int _currentShieldHP;
    private Coroutine _shieldCR;


    //EMP
    [Header("EMP")]
    [SerializeField] private ParticleSystem _empPS;
    [SerializeField] private ParticleSystem _empChargePS;
    [SerializeField] private float _empRange = 30f;
    [SerializeField] private float _empWindupTime = 3f;
    [SerializeField] private float _empDuration = 15f;
    [SerializeField] private float _empStunTime = 1.25f;
    private Ray _empRay;
    private Coroutine _empCR;

    protected override void Start()
    {
        base.Start();
        _anim = GetComponentInChildren<Animator>();
        _flameCollider.enabled = false;
        _shieldCollider.enabled = false;
        _punchCollider.enabled = false;
        _dropkickDmgCollider.enabled = false;
        _barrageCollider.enabled = false;
        _shieldMR.enabled = false;
        _myColliders = GetComponents<Collider>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        state = IDLE;
    }

    private bool _dropkickWorking = false;

    //Gun (done?) [0]
    #region Gun
    private void AimGun()
    {
        _aimTransform.position = Vector3.Lerp(_aimTransform.position, Camera.main.transform.position, 1 - Mathf.Pow(0.1f, Time.deltaTime));
        dir = _aimTransform.position - transform.position;
        _punchRB.transform.position = _punchStartTransform.position;
        _punchRB.transform.rotation = _punchStartTransform.rotation;
        dir.Normalize();
        transform.forward = Vector3.Lerp(transform.forward, dir, 1 - Mathf.Pow(0.1f, Time.deltaTime));
    }
    private IEnumerator ShootGun()
    {
        Debug.Log("gun");
        _anim.SetBool("gun", true);
        float t = 0;
        ParticleSystem gps = Instantiate(_gunChargePS, _bulletSpawnPoint);
        while (t < _gunStartupDelay)
        {

            t += Time.deltaTime;
            yield return null;
            AimGun();
        }
        

        t = 0;
        while (t < _gunDuration)
        {
            float dt = 0;
            while (dt < _gunDamageInterval)
            {
                AimGun();
                dt += Time.deltaTime;
                t += Time.deltaTime;
                yield return null;
            }
            StartCoroutine(Shoot());
        }
        //Destroy(gps.gameObject);
        _anim.SetBool("gun", false);
        Stun(_gunStunTime);
        _canShoot = false;
        Invoke("AllowShoot", _gunCooldown);
        Invoke("AllowAttack", _attackCooldown);
        _gunCR = null;
    }
    private IEnumerator Shoot()
    {
        Vector3 aimDir = _aimTransform.position - _bulletSpawnPoint.position;
        _gunRay = new Ray(_bulletSpawnPoint.position, aimDir);
        RaycastHit hit;
        ParticleSystem ps = Instantiate(_shotPS, _bulletSpawnPoint);
        if (Physics.Raycast(_gunRay, out hit, 100f, _mask))
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
                PlayerActions pa = hit1.collider.GetComponentInParent<PlayerActions>();

                if (pa!=null)
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
    #endregion


    //Flamethrower (done?) [1]
    #region Flamethrower
    private void FindFlameCheckpoint()
    {
        int r = Random.Range(0,_fireCheckpoints.Length);
        if (_fireCheckpoints[r] != _selectedFlameCheckpoint)
        {
            _selectedFlameCheckpoint = _fireCheckpoints[r];
        }
        else
        {
            FindFlameCheckpoint();
        }
    }
    private void MoveFlamethrower()
    {
        if (Vector3.Distance(transform.position, _selectedFlameCheckpoint.position) > _checkpointDetectionDistance)
        {
            dir = _selectedFlameCheckpoint.position - transform.position;
            dir.Normalize();
            rb.velocity = dir * _flameSpeed;
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            dir.y = 0;
            transform.forward = Vector3.Lerp(transform.forward, dir, 1 - Mathf.Pow(0.1f, Time.deltaTime));
        }
        else
        {
            FindFlameCheckpoint();
        }
    }
    private IEnumerator FlamethrowerAttack()
    {
        Debug.Log("flamethrower");
        _canAttack = false;
        float t = 0;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position;
        

        if (Physics.Raycast(new Ray(startPos, transform.up), _flamethrowerHeight + _punchHeight))
        {
            while (Vector3.Distance(transform.position, _middleTransform.position) > 1f)
            {
                dir = _middleTransform.position - transform.position;
                dir.Normalize();
                _punchRB.transform.position = _punchStartTransform.position;
                _punchRB.transform.rotation = _punchStartTransform.rotation;
                rb.velocity = dir * _barrageSpeed;
                yield return null;
            }
        }

        startPos = transform.position;
        endPos = transform.position + transform.up * _flamethrowerHeight;

        while (t < _flameWindup/2)
        {
            transform.position = Vector3.Lerp(startPos, endPos, t / (_flameWindup/2));
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        _anim.SetBool("flamethrower", true);
        while (t < _flameWindup)
        {
            t += Time.deltaTime;
            yield return null;
        }


        ParticleSystem ps = Instantiate(_flamePS, _bulletSpawnPoint);
        
        FindFlameCheckpoint();
        _flameCollider.enabled = true;
        t = 0;

        while(t < _flameDuration)
        {
            MoveFlamethrower();
            t += Time.deltaTime;
            yield return null;
        }

        if (Physics.Raycast(new Ray(startPos, -transform.up), _flamethrowerHeight/2))
        {
            while (Vector3.Distance(transform.position, _middleTransform.position) > 1f)
            {
                dir = _middleTransform.position - transform.position;
                dir.Normalize();
                rb.velocity = dir * _barrageSpeed;
                yield return null;
            }
        }
        _anim.SetBool("flamethrower", false);
        rb.velocity = Vector3.zero;
        ParticleSystem.EmissionModule pse = ps.emission;
        pse.enabled = false;
        Destroy(ps.gameObject, 4f);
        _flameCollider.enabled = false;

        _canFlamethrow = false;
        Stun(_flameStunTime);
        Invoke("AllowFlame", _flamethrowerCooldown);
        Invoke("AllowAttack", _attackCooldown);
        _flamethrowerCR = null;
        
    }
    private void AllowFlame()
    {
        _canFlamethrow = true;
    }
    #endregion


    // Punch (done?) [2]
    #region Punch

    private void AimPunch()
    {
        _aimTransform.position = playerTranform.position;
        findDirection();
        transform.forward = dir.normalized;
    }
    private IEnumerator StartPunch()
    {
        Debug.Log("punch1");
        _anim.SetBool("punch", true);
        _canAttack = false;
        Vector3 startPos = transform.position;
        Vector3 endPos;

        if (Physics.Raycast(new Ray(startPos, transform.up), out RaycastHit hit, _punchUpRayLength + _punchHeight))
        {
            endPos = startPos + Vector3.Distance(startPos, hit.point) * transform.up;
        }
        else
        {
            endPos = startPos + _punchUpRayLength * transform.up;
        }

        float t = 0;

        while (t < 1)
        {
            rb.MovePosition(Vector3.Lerp(startPos, endPos, t));
            t += Time.deltaTime;
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            yield return null;
        } 

        t = 0;
        
        while (t < _punchWindupTime)
        {
            AimPunch();
            t += Time.deltaTime;
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            yield return null;
        }
        _punchCR = StartCoroutine(ShootPunch());
        
    }
    private IEnumerator ShootPunch()
    {
        Debug.Log("punch2");
        float t = 0;
        Vector3 originalPos = _punchStartTransform.position;
        Destroy(Instantiate(_punchChargePS, _punchStartTransform), _punchShakeTime);
        while(t < _punchShakeTime)
        {
            t += Time.deltaTime;
            _punchRB.transform.position = Random.insideUnitSphere / 5 + originalPos;
            yield return null;
        }

        punchWorking = true;
        _punchRB.transform.position = originalPos;
        t = 0;
        LineRenderer lr = Instantiate(_punchLR, transform.position, Quaternion.identity);
        lr.SetPosition(0, _punchStartTransform.position);

        while (t < 1 && punchWorking)
        {
            _punchRB.MovePosition(Vector3.Lerp(originalPos, _aimTransform.position, t));
            lr.SetPosition(1, _punchRB.transform.position);
            t += Time.deltaTime * _punchShootSpeed/2;
            yield return null;
        }

        _punchCollider.enabled = true;
        Instantiate(_punchPS, _punchCollider.transform.position, Quaternion.identity).Play();
        yield return new WaitForFixedUpdate();
        _punchCollider.enabled = false;
        t = 0;

        while(t < _punchGrabDelay)
        {
            t += Time.deltaTime;
            yield return null;
        }

        Vector3 currentpos = _punchRB.position;

        t = 0;

        _anim.SetBool("punch", false);
        while (t < 1)
        {
            _punchRB.MovePosition(Vector3.Lerp(currentpos, originalPos, t));

            lr.SetPosition(1, _punchRB.transform.position);
            t += Time.deltaTime * _punchRetractSpeed/10;
            yield return null;
        }
        _canFirePunch = false;
        Destroy(lr.gameObject);
        Stun(_punchStunTime);
        Invoke("AllowPunch", _firePunchCooldown);
        Invoke("AllowAttack", _attackCooldown);
        _punchCR = null;

    }
    private void AllowPunch()
    {
        _canFirePunch = true;
    }

    #endregion


    // Dropkick (done?) [3]
    #region Dropkick
    private IEnumerator StartDropkick()
    {
        Debug.Log("dropkick");
        _canAttack = false;
        float t = 0;
        Vector3 startPos = transform.position;

        dir = _middleTransform.position - transform.position;
        dir.Normalize();
        dir.y = 0;
        while (t < 1) 
        {
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            transform.forward = Vector3.Lerp(transform.forward, dir, 1 - Mathf.Pow(0.1f, Time.deltaTime));
            rb.MovePosition(Vector3.Lerp(startPos, _middleTransform.position, t));
            t += Time.deltaTime/2;
            yield return null;
        }
        _anim.SetBool("dropkick", true);
        t = 0;

        while (t < _dropkickWindupTime)
        {
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            t += Time.deltaTime;
            AimPunch();
            yield return null;
        }
        t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            yield return null;
        }
        t = 0;
        _dropkickWorking = true;
        startPos = transform.position;
        while (t < 1 && _dropkickWorking)
        {
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            rb.MovePosition(Vector3.Lerp(startPos, _aimTransform.position, t));
            t += Time.deltaTime * _dropkickSpeed/2;
            yield return null;
        }
        _dropkickWorking = false;
        _anim.SetBool("dropkick", false);

        _dropkickDmgCollider.enabled = true;
        yield return new WaitForFixedUpdate();
        _dropkickDmgCollider.enabled = false;

        Instantiate(_dropkickPS, transform.position, Quaternion.identity).Play();
        
        Stun(_dropkickStunTime);
        _canDropKick = false;
        Invoke("AllowAttack", _attackCooldown);
        Invoke("AllowDropkick", _dropKickCooldown);
        _dropkickCR = null;
    }

    private void AllowDropkick()
    {
        _canDropKick = true;
    }



    #endregion


    //Barrage (done?) [4]
    #region Barrage
    private void MoveBarrage()
    {
        if (Vector3.Distance(transform.position, playerTranform.position) > _barrageDistance)
        {
            dir = playerTranform.position - transform.position;
            dir.Normalize();
            rb.velocity = dir * _barrageSpeed;
            transform.forward = Vector3.Lerp(transform.forward, dir, Mathf.Pow(0.5f, Time.deltaTime));
        }
        else if (!_inBarrageRange)
        {
            _inBarrageRange = true;
        }
    }
    private IEnumerator StartBarrage()
    {
        Debug.Log("spin");
        _inBarrageRange = false;
        ParticleSystem ps = Instantiate(_barragePS, transform);
        while(_inBarrageRange == false)
        {
            MoveBarrage();
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            yield return null;
        }
        _anim.SetBool("spin", true);
        for (int i = 0; i < _barrageAttackQuantity; i++)
        {
            float t = 0;
            while (t < _barrageAttackInterval)
            {
                _punchRB.transform.position = _punchStartTransform.position;
                _punchRB.transform.rotation = _punchStartTransform.rotation;
                t += Time.deltaTime;
                yield return null;
            }
            StartCoroutine(BarrageHit());
        }

        _anim.SetBool("spin", false);
        Stun(_barrageStunTime);
        Destroy(ps.gameObject);
        _canPunchBarrage = false;



        Invoke("AllowBarrage", _barrageCooldown);
        Invoke("AllowAttack", _attackCooldown);
        _barrageCR = null;
    }
    private IEnumerator BarrageHit()
    {
        float t = 0;
        _barrageCollider.enabled = true;
        _inBarrageRange = false;
        while (t < _barrageAttackDuration)
        {
            if (!_inBarrageRange) MoveBarrage();
            t += Time.deltaTime;
            yield return null;
        }
        _barrageCollider.enabled = false;
    }
    private void AllowBarrage()
    { 
        _canPunchBarrage = true;
    }
    #endregion


    //Shield (done?) [5]
    #region Shield
    private IEnumerator StartShield()
    {
        _canAttack = false;

        _currentShieldHP = _shieldHP;


        Debug.Log("shield");
        rb.useGravity = true;

        float t = 0;

        _anim.SetBool("shield", true);
        while (t < 1f)
        {
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            t += Time.deltaTime;
            yield return null;
        }



        _shieldMR.enabled = true;
        _shieldCollider.enabled = true;
        shielded = true;
        while (t < _shieldTime && _currentShieldHP > 0)
        {
            t += Time.deltaTime;
            _shieldMR.material.color = Color.Lerp(new Color(0.3f, 1, 1, 0.5f), new Color(1, 0, 0, 0.5f), 1f - _currentShieldHP * 1f/_shieldHP);
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            yield return null;
        }

        _shieldCollider.enabled = false;
        _shieldMR.enabled = false;
        shielded = false;
        if (_currentShieldHP > 0)
        {
            _anim.SetTrigger("badshield");
            _anim.SetBool("shield", false);
            currentHP += (_healingAmount + currentHP > maxHP) ? (maxHP - currentHP) : _healingAmount;
            Instantiate(_shieldHealPS, transform.position, Quaternion.identity).Play();
            Stun(_shieldStunTime/2);
        }
        else
        {
            _anim.SetBool("shield", false);
            Instantiate(_shieldBreakPS, transform.position, Quaternion.identity).Play();
            Stun(_shieldStunTime);
        }
        rb.useGravity = false;
        _canShield = false;
        Invoke("AllowShield", _shieldCooldown);
        Invoke("AllowAttack", _attackCooldown);
        _shieldCR = null;

    }
    private void AllowShield()
    {
        _canShield = true;
    }
    #endregion

    //EMP (done?) [6]
    #region EMP
    private IEnumerator StartEMP()
    {
        _canAttack = false;
        float t = 0;
        Debug.Log("Emp");
        ParticleSystem ps = Instantiate(_empChargePS, transform.position, Quaternion.identity);
        _anim.SetTrigger("emp");
        while (t < _empWindupTime)
        {
            t += Time.deltaTime;
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            yield return null;
        }
        _anim.SetTrigger("empboom");
        DoEMP();
        _canEMP = false;
        Destroy(ps.gameObject);
        Stun(_empStunTime);
        Invoke("AllowEMP", _empCooldown);
        Invoke("AllowAttack", _attackCooldown);
        _empCR = null;
    }
    private void DoEMP()
    {
        dir = playerTranform.position - transform.position;
        _empRay = new Ray(transform.position, dir);
        ParticleSystem ps = Instantiate(_empPS, transform.position, Quaternion.identity);
        ps.Play();
        if (Physics.Raycast(_empRay, out RaycastHit hit, _empRange))
        {
            PlayerActions p = hit.collider.GetComponentInParent<PlayerActions>();
            if (p != null)
            {
                StartCoroutine(p.GetEMPd(_empDuration));
            }
        }
    }
    private void AllowEMP()
    {
        _canEMP = true;
    }
    #endregion


    public override void ShieldDamage(int dmg)
    {
        _currentShieldHP -= dmg;
    }


    protected override void OnDestroy()
    {
        if (!PlayerActions.dead)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 1;
            SceneManager.LoadScene("win");
        }
    }
    private void AllowAttack()
    {
        _canAttack = true;
    }

    private void SelectAttack()
    {
        if (!_canAttack) return;
        state = ATTACKING;
        _selectedAttack = Random.Range(0, 7);
        rb.velocity = Vector3.zero;
        switch (_selectedAttack)
        {
            case 0:
                if (_canShoot && _gunCR == null) _gunCR = StartCoroutine(ShootGun());
                else state = IDLE;
                    break;

            case 1:
                if (_canFlamethrow && _flamethrowerCR == null) _flamethrowerCR = StartCoroutine(FlamethrowerAttack());
                else state = IDLE;
                break;

            case 2:
                if (_canFirePunch && _punchCR == null) _punchCR = StartCoroutine(StartPunch());
                else state = IDLE;
                break;

            case 3:
                if (_canDropKick && _dropkickCR == null) _dropkickCR = StartCoroutine(StartDropkick());
                else state = IDLE;
                break;

            case 4:
                if (_canPunchBarrage && _barrageCR == null) _barrageCR = StartCoroutine(StartBarrage());
                else state = IDLE;
                break;

            case 5:
                if(_canShield && _shieldCR == null) _shieldCR = StartCoroutine(StartShield());
                else state = IDLE;
                break;

            case 6:
                if(_canEMP && _empCR == null) _empCR = StartCoroutine(StartEMP());
                else state = IDLE;
                break;

            default:
                state = IDLE;
                break;
        }
    }

    private void Update()
    {
        if(state == SEEKING || state == IDLE)
        {
            findDirection();
            rb.velocity = dir.normalized * speed;
            transform.forward = Vector3.Lerp(transform.forward, dir, Mathf.Pow(0.1f, Time.deltaTime));
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
            HPDisplay.text = currentHP + "/" + maxHP;
            if (Random.Range(0, 100) < 5 && _canAttack)
            {
                SelectAttack();
                Debug.Log("attack");
            }
        }
        else if (state == STUNNED)
        {
            _punchRB.transform.position = _punchStartTransform.position;
            _punchRB.transform.rotation = _punchStartTransform.rotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody pRB = other.GetComponentInParent<Rigidbody>();

        if (other.GetComponentInParent<PlayerActions>())
        {
            if (_barrageCollider.enabled)
            {
                player.takeDamage(_barrageHitDamage);
                pRB.drag = 0;
                findDirection();
                pRB.AddForce((dir + Vector3.up).normalized * _barrageKnockback, ForceMode.Impulse);
            }
            else if (_dropkickDmgCollider.enabled)
            {
                player.takeDamage(_dropkickDamage);
                pRB.drag = 0;
                findDirection();
                pRB.AddForce((dir + Vector3.up).normalized * _dropkickKnockback, ForceMode.Impulse);
            }
            else if (_flameCollider.enabled)
            {
                pRB.drag = 0;
                findDirection();
                pRB.AddForce((dir + Vector3.up).normalized * _flamethrowerKnockback, ForceMode.Impulse);
                player.takeDamage(_fireDamage);
            }
        }
    }

    public void HitFist(Vector3 _dir)
    {
        Rigidbody pRB = player.GetComponentInParent<Rigidbody>();


        pRB.drag = 0;
        pRB.AddForce((_dir + Vector3.up).normalized * _punchKnockback, ForceMode.Impulse);
        player.takeDamage(_punchDamage);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(_dropkickWorking) _dropkickWorking = false;
    }


}
