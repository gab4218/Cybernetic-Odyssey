using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearUpperCollisionDetection : MonoBehaviour
{
    // Usado por PolarBear para deteccion de colisiones en la zona superior
    private PolarBear pb;

    void Start()
    {
        pb = GetComponentInParent<PolarBear>();
    }

    private void OnTriggerEnter(Collider other) //Si se detecta una colision en el trigger de arriba y la colision es con el jugador, el oso hace Ball
    {
        PlayerActions pAct = other.GetComponentInParent<PlayerActions>();
        if (pAct!=null)
        {
            pb.StartBall();
        }
    }
}
