using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnims : MonoBehaviour
{
    [SerializeField] EnemySpider _spider;

    public void DoBite()
    {
        _spider.DoBite();
    }

    public void EndBite()
    {
        _spider.EndBite();
    }

    public void DoGrab()
    {
        _spider.DoGrab();
    }

    public void ShootAcid()
    {
        _spider.ShootAcid();
    }

    public void EndAcid()
    {
        _spider.EndAcid();
    }

    public void ShootSilk()
    {
        _spider.ShootSilk();
        
    }

    public void EndSilk()
    {
        _spider.EndSilk();
    }

    public void SpawnSpiders()
    {
        _spider.SpawnSpiders();
    }

    public void EndSpawn()
    {
        _spider.EndSpawn();
    }
    

}
