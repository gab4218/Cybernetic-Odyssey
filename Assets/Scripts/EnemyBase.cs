using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    protected const int IDLE = 0;
    protected const int SEEKING = 1;
    protected const int ATTACKING = 2;
    [SerializeField] protected int maxHP;
    [SerializeField] protected float speed;
    [SerializeField] protected float detectionDistance;
    [SerializeField] protected float blindDistance;
    [SerializeField] protected int damageType;
    [SerializeField] protected float stunTime;
    [SerializeField] protected Vector3[] randomMovementDimensions;
    [SerializeField] protected TMP_Text HPDisplay;
    [SerializeField] protected float positionThreshold = 0.5f;
    protected Transform playerTranform;
    protected PlayerActions player;
    public int currentHP;
    protected Vector3 dir;
    public int state;
    protected Rigidbody rb;



    protected virtual void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        currentHP = maxHP;
        player = FindObjectOfType<PlayerActions>();
        playerTranform = player.gameObject.transform;
        state = IDLE;
    }

    protected virtual void detectPlayer()
    {
        if (Vector3.Distance(transform.position, playerTranform.position) <= detectionDistance && state != ATTACKING)
        {
            state = SEEKING;
        }
        else if (state != IDLE && state != ATTACKING && Vector3.Distance(transform.position, playerTranform.position) >= blindDistance)
        {
            state = IDLE;
        }
    }

    protected void findDirection()
    {
        dir = playerTranform.position - transform.position;
        dir.y = 0;
        dir.Normalize();
    }

    protected void findDirection(Vector2 newSpot)
    {
        Vector3 newSpot3 = new Vector3(newSpot.x, transform.position.y, newSpot.y);
        dir = newSpot3 - transform.position;
        dir.Normalize();
    }

    protected void findDirection(Vector3 newSpot)
    {
        dir = newSpot - transform.position;
        dir.Normalize();
    }

    protected bool hasReachedDestination(Vector2 targetPos)
    {
        Vector3 targetPos3 = new Vector3(targetPos.x, transform.position.y, targetPos.y);
        return Vector3.Distance(transform.position, targetPos3) <= positionThreshold;
        
    }

    protected bool hasReachedDestination(Vector3 targetPos)
    {
       return Vector3.Distance(transform.position, targetPos) <= positionThreshold;

    }

    protected virtual void move()
    {
        if (dir.sqrMagnitude > 0)
        {
            rb.velocity = dir * speed;
            transform.forward = Vector3.Lerp(transform.forward, -dir, 0.1f);
        }
    }

    public virtual void takeDamage(int dmg, int dmgColor)
    {
        currentHP -= dmg * dmgColor == damageType? 3 : 1;
        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }

}
