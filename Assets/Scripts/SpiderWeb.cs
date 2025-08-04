using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] _renderers;
    [SerializeField] private float _lifeSpan = 10f;
    [SerializeField] private float _slowFactor = 2f;
    [SerializeField] private float _fadeTime = 3f;
    private float _time = 0;
    private bool _active = true;


    void Update()
    {
        if (Pause.paused) return;
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
                foreach (MeshRenderer _renderer in _renderers)
                {
                    _renderer.material.color = new Color(_renderer.material.color.r, _renderer.material.color.g, _renderer.material.color.b, 1f - _time / _fadeTime);
                }
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
            pa.Slow(_slowFactor);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerActions pa = other.GetComponentInParent<PlayerActions>();
        if (pa != null)
        {
            pa.RegularSpeed(_slowFactor);
        }
    }

}
