using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemy;
    [SerializeField] Vector3[] spawnArea;
    [SerializeField] int maxEnemyCount;
    [SerializeField] int spawnDelay;

    public int enemyCount = 0;
    
    void Start()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag(enemy.tag))
        {
            enemyCount++;
            enemy.GetComponent<EnemyBase>().enemySpawner = this;
        }
        
    }

    
    void Update()
    {
        if (enemyCount < maxEnemyCount)
        {
            Invoke("SpawnEnemy", spawnDelay);
            enemyCount++;
        }
    }


    private void SpawnEnemy()
    {
        Vector3 spawnPoint = new Vector3(Random.Range(spawnArea[0].x, spawnArea[1].x), Random.Range(spawnArea[0].y, spawnArea[1].y), Random.Range(spawnArea[0].z, spawnArea[1].z));
        GameObject go = Instantiate(enemy, spawnPoint, Quaternion.identity);
        go.GetComponent<EnemyBase>().enemySpawner = this;
    }

}
