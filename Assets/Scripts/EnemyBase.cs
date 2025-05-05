using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
 

    //Variables de estado
    protected const int IDLE = 0;
    protected const int SEEKING = 1;
    protected const int ATTACKING = 2;
    protected const int STUNNED = 3;
    public int state;

    //Variables basicas modificables en el editor que un enemigo podria tener
    [SerializeField] protected int maxHP;
    [SerializeField] protected float speed;
    [SerializeField] protected float detectionDistance = 15f;
    [SerializeField] protected float escapeDistance = 20f;
    [SerializeField] protected int damageType;
    [SerializeField] protected Vector3[] randomMovementDimensions;
    [SerializeField] protected float positionThreshold = 0.5f;

    [SerializeField] protected TMP_Text HPDisplay; //Para debug

    //Otras variables comunes de enemigo
    public int currentHP;
    public EnemySpawner enemySpawner;
    protected Rigidbody rb;
    
    //Variables para deteccion de jugador
    protected Transform playerTranform;
    protected PlayerActions player;
    protected Vector3 dir;
    


    protected virtual void Start()
    {
        //Preparaciones
        rb = GetComponentInChildren<Rigidbody>();
        currentHP = maxHP;
        playerTranform = FindObjectOfType<PlayerMovement>().transform;
        player = FindObjectOfType<PlayerActions>();
        state = IDLE;
    }

    private void OnDestroy()
    {
        enemySpawner.enemyCount--;
    }
    protected virtual void detectPlayer() //Detectar jugador
    {
        if (Vector3.Distance(transform.position, playerTranform.position) <= detectionDistance && state == IDLE) //Si el jugador esta dentro del radio de deteccion y estado = idle, cambiar a buscar
        {
            state = SEEKING; 
        }
        else if (state == SEEKING && Vector3.Distance(transform.position, playerTranform.position) >= escapeDistance) //Si el jugador esta fuera del radio de escape y estado = buscar, cambiar a idle
        {
            state = IDLE;
        }
    }

    //Las siguientes funciones fueron nombradas iguales porque me gustan las funciones con multiples overrides

    protected void findDirection() //Si la llamada de la funcion no toma argumentos, mirar a la proyeccion de la posicion del jugador en el plano xz
    {
        dir = playerTranform.position - transform.position;
        dir.y = 0;
        dir.Normalize();
    }

    protected void findDirection(Vector2 newSpot) //Si la llamada de la funcion toma un Vector2, mirar a la proyeccion de la posicion pasada en el plano xz
    {
        
        dir = new Vector3(newSpot.x, transform.position.y, newSpot.y) - transform.position;
        dir.Normalize();
    }

    protected void findDirection(Vector3 newSpot) //Si la llamada de la funcion toma un Vector3, mirar a la posicion pasada
    {
        dir = newSpot - transform.position;
        dir.Normalize();
    }

    protected bool hasReachedDestination(Vector2 targetPos) //Si la llamada de la funcion toma un Vector2, chequear si la posicion esta dentro de una tolerancia de la proyeccion en el plano xz del vector
    {
        
        return Vector3.Distance(transform.position, new Vector3(targetPos.x, transform.position.y, targetPos.y)) <= positionThreshold;
        
    }

    protected bool hasReachedDestination(Vector3 targetPos) //Si la llamada de la funcion toma un Vector3, chequear si la posicion esta dentro de una tolerancia del vector
    {
       return Vector3.Distance(transform.position, targetPos) <= positionThreshold;

    }

    protected virtual void move(bool inverted) //Mover en la direccion encontrada
    {
        if (dir.sqrMagnitude > 0)
        {
            rb.velocity = dir * speed + rb.velocity.y * Vector3.up;
            transform.forward = Vector3.Lerp(transform.forward, (inverted ? -dir : dir), 0.1f);
        }
    }

    public virtual void takeDamage(int dmg, int dmgColor) 
    {
        if (HPDisplay != null) //Si se puede mostrar HP, mostrarla
        {
            HPDisplay.text = $"Bear HP: {currentHP}/{maxHP}";

        }
        currentHP -= dmg * (dmgColor == damageType? 3 : 1); //Restar HP acorde al tipo de damage recibido
        if (currentHP <= 0) //Si muerto, destruir
        {
            Destroy(gameObject);
        }
    }

    protected void Stun(float stunTime) //Stunnear por un periodo de tiempo
    {
        state = STUNNED;
        Invoke("Destun", stunTime);
    }

    private void Destun() //Usado para Invoke
    {
        state = IDLE;
    }
}
