using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    //Comments were written in English because when I code, I like to think in English, as it's closer in nature to C# and (at least professionally) it's more common
    //[Traduccion] Los comentarios fueron escritos en Ingles porque, cuando hago codigo, me gusta pensar en Ingles, ya que es fundamentalmente mas parecido a C# y (al menos profesionalmente) es mas comun

    //State variables
    protected const int IDLE = 0;
    protected const int SEEKING = 1;
    protected const int ATTACKING = 2;
    protected const int STUNNED = 3;
    public int state;

    //Basic editor-modifiable variables an enemy may have
    [SerializeField] protected int maxHP;
    [SerializeField] protected float speed;
    [SerializeField] protected float detectionDistance = 15f;
    [SerializeField] protected float escapeDistance = 20f;
    [SerializeField] protected int damageType;
    [SerializeField] protected Vector3[] randomMovementDimensions;
    [SerializeField] protected float positionThreshold = 0.5f;

    [SerializeField] protected TMP_Text HPDisplay; //For debug purposes

    //Other common enemy variables
    public int currentHP;
    protected Rigidbody rb;
    
    //Player detection variables
    protected Transform playerTranform;
    protected PlayerActions player;
    protected Vector3 dir;
    


    protected virtual void Start()
    {
        //Preparations
        rb = GetComponentInChildren<Rigidbody>();
        currentHP = maxHP;
        playerTranform = FindObjectOfType<PlayerMovement>().transform;
        player = FindObjectOfType<PlayerActions>();
        state = IDLE;
    }

    protected virtual void detectPlayer()
    {
        if (Vector3.Distance(transform.position, playerTranform.position) <= detectionDistance && state == IDLE) //If player is within detection distance and is Idle
        {
            state = SEEKING; 
        }
        else if (state == SEEKING && Vector3.Distance(transform.position, playerTranform.position) >= escapeDistance) //If chasing player and player is outside escape radius
        {
            state = IDLE;
        }
    }

    //These are all named the same because I thought it would be neat to have overridable functions

    protected void findDirection() //If the findDirection function call takes no arguments, look at player position projected onto the xz plane
    {
        dir = playerTranform.position - transform.position;
        dir.y = 0;
        dir.Normalize();
    }

    protected void findDirection(Vector2 newSpot) //If the findDirection function call takes a Vector2 argument, look at position projected onto the xz plane
    {
        
        dir = new Vector3(newSpot.x, transform.position.y, newSpot.y) - transform.position;
        dir.Normalize();
    }

    protected void findDirection(Vector3 newSpot) //If the findDirection function call takes a Vector3 argument, look at position
    {
        dir = newSpot - transform.position;
        dir.Normalize();
    }

    protected bool hasReachedDestination(Vector2 targetPos) //If the hasReachedDestination function call takes a Vector2 argument, check if position is within a threshold of the xz plane projection of the Vector
    {
        
        return Vector3.Distance(transform.position, new Vector3(targetPos.x, transform.position.y, targetPos.y)) <= positionThreshold;
        
    }

    protected bool hasReachedDestination(Vector3 targetPos) // If the hasReachedDestination function call takes a Vector3 argument, check if position is within a threshold of the vector
    {
       return Vector3.Distance(transform.position, targetPos) <= positionThreshold;

    }

    protected virtual void move() //Move in the found direction
    {
        if (dir.sqrMagnitude > 0)
        {
            rb.velocity = dir * speed + rb.velocity.y * Vector3.up;
            transform.forward = Vector3.Lerp(transform.forward, -dir, 0.1f);
        }
    }

    public virtual void takeDamage(int dmg, int dmgColor) 
    {
        if (HPDisplay != null) //If can display HP, display HP
        {
            HPDisplay.text = $"Bear HP: {currentHP}/{maxHP}";

        }
        currentHP -= dmg * (dmgColor == damageType? 3 : 1); //Remove HP according to damage type received 
        if (currentHP <= 0) //If dead, destroy gameObject
        {
            Destroy(gameObject);
        }
    }

    protected void Stun(float stunTime) //Stun for a period of time
    {
        state = STUNNED;
        Invoke("Destun", stunTime);
    }

    private void Destun() //Used for Invoke
    {
        state = IDLE;
    }
}
