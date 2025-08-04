using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SalamanderAnim : MonoBehaviour
{
    [SerializeField] EnemySalamander salamander;

    public void DoBite()
    {
        salamander.DoBite();
    }

    public void EndBite()
    {
        salamander.EndBite();
    }

    public void DoWhip()
    {
        salamander.DoWhip();
    }

    public void EndWhip()
    {
        salamander.EndWhip();
    }

  
}
