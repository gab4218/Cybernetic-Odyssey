using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Lava : MonoBehaviour
{
    [SerializeField] private int _damage = 2;
    private PlayerActions _player;
    private Coroutine _cr;

    private void OnCollisionEnter(Collision collision)
    {
        PlayerActions pa = collision.collider.GetComponentInParent<PlayerActions>();
        if (pa != null)
        {
            _player = pa;
            _cr = StartCoroutine(DamagePlayer());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        PlayerActions pa = collision.collider.GetComponentInParent<PlayerActions>();
        if (pa != null)
        {
            _player = null;
            StopCoroutine(_cr);
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
