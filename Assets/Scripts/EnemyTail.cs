using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyTail : EnemyBase
{
    [Header("General")]
    [SerializeField] private Animator _anim;
    [SerializeField] private Image _hpD;
    [SerializeField] private float _attackCooldown = 5f;
    private bool _canAttack = true;
    private Vector2 _randomPos;

    [Header("Lunge")]
    [SerializeField] private Collider _lungeCollider;
    [SerializeField] private ParticleSystem _lungePS;
    [SerializeField] private int _lungeDamage = 20;
    [SerializeField] private float _lungeStunTime = 1f;
    [SerializeField] private float _lungeCooldown = 15f;
    [SerializeField] private float _lungeKnockback = 10f;
    [SerializeField] private float _lungeSpeed = 5f;
    private bool _canLunge = true;
    private bool _lunging = false;


    [Header("Fling")]
    [SerializeField] private Collider _flingCollider;
    [SerializeField] private ParticleSystem _flingPS;
    [SerializeField] private int _flingDamage = 35;
    [SerializeField] private float _flingKnockback = 15f;
    [SerializeField] private float _flingCooldown = 20f;
    [SerializeField] private float _flingStunTime = 1.5f;
    [SerializeField] private float _flingRange = 7f;


    [Header("Burrow")]
    [SerializeField] private Transform[] _lakes;
    [SerializeField] private Rigidbody _fireballRB;
    [SerializeField] private int _fireballQuantity = 12;
    [SerializeField] private float _burrowCooldown = 15f;
    [SerializeField] private float _fireballSpeed = 5f;
    [SerializeField] private float _burrowInbetweenTime = 2f;
    private bool _canBurrow = true;
    private bool _burrowing = false;
    private Transform _selectedLake;
    
    private bool _canFling = true;
    
    

    private void OnEnable()
    {
        if (ProgressManager.beatSalamander)
        {
            //_hpD.gameObject.SetActive(true);
            maxHP = (int)(maxHP * 1.5f);
            currentHP = maxHP;
            _flingDamage = (int)(1.2f * _flingDamage);
            _lungeDamage = (int)(1.2f * _lungeDamage);
            GetComponent<NavMeshAgent>().enabled = true;
            base.Start();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    #region Lunge

    private void StartLunge()
    {
        _anim.SetTrigger("lunge");
        Debug.Log("lunge");
        state = ATTACKING;
        _canAttack = false;
        navMeshAgent.isStopped = true;
        _canLunge = false;
    }

    public void DoLunge()
    {
        dir = player.transform.position - transform.position;
        dir.y = 0;
        dir.Normalize();
        
        rb.velocity = dir * _lungeSpeed + Vector3.up;
        canParry = true;
        _lunging = true;
    }

    public void EndLunge()
    {
        canParry = false;
        _lungeCollider.enabled = false;
        _lunging = false;
        rb.velocity = Vector3.zero;
        Stun(_lungeStunTime);
        Invoke("AllowAttack", _attackCooldown);
        Invoke("AllowLunge", _lungeCooldown);
    }

    private void AllowLunge()
    {
        _canLunge = true;
    }

    #endregion

    #region Fling
    private void StartFling()
    {
        Debug.Log("fling");
        _anim.SetTrigger("fling");
        state = ATTACKING;
        _canAttack = false;
        _canFling = false;
    }

    public void DoFling()
    {
        _flingCollider.enabled = true;
        canParry = true;
    }

    public void EndFling()
    {
        canParry = false;
        _flingCollider.enabled = false;
        Stun(_flingStunTime);
        Invoke("AllowAttack", _attackCooldown);
        Invoke("AllowFling", _flingCooldown);
    }

    private void AllowFling()
    {
        _canFling = true;
    }


    #endregion

    #region Burrow
    
    private void StartBurrow()
    {
        _canAttack = false;
        _canBurrow = false;
        _burrowing = true;
        _anim.SetBool("walking", true);
        _selectedLake = _lakes[Random.Range(0, _lakes.Length)];
        foreach (Transform t in _lakes)
        {
            if (Vector3.Distance(transform.position, t.position) < Vector3.Distance(transform.position, _selectedLake.position))
            {
                _selectedLake = t;
            }
        }
        setDestination(_selectedLake.position);
        Debug.Log("burrow");
    }

    private void ReachedBurrow()
    {
        _burrowing = false;
        navMeshAgent.isStopped = true;
        state = ATTACKING;
        _anim.SetTrigger("burrow");
    }

    public void Burrow()
    {
        Transform t = _selectedLake;
        while (t == _selectedLake)
        {
            _selectedLake = _lakes[Random.Range(0, _lakes.Length)];
        }
        transform.position = _selectedLake.position;
        Stun(_burrowInbetweenTime);
        Invoke("AllowAttack", _attackCooldown);
        Invoke("AllowBurrow", _burrowCooldown);
    }

    public void ShootFireballs()
    {
        for (int i = 0; i < _fireballQuantity; i++)
        {
            Rigidbody r = Instantiate(_fireballRB, _selectedLake.transform.position + Vector3.up * 2, Quaternion.identity);
            Vector3 d = Random.onUnitSphere;
            d.y = 0;
            d.Normalize();
            r.velocity = d * _fireballSpeed;
            r.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }

    

    private void AllowBurrow()
    {
        _canBurrow = true;
    }


    #endregion


    private void Update()
    {
        if (Pause.paused) return;
        if (CameraController.inCutscene) return;
        detectPlayer();

        if (state == SEEKING) //Si se persigue al jugador, atacarlo cuando sea posible 
        {
            if (_burrowing)
            {
                if (!navMeshAgent.pathPending)
                {
                    if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                    {
                        if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                        {
                            ReachedBurrow();
                        }
                    }
                }
            }
            else
            {
                findDirection();
                _anim.SetBool("walking", true);
                if (_canAttack)
                {
                    ChooseAttack();
                }
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

    private void OnCollisionEnter(Collision collision)
    {
        if (_lunging)
        {
            Debug.Log("lungehit");
            _lungeCollider.enabled = true;
            ParticleSystem p = Instantiate(_lungePS, transform.position, Quaternion.identity);
            p.Play();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (_lungeCollider.enabled)
        {
            if (other.GetComponentInParent<PlayerActions>())
            {
                player.takeDamage(_lungeDamage);
                _lungeCollider.enabled = false;
                Rigidbody prb = player.GetComponent<Rigidbody>();
                dir = player.transform.position - transform.position;
                dir.y = 0;
                dir.Normalize();
                prb.AddForce(dir * _lungeKnockback + Vector3.up * 2f, ForceMode.Impulse);
            }
        } 
        else if (_flingCollider.enabled)
        {
            if (other.GetComponentInParent<PlayerActions>())
            {
                player.takeDamage(_flingDamage);
                _flingCollider.enabled = false;
                Rigidbody prb = player.GetComponent<Rigidbody>();;
                prb.AddForce(Vector3.up * _flingKnockback, ForceMode.Impulse);
            }
        }
    }

    private void ChooseAttack()
    {
        switch (Random.Range(0, 4))
        {
            case 0:
                if (_canLunge)
                {
                    StartLunge();
                }
                break;
            case 1:
                if (Vector3.Distance(transform.position, player.transform.position) <= _flingRange && _canFling)
                {
                    StartFling();
                }
                break;
            case 3:
                if (_canBurrow)
                {
                    StartBurrow();
                }
                break;
            default:
                break;
        }
    }

}
