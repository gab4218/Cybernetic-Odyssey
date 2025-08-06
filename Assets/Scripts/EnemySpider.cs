using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class EnemySpider : EnemyBase
{
    private bool _canAttack = true;
    private Vector3 _randomPos;
    private bool _aiming = false;

    [Header("General")]
    [SerializeField] private Animator _anim;
    [SerializeField] private Transform[] _checkpoints;
    [SerializeField] private Image _venomIMG;
    [SerializeField] private float _attackCooldown = 5f;
    [SerializeField] private LayerMask _mask;
    [SerializeField] private float _extraAttackDistance = 25f;

    [Header("Acid")]
    [SerializeField] private Rigidbody _acidRB;
    [SerializeField] private Transform _acidSpawn;
    [SerializeField] private float _acidLaunchSpeed = 1f;
    [SerializeField] private float _acidStunTime = 1f;
    [SerializeField] private float _acidCooldown = 10f;
    [SerializeField] private AudioSource _hissSound;
    private bool _canAcid = true;

    [Header("Grab")]
    [SerializeField] private LineRenderer _grabLR;
    [SerializeField] private Transform _webSpawn;
    [SerializeField] private GameObject _grabUI;
    [SerializeField] private int _spacebarCount = 6;
    [SerializeField] private float _grabShootSpeed = 8f;
    [SerializeField] private float _grabSpeed = 3f;
    [SerializeField] private float _minGrabDistance = 10f;
    [SerializeField] private float _releaseDistance = 6f;
    [SerializeField] private float _grabStunTime = 1.25f;
    [SerializeField] private float _grabCooldown = 15f;
    [SerializeField] private AudioSource _defaultSound;
    private bool _canGrab = true;
    private bool _grabbing = false; 
    private LineRenderer _currentLR;
    private Ray _grabRay;
    private Coroutine _grabCR;

    [Header("Bite")]
    [SerializeField] private Collider _biteCollider;
    [SerializeField] private int _biteDamage = 25;
    [SerializeField] private int _venomDamage = 2;
    [SerializeField] private float _venomDuration = 5f;
    [SerializeField] private float _venomInterval = 0.5f;
    [SerializeField] private float _biteDistance = 3f;
    [SerializeField] private float _biteStunTime = 1.5f;
    [SerializeField] private float _biteCooldown = 20f;
    [SerializeField] private AudioSource _biteSound;
    private bool _canBite = true;
    private Coroutine _venomCR;

    [Header("Slow Silk")]
    [SerializeField] private Rigidbody _silkRB;
    [SerializeField] private float _silkShootSpeed = 5f;
    [SerializeField] private float _minSilkDistance = 15f;
    [SerializeField] private float _silkStunTime = 1f;
    [SerializeField] private float _silkCooldown = 15f;
    private bool _canSilk = true;

    [Header("Spawn Spiderlings")]
    [SerializeField] private Rigidbody _spiderlingRB;
    [SerializeField] private Transform _spawnTransform;
    [SerializeField] private int _spiderCount = 5;
    [SerializeField] private float _spiderSpawnInterval = 0.1f;
    [SerializeField] private float _spawnThrowSpeed = 1f;
    [SerializeField] private float _spawnStunTime = 1.5f;
    [SerializeField] private float _spawnCooldown = 20f;
    private bool _canSpawn = true;
    private Coroutine _spawnCR;

    private void AllowAttack()
    {
        _canAttack = true;
    }

    #region Acid

    private void StartAcid()
    {
        _canAttack = false;
        _canAcid = false;
        state = ATTACKING;
        navMeshAgent.isStopped = true;
        _anim.SetTrigger("acid");
        _aiming = true;
    }

    public void ShootAcid()
    {
        _aiming = false;
        Rigidbody r = Instantiate(_acidRB, _acidSpawn.position, Quaternion.identity);
        Vector3 d = player.transform.position - transform.position;
        d.Normalize();
        r.velocity = d  * Vector3.Distance(transform.position, player.transform.position);
        if (ProgressManager.beatSpider)
        {
            r = Instantiate(_acidRB, _acidSpawn.position, Quaternion.identity);
            d = player.transform.position - transform.position;
            d.Normalize();
            d += Random.insideUnitSphere/2f;
            d.Normalize();
            r.velocity = d * Vector3.Distance(transform.position, player.transform.position);

            r = Instantiate(_acidRB, _acidSpawn.position, Quaternion.identity);
            d = player.transform.position - transform.position;
            d.Normalize();
            d += Random.insideUnitSphere/2f;
            d.Normalize();
            r.velocity = d * Vector3.Distance(transform.position, player.transform.position);
        }
        AudioSource aS = Instantiate(_hissSound, transform.position, Quaternion.identity);
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);
       // r.AddForce(Vector3.up * 3f, ForceMode.Impulse);
    }

    public void EndAcid()
    {
        Stun(_acidStunTime);
        Invoke("AllowAcid", _acidCooldown);
        Invoke("AllowAttack", _attackCooldown);
    }

    private void AllowAcid()
    {
        _canAcid = true;
    }


    #endregion

    #region Grab

    private void StartGrab()
    {
        _canAttack = false;
        _canGrab = false;
        navMeshAgent.isStopped = true;
        _anim.SetTrigger("grab");
        _aiming = true;
        state = ATTACKING;
        DoGrab();
    }

    public void DoGrab()
    {
        _aiming = false;
        _currentLR = Instantiate(_grabLR, _webSpawn.position, Quaternion.identity);
        Vector3 d = Camera.main.transform.position - _webSpawn.transform.position;
        d.Normalize();
        _grabRay = new Ray(_webSpawn.position, d);
        AudioSource aS = Instantiate(_defaultSound, transform.position, Quaternion.identity);
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);
        _grabCR = StartCoroutine(ShootLine());
    }

    private IEnumerator ShootLine()
    {
        float t = 0;
        _currentLR.SetPosition(0, _webSpawn.position);
        _currentLR.SetPosition(1, _webSpawn.position);
        if (Physics.Raycast(_grabRay, out RaycastHit hit, _mask))
        {
            while (t < 1)
            {
                _currentLR.SetPosition(1, Vector3.Lerp(_webSpawn.position, hit.point, t));
                t += Time.deltaTime * _grabShootSpeed;
                yield return null;
            }
            t = 0;
            if (Physics.Raycast(_grabRay, out RaycastHit hit2, _mask))
            {
                if (hit2.collider.GetComponentInParent<PlayerActions>())
                {
                    player.GetGrabbed();
                    _grabUI.SetActive(true);
                    Vector3 d = transform.position - player.transform.position;
                    d.y = 0;
                    d.Normalize();
                    Rigidbody pRb = player.GetComponent<Rigidbody>();
                    int c = 0;
                    _grabbing = true;

                    while (Vector3.Distance(transform.position, player.transform.position) > _releaseDistance && c < _spacebarCount && _grabbing)
                    {
                        pRb.velocity = d * _grabSpeed;
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            c++;
                        }
                        _currentLR.SetPosition(1, Camera.main.transform.position);
                        yield return null;
                    }
                    
                    player.Ungrab();
                    pRb.velocity = Vector3.zero;
                    _grabUI.SetActive(false);
                    
                }
                
                _anim.SetTrigger("release");
                Vector3 p = _currentLR.GetPosition(1);
                while (t < 1)
                {
                    _currentLR.SetPosition(1, Vector3.Lerp(p, _webSpawn.position, t));
                    t += Time.deltaTime * _grabShootSpeed;
                    yield return null;
                }
                Destroy(_currentLR.gameObject);
            }
        }
        _anim.SetTrigger("release");
        Stun(_grabStunTime);
        Invoke("AllowAttack", _attackCooldown);
        Invoke("AllowGrab", _grabCooldown);
    }

    private void AllowGrab()
    {
        _canGrab = true;
    }


    #endregion

    #region Bite

    private void StartBite()
    {
        _anim.SetTrigger("bite");
        _canAttack = false;
        _canBite = false;
        state = ATTACKING;
        navMeshAgent.isStopped = true;
        AudioSource aS = Instantiate(_biteSound, transform.position, Quaternion.identity);
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);
    }

    public void DoBite()
    {
        _biteCollider.enabled = true;
        canParry = true;
    }

    public void EndBite()
    {
        _biteCollider.enabled = false;
        canParry = false;
        Stun(_biteStunTime);
        Invoke("AllowBite", _biteCooldown);
        Invoke("AllowAttack", _attackCooldown);
    }

    private IEnumerator Poison(float len)
    {
        float t = 0;
        _venomIMG.gameObject.SetActive(true);
        while (t < len)
        {
            float tt = 0f;
            while (tt < _venomInterval)
            { 
                t += Time.deltaTime;
                tt += Time.deltaTime;
                yield return null;
            }
            player.takeDamage(_venomDamage);
        }
        _venomIMG.gameObject.SetActive(false);
        _venomCR = null;
    }


    private void AllowBite()
    {
        _canBite = true;
    }

    #endregion

    #region Silk

    private void StartSilk()
    {
        _canAttack = false;
        _canSilk = false;
        state = ATTACKING;
        navMeshAgent.isStopped = true;
        _anim.SetTrigger("silk");
        _aiming = true;
    }

    public void ShootSilk()
    {
        _aiming = false;
        Rigidbody r = Instantiate(_silkRB, _webSpawn.position, Quaternion.identity);
        Vector3 d = player.transform.position - transform.position;
        //d.y = 0;
        d.Normalize();
        r.velocity = d * Vector3.Distance(transform.position, player.transform.position);
        r.AddForce(Vector3.up, ForceMode.Impulse);
        AudioSource aS = Instantiate(_defaultSound, transform.position, Quaternion.identity);
        aS.Play();
        Destroy(aS.gameObject, aS.clip.length);
    }

    public void EndSilk()
    {
        Stun(_silkStunTime);
        Invoke("AllowSilk", _silkCooldown);
        Invoke("AllowAttack", _attackCooldown);
    }

    private void AllowSilk()
    {
        _canSilk = true;
    }

    #endregion

    #region Spawn Spiderlings

    private void StartSpawning()
    {
        _anim.SetTrigger("spawn");
        _canAttack = false;
        _canSpawn = false;
        state = ATTACKING;
        navMeshAgent.isStopped = true;
    }
    
    public void SpawnSpiders()
    {
        if (_spawnCR == null)
        {
            _spawnCR = StartCoroutine(Spawning());
        }
    }

    private IEnumerator Spawning()
    {
        for (int i = 0; i < _spiderCount; i++)
        {
            float t = 0;
            while (t < _spiderSpawnInterval)
            {
                t += Time.deltaTime;
                yield return null;
            }
            Rigidbody r = Instantiate(_spiderlingRB, _spawnTransform.position, Quaternion.identity);
            Vector3 d = player.transform.position - transform.position;
            d = d.normalized + Random.insideUnitSphere;
            r.velocity = d.normalized * _spawnThrowSpeed * Vector3.Distance(transform.position, player.transform.position) + Vector3.up * 2f;
        }
        _spawnCR = null;
    }

    public void EndSpawn()
    {
        Stun(_spawnStunTime);
        Invoke("AllowAttack", _attackCooldown);
        Invoke("AllowSpawn", _spawnCooldown);
    }

    private void AllowSpawn()
    {
        _canSpawn = true;
    }

    #endregion

    private void Update()
    {
        if (Pause.paused) return;
        if (CameraController.inCutscene) return;
        detectPlayer();
        //SetUpVector();

      

        if (state == SEEKING) //Si se persigue al jugador, atacarlo cuando sea posible 
        {
            findDirection();
            _anim.SetBool("walking", true);
            if (_canAttack && !navMeshAgent.isOnOffMeshLink)
            {
                ChooseAttack();
            }

        }
        else if (state == IDLE) //Si no sigue al jugador, moverse a una posicion random
        {
            if (hasReachedDestination(_randomPos))
            {
                _randomPos = _checkpoints[Random.Range(0, _checkpoints.Length)].position;
            }
            else
            {
                setDestination(_randomPos);
                _anim.SetBool("walking", true);
            }
            if (_canAttack && !navMeshAgent.isOnOffMeshLink && Vector3.Distance(transform.position, player.transform.position) < _extraAttackDistance)
            {
                ChooseAttack();
            }
        }

    }

    protected override void OnDestroy()
    {
        if (ProgressManager.beatSpider)
        {
            if (HPDisplay != null) HPDisplay.gameObject.SetActive(false);
            ProgressManager.refoughtSpider = true;
            return;
        }
        if (!PlayerActions.dead)
        {
            if (SoundSingleton.Instance != null)
            {
                ProgressManager.beatSpider = true;
                SoundSingleton.Instance.OsoMuerte();
            }
            if (HPDisplay != null) HPDisplay.gameObject.SetActive(false);
        }
    }

    protected override void Start()
    {
        base.Start();
        navMeshAgent.updateUpAxis = true;
        _randomPos = _checkpoints[Random.Range(0, _checkpoints.Length)].position;
        if (HPDisplay != null) //Si se puede mostrar HP, mostrarla
        {
            HPDisplay.text = $"Boss HP: {Mathf.Max(currentHP, 0)}/{maxHP}";
        }
        if (ProgressManager.beatSpider)
        {
            //Destroy(gameObject);
            maxHP = (int)(maxHP * 1.5f);
            currentHP = maxHP;
            drops = false;
            _biteDamage = (int)(1.2f * _biteDamage);
            _venomDamage = (int)(1.2f * _venomDamage);
        }
    }


    private void ChooseAttack()
    {
        _anim.SetBool("walking", false);
        switch (Random.Range(0, 5))
        {
            case 0:
                if (_canBite && Vector3.Distance(transform.position, player.transform.position) <= _biteDistance)
                {
                    StartBite();
                }
                break;
            case 1:
                if (_canGrab && Vector3.Distance(transform.position, player.transform.position) >= _minGrabDistance)
                {
                    StartGrab();
                }
                break;
            case 2:
                if (_canSilk && Vector3.Distance(transform.position, player.transform.position) >= _minSilkDistance)
                {
                    StartSilk();
                }
                break;
            case 3:
                if (_canSpawn)
                {
                    StartSpawning();
                }
                break;
            case 4:
                if (_canAcid)
                {
                    StartAcid();
                }
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_biteCollider.enabled)
        {
            if (other.GetComponentInParent<PlayerActions>())
            {
                _biteCollider.enabled = false;
                if (_venomCR == null)
                {
                    _venomCR = StartCoroutine(Poison(_venomDuration));
                }
                //canParry = false;
                player.takeDamage(_biteDamage);
            }
        }
    }

    public override void takeDamage(int dmg, PlayerActions.damageType dmgType)
    {
        if (_grabbing)
        {
            _grabbing = false;
        }
        base.takeDamage(dmg, dmgType);
    }



    private void SetUpVector()
    {
        Ray r = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            transform.up = hit.normal;
        }
    }

}
