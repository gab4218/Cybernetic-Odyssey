using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemy;
    [SerializeField] Vector3 spawnArea;
    [SerializeField] int maxEnemyCount;
    [SerializeField] int spawnDelay;

    int enemyCount;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
