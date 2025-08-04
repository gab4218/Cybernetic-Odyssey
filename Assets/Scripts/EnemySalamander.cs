using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySalamander : EnemyBase
{
    private bool _hasTail = true;
    private bool _canAttack = true;
    private Vector2 _randomPos;

    [Header("General Stuff")]
    [SerializeField] private EnemyTail _tail;
    [SerializeField] private Transform _tailTransform;
    [SerializeField] private Transform _tailBoneTransform;
    [SerializeField] private SkinnedMeshRenderer _tailGeoSMR;
    [SerializeField] private Transform _tailGeoDestination;
    [SerializeField] private Transform[] _newTailBones;
    [SerializeField] private Animator _anim;
    [SerializeField] private int _tailLossHP = 300;
    [SerializeField] private float _attackCooldown = 5f;

    [Header("Bite")]
    [SerializeField] private Collider[] _biteTailColliders;
    [SerializeField] private Collider _biteCollider;
    [SerializeField] private Transform _biteTransform;
    [SerializeField] private int _biteTickDamage = 5;
    [SerializeField] private int _biteHitCount = 2;
    [SerializeField] private float _biteCooldown = 15f;
    [SerializeField] private float _biteLength = 3f;
    [SerializeField] private float _biteInterval = 0.25f;
    [SerializeField] private float _biteDistance = 5f;
    [SerializeField] private float _biteStunTime = 1.5f;
    [SerializeField] private AudioSource _biteSound;
    private bool _canBite = true;
    private int _currentHitCount = 0;
    private Coroutine _biteCR;

    [Header("Tail Whip")]
    [SerializeField] private Collider _whipCollider;
    [SerializeField] private int _whipDamage = 30;
    [SerializeField] private float _tailWhipCooldown = 10f;
    [SerializeField] private float _tailWhipDistance = 7f;
    [SerializeField] private float _whipStunTime = 1f;
    [SerializeField] private float _whipKnockback = 5f;
    [SerializeField] private AudioSource _whipSound;
    private bool _canWhip = true;

    [Header("Fireball")]
    [SerializeField] private Rigidbody _fireballRB;
    [SerializeField] private Transform _fireballSpawn;
    [SerializeField] private int _fireballQuantity = 3;
    [SerializeField] private float _fireballLaunchInterval = 1f;
    [SerializeField] private float _fireballCooldown = 20f;
    [SerializeField] private float _fireballSpeed = 5f;
    [SerializeField] private float _fireballStunTime = 2f;
    [SerializeField] private float _fireballMinDistance = 10f;
    [SerializeField] private AudioSource _fireballSound;
    private bool _canFireball = true;
    private Coroutine _fireballCR;

    [Header("Tornado")]
    [SerializeField] private Rigidbody _tornadoRB;
    [SerializeField] private float _tornadoCooldown = 25f;
    [SerializeField] private float _tornadoSpeed = 3f;
    [SerializeField] private float _tornadoWindup = 2f;
    [SerializeField] private float _tornadoStunTime = 3f;
    [SerializeField] private float _tornadoMinDistance = 15f;
    private Coroutine _tornadoCR;
    private bool _canTornado = true;


    protected override void Start()
    {
        base.Start();
        if (ProgressManager.beatSalamander)
        {
            Destroy(gameObject);
            maxHP = (int)(maxHP * 1.5f);
            currentHP = maxHP;
            _biteTickDamage = (int)(1.2f * _biteTickDamage);
            _whipDamage = (int)(1.2f * _whipDamage);
        }
    }


    #region Bite

    private void Bite()
    {
        state = ATTACKING;
        _anim.SetTrigger("bite");
        _canAttack = false;
        _canBite = false;
        navMeshAgent.isStopped = true;
    }

    public void WallBite()
    {
        foreach (Collider c in _biteTailColliders)
        {
            c.enabled = true;
        }
    }

    public void EndWallBite()
    {
        foreach (Collider c in _biteTailColliders)
        {
            c.enabled = false;
        }
    }

    public void DoBite()
    {
        _biteCollider.enabled = true;
        canParry = true;
        AudioSource aS = Instantiate(_biteSound, transform.position, Quaternion.identity);
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);

    }




    public void EndBite()
    {
        _biteCollider.enabled = false;
        canParry = false;
        Stun(_biteStunTime);
        Invoke("AllowBite", _biteCooldown);
        Invoke("AllowAttack", _attackCooldown);
        player.Ungrab();
    }

    private IEnumerator GrabPlayer()
    {
        float t = 0;
        _currentHitCount = 0;
        _anim.SetTrigger("grabPlayer");
        float tt = 0;
        while (t < _biteLength && _currentHitCount < _biteHitCount)
        {
            player.transform.position = Vector3.Lerp(player.transform.position, _biteTransform.position, 1 - Mathf.Pow(0.1f, Time.deltaTime));
            t += Time.deltaTime;
            tt += Time.deltaTime;
            if (tt > _biteInterval)
            {
                player.takeDamage(_biteTickDamage);
                tt = 0;
            }
            yield return null;
        }
        _anim.SetTrigger("endBite");
        _biteCR = null;
    }

    private void AllowBite()
    {
        _canBite = true;
    }

    #endregion


    #region Whip

    private void Whip()
    {
        state = ATTACKING;
        _anim.SetTrigger("whip");
        _canAttack = false;
        _canWhip = false;
        navMeshAgent.isStopped = true;
        AudioSource aS = Instantiate(_whipSound, transform.position, Quaternion.identity);
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);
    }

    public void DoWhip()
    {
        _whipCollider.enabled = true;
        canParry = true;
    }

    public void EndWhip()
    {
        _whipCollider.enabled = false;
        Stun(_whipStunTime);
        canParry = false;
        Invoke("AllowWhip", _tailWhipCooldown);
        Invoke("AllowAttack", _attackCooldown);
    }

    private void AllowWhip()
    {
        _canWhip = true;
    }

    #endregion


    #region Fireball

    private IEnumerator ShootFireballs()
    {
        state = ATTACKING;
        _canAttack = false;
        _canFireball = false;
        navMeshAgent.isStopped = true;
        _anim.SetBool("walking", false);
        for (int i = 0; i < _fireballQuantity; i++)
        {
            _anim.SetTrigger("fireball");
            float t = 0;
            while (t < _fireballLaunchInterval/2f)
            {
                t += Time.deltaTime;
                dir = player.transform.position - transform.position;
                transform.forward = Vector3.Lerp(transform.forward, dir, 1 - Mathf.Pow(0.1f, Time.deltaTime));
                yield return null;
            }
            dir.Normalize();
            AudioSource aS = Instantiate(_fireballSound, transform.position, Quaternion.identity);
            aS.Play();
            Destroy(aS.gameObject, aS.clip.length);
            Rigidbody r = Instantiate(_fireballRB, _fireballSpawn.position, Quaternion.identity);
            r.velocity = dir * _fireballSpeed * Vector3.Distance(_fireballSpawn.position, player.transform.position);
            r.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            while (t < _fireballLaunchInterval)
            {
                t += Time.deltaTime;
                
                yield return null;
            }
        }
        Invoke("AllowFireball", _fireballCooldown);
        Invoke("AllowAttack", _attackCooldown);
        Stun(_fireballStunTime);
        _fireballCR = null;

    }

    private void AllowFireball()
    {
        _canFireball = true;
    }


    #endregion


    #region Tornado
    private IEnumerator StartTornado()
    {
        state = ATTACKING;
        _canAttack = false;
        _canTornado = false;
        navMeshAgent.isStopped = true;

        float t = 0;
        _anim.SetTrigger("tornado");
        Rigidbody r = Instantiate(_tornadoRB, transform.position, Quaternion.identity);
        r.useGravity = false;

        while (t < _tornadoWindup)
        {
            t += Time.deltaTime;
            dir = player.transform.position - transform.position;
            dir.Normalize();
            transform.forward = Vector3.Lerp(transform.forward, dir, 1f - Mathf.Pow(0.1f, Time.deltaTime));
            yield return null;
        }

        Stun(_tornadoStunTime);
        r.useGravity = true;
        r.detectCollisions = true;
        r.velocity = dir * _tornadoSpeed;

        
        Invoke("AllowAttack", _attackCooldown);
        Invoke("AllowTornado", _tornadoCooldown);
    }


    private void AllowTornado()
    {
        _canTornado = true;
    }

    #endregion


    protected override void OnDestroy()
    {
        if (ProgressManager.beatSalamander)
        {
            if (HPDisplay != null) HPDisplay.gameObject.SetActive(false);
            ProgressManager.refoughtSalamander = true;
            return;
        }
        if (!PlayerActions.dead)
        {
            if (SoundSingleton.Instance != null)
            {
                ProgressManager.beatSalamander = true;
                SoundSingleton.Instance.OsoMuerte();
            }
            if (HPDisplay != null) HPDisplay.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        if (Pause.paused) return;
        if (CameraController.inCutscene) return;
        if (currentHP <= _tailLossHP && _hasTail && ProgressManager.beatSalamander)
        {
            _hasTail = false;
            ReleaseTail();
        }
        detectPlayer();

        if (state == SEEKING) //Si se persigue al jugador, atacarlo cuando sea posible 
        {
            findDirection();
            _anim.SetBool("walking", true);
            if (_canAttack)
            {
                ChooseAttack();
            }

        }
        else if (state == IDLE) //Si no sigue al jugador, moverse a una posicion random
        {
            if (hasReachedDestination(_randomPos))
            {
                _randomPos = new Vector2
                                (
                                    Random.Range(randomMovementDimensions[0].x, randomMovementDimensions[1].x),
                                    Random.Range(randomMovementDimensions[0].z, randomMovementDimensions[1].z)
                                );
            }
            else
            {
                setDestination(_randomPos);
                _anim.SetBool("walking", true);
            }
        }

    }

    private void AllowAttack()
    {
        _canAttack = true;
    }

    private void ReleaseTail()
    {
        Vector3 v = _tailBoneTransform.position;
        v.y = transform.position.y;
        _tail.transform.position = v;
        _tailBoneTransform.parent = _tailTransform;
        _tailGeoSMR.transform.parent = _tailGeoDestination;
        _tailGeoSMR.bones = _newTailBones;
        _tailGeoSMR.rootBone = _newTailBones[0];
        _tail.enabled = true;
    }


    private void ChooseAttack()
    {
        Debug.Log("attacked");
        switch (Random.Range(0, 4))
        {
            case 0:
                if (Vector3.Distance(transform.position, player.transform.position) <= _biteDistance && _canBite)
                {
                    Bite();
                }
                break;
            case 1:
                if (Vector3.Distance(transform.position, player.transform.position) <= _tailWhipDistance && _canWhip)
                {
                    Whip();
                }
                break;
            case 2:
                if (Vector3.Distance(transform.position, player.transform.position) >= _fireballMinDistance && _canFireball && _fireballCR == null)
                {
                    _fireballCR = StartCoroutine(ShootFireballs());
                }
                break;
            case 3:
                if (Vector3.Distance(transform.position, player.transform.position) <= _tornadoMinDistance && _canTornado && _tornadoCR == null)
                {
                    _tornadoCR = StartCoroutine(StartTornado());
                }
                break;
            default:
                break;
        }
    }

    public override void takeDamage(int dmg, PlayerActions.damageType dmgType)
    {
        if (_biteCR != null)
        {
            _currentHitCount++;
            base.takeDamage(dmg / 2, dmgType);
        }
        else
        {
            base.takeDamage(dmg, dmgType);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_biteCollider.enabled)
        {
            if (other.GetComponentInParent<PlayerActions>())
            {
                player.GetGrabbed();
                _biteCR = StartCoroutine(GrabPlayer());
                canParry = false;
            }
        }
        else if (_whipCollider.enabled)
        {
            if (other.GetComponentInParent<PlayerActions>())
            {
                player.takeDamage(_whipDamage);
                Rigidbody pRB = player.GetComponent<Rigidbody>();
                pRB.velocity = Vector3.zero;
                pRB.AddForce(dir.normalized *_whipKnockback + Vector3.up * 2, ForceMode.Impulse);
                canParry = false;
            }
        }
    }

}
