 using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Physics")]
    //[SerializeField] private float airSpeed = 20f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airDrag  = 1f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float accelerationForce = 2000f;
    [SerializeField] private float sprintMult = 1.5f;
    [SerializeField] private float maxStamina = 5f;
    [SerializeField] private float staminaTimer = 1f;
    [SerializeField] private float crouchMult = 0.5f;
    [SerializeField] private float groundRayLen = 0.2f;
    [SerializeField] private float slideImpulse = 2f;
    [SerializeField] private float slideTime = 2f;
    [SerializeField] private LayerMask groundRayLayerMask = 30;
    

    [Header("UI")]
    [SerializeField] private RectTransform staminaImg;


    private float originalSpeed, xDir, zDir, accelMult, currentStamina;

    private Ray groundRay;

    private Vector3 dir = Vector3.zero;

    private bool isCrouching = false, isSprinting = false, resting = false, isSliding = false, forceStopSlide = true;

    private Rigidbody rb;

    private Vector3 posOffset;

    


    private void Awake()
    {
        //Preparations and storing original maxSpeed

        currentStamina = maxStamina;

        rb = GetComponent<Rigidbody>();
        
        originalSpeed = maxSpeed;

        accelMult = 1;

        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
    }

    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        */

        //Get player input
        xDir = Input.GetAxisRaw("Horizontal");
        zDir = Input.GetAxisRaw("Vertical");

        if (!isSliding)
        {
            dir = transform.right * xDir + transform.forward * zDir;
        }
        
        //Create ground-detecting ray
        groundRay = new Ray(-transform.up, transform.position);
        //Change drag according to whether the player is on the ground or in the air
        if (groundDetect())
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }

        //Check if there's any modification to the player's movement state and correct speed
        checkSprint();

        checkCrouch();
        
        if (Input.GetKeyDown(jumpKey) && groundDetect())
        {
            Jump();
        }


        checkSpeed();

        //This handles stamina
        if (isSprinting)
        {
            currentStamina -= Time.deltaTime;

        }
        else if(resting && currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(currentStamina + Time.deltaTime * 2, maxStamina);
        }
        staminaImg.sizeDelta = new Vector2(currentStamina * 80, staminaImg.sizeDelta.y);
    }

    private void FixedUpdate()
    {
        //If player is providing movement input, move
        if (dir.sqrMagnitude != 0)
        {
            Movement(dir);
            
        }
    }

  
    private bool groundDetect()
    {
        //If groundRay hits the ground, return true, otherwise return false
        posOffset = new Vector3(transform.position.x,transform.position.y + groundRayLen / 3.33f, transform.position.z);
        groundRay = new Ray(posOffset, -transform.up);
        return Physics.Raycast(groundRay, groundRayLen, groundRayLayerMask);
    }

    private void Movement(Vector3 dir)
    {
        
        rb.AddForce(dir.normalized * accelerationForce * Time.fixedDeltaTime * accelMult, ForceMode.Force);
        
    }



    private void checkSpeed()
    {
        //Isolate horizontal components of velocity
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        
        
        //Check if horizontal velocity exceeds speed limit
        if (flatVelocity.magnitude > maxSpeed)
        {
            //Correct velocity to limit
            flatVelocity = flatVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(flatVelocity.x, rb.velocity.y, flatVelocity.z);
        }
    }

    private void checkCrouch()
    {
        if (Input.GetKeyDown(crouchKey) && !isCrouching && groundDetect())
        {
            //If player crouches, stop sprinting and start crouching

            if (isSprinting && !isSliding)
            {
                isSliding = true;
                isSprinting = false;
                rb.AddForce(dir * slideImpulse, ForceMode.Impulse);
                rb.drag = 0;
                maxSpeed *= 3;
                isCrouching = true;
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);
                StartCoroutine(slideTimer());
            }
            else
            {
                isSprinting = false;
                isCrouching = true;
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);
                maxSpeed = originalSpeed * crouchMult;
            }

        }
        else if ((Input.GetKeyUp(crouchKey) && isCrouching) || !forceStopSlide)
        {
            //If player stops crouching, go back to default state
            StopCoroutine(slideTimer());
            maxSpeed = originalSpeed;
            isCrouching = false;
            isSliding = false;
            forceStopSlide = true;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 2, transform.localScale.z);
        }
    }

    private void checkSprint()
    {
        if (Input.GetKey(sprintKey) && !isSprinting && !isCrouching && zDir > 0 && currentStamina > 0)
        {
            //If player sprints while moving forward and is not crouching, make them go faster
            maxSpeed *= sprintMult;
            accelMult = sprintMult;
            isSprinting = true;
            resting = false;
            StopCoroutine(rechargeStamina());
        }
        else if ((Input.GetKeyUp(sprintKey) || zDir < 1 || currentStamina <= 0) && isSprinting)
        {
            //If player stops sprinting, go back to default state
            maxSpeed = originalSpeed;
            isSprinting = false;
            accelMult = 1;
            StartCoroutine(rechargeStamina());
        }
    }

    private void Jump()
    {
        //Halt all vertical movement for consistency and apply an impulse force upwards
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }


    private IEnumerator slideTimer()
    {
        float currentTime = 0;
        while (currentTime < slideTime)
        {
            currentTime += Time.deltaTime;
            yield return null;

        }
        if (isSliding)
        {
            maxSpeed = originalSpeed;
            forceStopSlide = false;
        }
        yield break;
    }

    private IEnumerator rechargeStamina()
    {

        yield return new WaitForSeconds(staminaTimer);

        if (!isSprinting)
        {
            resting = true;
        }
        else
        {
            resting = false;
        }
        yield break;
    }

}
