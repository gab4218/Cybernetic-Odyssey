using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilkBall : MonoBehaviour
{
    [SerializeField] private SpiderWeb _web;

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(_web, transform.position, Quaternion.LookRotation(Random.onUnitSphere));
        Destroy(gameObject);
    }
}
