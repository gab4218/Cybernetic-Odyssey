using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acid : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _lifeSpan = 5f;
    [SerializeField] private float _fadeTime = 1.5f;
    [SerializeField] public bool permanent = false;
    [SerializeField] private bool _boss = false;
    [SerializeField] private Animator _anim;
    private float _time = 0;
    private bool _active = true;
    private PlayerActions _player;
    private Coroutine _cr;

    private void Start()
    {
        if (_boss)
        {
            if (ProgressManager.beatSpider)
            {
                _anim.enabled = true;
            }
            else
            {
                _anim.enabled = false;
            }
        }
    }

    protected virtual void Update()
    {
        if (Pause.paused) return;
        if (CameraController.inCutscene) return;
        if (permanent) return;
        if (_active)
        {
            if (_time > _lifeSpan)
            {
                _time = 0;
                _active = false;
            }
        }
        else
        {
            if (_time < _fadeTime)
            {
                _renderer.material.color = new Color(_renderer.material.color.r, _renderer.material.color.g, _renderer.material.color.b, 1f - _time / _fadeTime);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        _time += Time.deltaTime;
    }
    

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions pa = other.GetComponentInParent<PlayerActions>();
        if (pa != null)
        {
            _player = pa;
            _cr = StartCoroutine(DamagePlayer());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerActions pa = other.GetComponentInParent<PlayerActions>();
        if (pa != null)
        {
            _player = null;
            StopCoroutine(_cr);
            _cr = null;
        }
    }

    private IEnumerator DamagePlayer()
    {
        while (!PlayerActions.dead)
        {
            if (_player != null)
            {
                _player.takeDamage(_damage);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

}
