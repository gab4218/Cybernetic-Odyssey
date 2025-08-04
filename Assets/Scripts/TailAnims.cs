using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailAnims : MonoBehaviour
{
    [SerializeField] private EnemyTail _tail;

    public void DoLunge()
    {
        _tail.DoLunge();
    }

    public void EndLunge()
    {
        _tail.EndLunge();
    }

    public void Burrow()
    {
        _tail.Burrow();
    }

    public void Unburrow()
    {
        _tail.ShootFireballs();
    }

    public void DoFling()
    {
        _tail.DoFling();
    }

    public void EndFling()
    {
        _tail.EndFling();
    }
}
