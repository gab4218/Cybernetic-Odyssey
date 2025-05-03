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
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airDrag  = 0.5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float accelerationForce = 2000f;
    [SerializeField] private float sprintMult = 1.5f;
    [SerializeField] private float maxStamina = 5f;
    [SerializeField] private float staminaTimer = 1f;
    [SerializeField] private float crouchMult = 0.5f;
    [SerializeField] private float groundRayLen = 0.2f;
    [SerializeField] private float slideImpulse = 2f;
    [SerializeField] private float slideTime = 2f;
    [SerializeField] private float grapplingForce = 3000f;
    [SerializeField] private LayerMask groundRayLayerMask = 30;

    

    [Header("UI")]
    [SerializeField] private RectTransform staminaImg;
    [SerializeField] private Camera[] cams;
    [SerializeField] private float FOV_Sprint, FOV_Crouch;

    [SerializeField] private LineRenderer grappleLinePF;
    [SerializeField] private Transform grapplePoint;

    private float targetFOV;


    private float originalSpeed, xDir, zDir, accelMult, currentStamina, defaultFOV, targetSpeed, fac = 0;

    private LineRenderer grappleLine;

    private Ray groundRay, wallRay;

    private Vector3 dir = Vector3.zero;
    Vector3 flatVelocity;
    private bool isCrouching = false, isSprinting = false, resting = false, isSliding = false, forceStopSlide = true, canWalljump = false, walljumping = false, isGrappling = false, grounded = true;

    private Rigidbody rb;

    private Vector3 posOffset, grapplePosition;

    


    private void Awake()
    {
        //Preparations and storing original maxSpeed
        defaultFOV = Camera.main.fieldOfView;
        currentStamina = maxStamina;
        targetFOV = defaultFOV;
        rb = GetComponent<Rigidbody>();
        
        originalSpeed = maxSpeed;

        accelMult = 1;

        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;
        foreach(var cam in cams)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, 2 * Time.deltaTime);
        }
        //Get player input

        if (isGrappling)
        {
            dir = grapplePosition - transform.position;
            grappleLine.SetPosition(0, grapplePoint.position);
            if (Vector3.Distance(transform.position, grapplePosition) < 3)
            {
                isGrappling = false;
                Destroy(grappleLine.gameObject);
            }
        }
        else
        {
            xDir = Input.GetAxisRaw("Horizontal");
            zDir = Input.GetAxisRaw("Vertical");

            if (!isSliding)
            {
                dir = transform.right * xDir + transform.forward * zDir;
            }
        
            //Create ground-detecting ray
            groundRay = new Ray(-transform.up, transform.position);
            //Change drag according to whether the player is on the ground or in the air
            if (grounded)
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
        
            if (Input.GetKeyDown(jumpKey) && grounded)
            {
                Jump();
            }
            if (Input.GetKeyDown(jumpKey) && WallDetect() && currentStamina >= 0.5f) WallJump();


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

    }

    private void FixedUpdate()
    {
        //If player is providing movement input, move
        if (dir.sqrMagnitude != 0 && !isGrappling)
        {
            Movement(dir);
            
        }
        else if(isGrappling)
        {
            rb.AddForce(dir.normalized * grapplingForce * Time.fixedDeltaTime, ForceMode.Force);
        }
    }

  
    private void groundDetect()
    {
        //If groundRay hits the ground, return true, otherwise return false
        posOffset = new Vector3(transform.position.x,transform.position.y + groundRayLen / 3.33f, transform.position.z);
        groundRay = new Ray(posOffset, -transform.up);
        grounded = Physics.Raycast(groundRay, groundRayLen, groundRayLayerMask);
        if (grounded && !isSprinting && walljumping)
        {
            walljumping = false;
            StartCoroutine(RechargeStamina());
        }
        
    }

    private bool WallDetect()
    {
        
        wallRay = new Ray(transform.position + Vector3.up, dir.normalized);


        if (!grounded && canWalljump)
        {
            return Physics.Raycast(wallRay, groundRayLen*3);
        }
        else
        {
            return false;
        }
    }

    private void Movement(Vector3 dir)
    {
        
        rb.AddForce(dir.normalized * accelerationForce * Time.fixedDeltaTime * accelMult, ForceMode.Force);
        
    }



    private void checkSpeed()
    {
        //Isolate horizontal components of velocity
        flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        
        
        //Check if horizontal velocity exceeds speed limit
        if (flatVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            //Correct velocity to limit
            flatVelocity = flatVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(flatVelocity.x, rb.velocity.y, flatVelocity.z);
        }
    }

    private void checkCrouch()
    {
        if (Input.GetKeyDown(crouchKey) && !isCrouching && grounded)
        {
            //If player crouches, stop sprinting and start crouching

            StopCoroutine(SmoothSpeed());
            fac = 0;
            
            if (isSprinting && !isSliding)
            {
                isSliding = true;
                isSprinting = false;
                rb.AddForce(dir * slideImpulse, ForceMode.Impulse);
                rb.drag = 0;
                maxSpeed *= 3;
                targetSpeed = maxSpeed;
                isCrouching = true;
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);
                StartCoroutine(slideTimer());
            }
            else
            {
                isSprinting = false;
                isCrouching = true;
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);
                targetSpeed = originalSpeed * crouchMult;
                StartCoroutine(restartSmoothSpeed());
                targetFOV = FOV_Crouch;
            }

        }
        else if ((Input.GetKeyUp(crouchKey) && isCrouching) || !forceStopSlide)
        {
            //If player stops crouching, go back to default state

            StopCoroutine(slideTimer());
            fac = 0;
            StopCoroutine(SmoothSpeed());
            targetSpeed = originalSpeed;
            StartCoroutine(restartSmoothSpeed());
            isCrouching = false;
            isSliding = false;
            targetFOV = defaultFOV;
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
            StopCoroutine(SmoothSpeed());
            fac = 0;
            targetSpeed = sprintMult * originalSpeed;
            StopCoroutine(RechargeStamina());
            targetFOV = FOV_Sprint;
        }
        else if ((Input.GetKeyUp(sprintKey) || zDir < 1 || currentStamina <= 0) && isSprinting)
        {
            //If player stops sprinting, go back to default state
            StopCoroutine(SmoothSpeed());
            fac = 0;
            targetSpeed = originalSpeed;
            StartCoroutine(restartSmoothSpeed());
            isSprinting = false;
            accelMult = 1;
            StartCoroutine(RechargeStamina());
            targetFOV = defaultFOV;
        }
    }

  
    private void Jump()
    {
        //Halt all vertical movement for consistency and apply an impulse force upwards
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void WallJump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce((transform.up - 2 * dir).normalized * jumpForce * 3, ForceMode.Impulse);
        currentStamina -= 0.5f;
        resting = false;
        walljumping = true;
    }

    public void ChangeWalljump(bool newWalljump)
    {
        canWalljump = newWalljump;
    }

    public void GrappleTo(Vector3 grapplePos)
    {
        isGrappling = true;
        grapplePosition = grapplePos;
        grappleLine = Instantiate(grappleLinePF, transform.position, Quaternion.identity);
        grappleLine.SetPositions(new Vector3[] { grapplePoint.position, grapplePosition });
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
            StopCoroutine(SmoothSpeed());
            targetSpeed = originalSpeed;
            fac = 0;
            StartCoroutine(restartSmoothSpeed());
            forceStopSlide = false;
        }
        yield break;
    }

    private IEnumerator RechargeStamina()
    {
        float timer = 0;
        while (timer < staminaTimer)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!isSprinting && !walljumping)
        {
            resting = true;
        }
        else
        {
            resting = false;
        }
        yield break;
    }

    private IEnumerator SmoothSpeed()
    {
        fac = 0;
        while (fac < 2)
        {
            fac += Time.deltaTime;
            maxSpeed = Mathf.Lerp(maxSpeed, targetSpeed, Mathf.Min(fac/2, 1));
            
            yield return null;
        }
        maxSpeed = targetSpeed;
        yield break;
        
    }
    private IEnumerator restartSmoothSpeed()
    {
        yield return null;
        StartCoroutine(SmoothSpeed());
        yield break;
    }

}
