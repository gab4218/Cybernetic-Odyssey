using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemy;
    [SerializeField] Vector3[] spawnArea;
    [SerializeField] int maxEnemyCount;
    [SerializeField] int spawnDelay;

    int enemyCount;
    List<GameObject> enemyList = new List<GameObject>();
    void Start()
    {
        enemyList.AddRange(GameObject.FindGameObjectsWithTag(enemy.tag));
        enemyCount = enemyList.Count;
    }

    
    void Update()
    {
        enemyList.RemoveAll(null);
        Debug.Log(enemyList.Count);
        if (enemyCount < maxEnemyCount)
        {
            Invoke("SpawnEnemy", spawnDelay);

        }
    }


    private void SpawnEnemy()
    {
        Vector3 spawnPoint = new Vector3(Random.Range(spawnArea[0].x, spawnArea[1].x), Random.Range(spawnArea[0].y, spawnArea[1].y), Random.Range(spawnArea[0].z, spawnArea[1].z));
    }

}
