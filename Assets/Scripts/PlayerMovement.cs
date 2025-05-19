 using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{


    
    private Animator anim;
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
    [SerializeField] private float maxSlopeAngle = 45f;

    

    [Header("UI")]
    [SerializeField] private Image staminaImg;
    [SerializeField] private Camera[] cams;
    [SerializeField] private float FOV_Sprint, FOV_Crouch;

    [SerializeField] private LineRenderer grappleLinePF;
    [SerializeField] private Transform grapplePoint;

    private float targetFOV;

    public bool isCrouching = false, allowedToSlide = false;
    private float originalSpeed, xDir, zDir, accelMult, currentStamina, defaultFOV, targetSpeed, fac = 0;

    private LineRenderer grappleLine;

    private Ray groundRay, wallRay;

    private Vector3 dir = Vector3.zero;
    Vector3 flatVelocity;
    private bool isSprinting = false, resting = false, isSliding = false, forceStopSlide = true, canWalljump = false, walljumping = false, isGrappling = false, grounded = true, forceStopGrapple = false, onSlope = false, jumping = false;

    private Rigidbody rb;

    private Vector3 posOffset, grapplePosition;

    private RaycastHit slopeHit;



    


    private void Awake()
    {
        //Preparaciones
        defaultFOV = Camera.main.fieldOfView;
        currentStamina = maxStamina;
        targetFOV = defaultFOV;
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        originalSpeed = maxSpeed;

        accelMult = 1;

        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
    }

    private void Update()
    {
        if (Time.timeScale == 0) return; //Si esta pausado
        foreach (var cam in cams)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, 1 - Mathf.Pow(0.5f, Time.deltaTime * 8));
        }

        groundDetect();

        if (isGrappling) //Si esta grappling, moverse al punto de grapple e ignorar input
        {
            dir = grapplePosition - transform.position;
            grappleLine.SetPosition(0, grapplePoint.position);
            if (Vector3.Distance(transform.position, grapplePosition) < 3 || forceStopGrapple)
            {
                isGrappling = false;
                forceStopGrapple = false;
                Destroy(grappleLine.gameObject);
            }
        }
        else //Sino, moverse con normalidad
        {
            xDir = Input.GetAxisRaw("Horizontal");
            zDir = Input.GetAxisRaw("Vertical");
            anim.SetFloat("xDir", Input.GetAxis("Horizontal"));
            anim.SetFloat("zDir", Input.GetAxis("Vertical"));
            //Direccion

            if (!isSliding) //Si no esta sliding, actualizar dir
            {
                dir = transform.right * xDir + transform.forward * zDir; 
            }
        
            //Crear rayo para detectar suelo
            groundRay = new Ray(-transform.up, transform.position);
            //Cambiar drag acorde a si esta en el suelo o en el aire
            if (grounded)
            {
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = airDrag;
            }

            //Chequear si hay modificaciones al movimiento
            checkSprint();

            checkCrouch();

            SlopeDetect();

            rb.useGravity = !onSlope;


            if (Input.GetKeyDown(jumpKey) && grounded) //Salto
            {
                Jump();
            }
            if (Input.GetKeyDown(jumpKey) && WallDetect() && currentStamina >= 0.5f) WallJump();


            checkSpeed();
            anim.SetBool("Sprinting", isSprinting);
            anim.SetBool("Sliding", isSliding);
            anim.SetBool("Crouching", isCrouching);

            //Stamina
            if (isSprinting)
            {
                currentStamina -= Time.deltaTime;

            }
            else if(resting && currentStamina < maxStamina)
            {
                currentStamina = Mathf.Min(currentStamina + Time.deltaTime * 2, maxStamina);
            }
            staminaImg.fillAmount = currentStamina/maxStamina;

        }

    }

    private void FixedUpdate()
    {
        //Si el jugador se esta moviendo, moverlo
        if (dir.sqrMagnitude != 0 && !isGrappling)
        {
            Movement(dir);
            
        }
        else if(isGrappling) //Si esta grappling, moverlo para el punto de grapple
        {
            rb.AddForce(dir.normalized * grapplingForce * Time.fixedDeltaTime, ForceMode.Force);
        }
    }

  
    private void groundDetect()
    {
        //Actualizar grounded acorde al raycast
        posOffset = new Vector3(transform.position.x,transform.position.y + groundRayLen / 3.33f, transform.position.z);
        groundRay = new Ray(posOffset, -transform.up);
        grounded = Physics.Raycast(groundRay, groundRayLen, groundRayLayerMask);
        anim.SetBool("Grounded", grounded);
        
        if (grounded && !isSprinting && walljumping)
        {
            walljumping = false;
            StartCoroutine(RechargeStamina());
        }
        
    }

    public void FailGrapple(Vector3 pos)
    {
        grapplePosition = pos;
        grappleLine = Instantiate(grappleLinePF, transform.position, Quaternion.identity);
        StartCoroutine(GrappleFail());
    }
    private void SlopeDetect()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.3f, groundRayLayerMask))
        {

            slopeHit = hit;
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            onSlope = angle != 0 && angle < maxSlopeAngle;
        }
        else
        {
            onSlope = false;

        }
    }

    private Vector3 SlopeMoveDir()
    {
        return Vector3.ProjectOnPlane(dir, slopeHit.normal).normalized;
    }

    private bool WallDetect() //Detectar paredes para walljump
    {
        
        wallRay = new Ray(transform.position + Vector3.up, dir);


        if (!grounded && canWalljump)
        {
            return Physics.Raycast(wallRay, groundRayLen*3);
        }
        else
        {
            return false;
        }
    }

    private void Movement(Vector3 dir) //Mover
    {

        if (onSlope)
        {
            
            rb.AddForce(SlopeMoveDir() * accelerationForce * Time.fixedDeltaTime * accelMult, ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 10f, ForceMode.Force);
            }
        }
        else
        {
            rb.AddForce(dir * accelerationForce * Time.fixedDeltaTime * accelMult, ForceMode.Force);
        }

    }



    private void checkSpeed()
    {
        if (onSlope && !jumping)
        {
            if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }
        else
        {
            //Aislar componentes x & z de la velocidad
            flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        
        
            //Chequear si la velocidad horizontal excede un limite y corregir
            if (flatVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            {
                flatVelocity = flatVelocity.normalized * maxSpeed;
                rb.velocity = new Vector3(flatVelocity.x, rb.velocity.y, flatVelocity.z);
            }
            
        }
        
    }

    private void checkCrouch()
    {
        if (Input.GetKeyDown(crouchKey) && !isCrouching && grounded)
        {
            //Si el jugador se agacha, frenar Sprint y Slidear si es necesario

            StopCoroutine(SmoothSpeed());
            fac = 0;
            
            if (isSprinting && !isSliding && allowedToSlide)
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
<<<<<<< Updated upstream
                StartCoroutine(RestartSmoothSpeed());
=======
                if (slideCR != null)
                {
                    StopCoroutine(slideCR);
                }
                smoothSpeedCR = StartCoroutine(SmoothSpeed());
>>>>>>> Stashed changes
            }

        }
        else if ((Input.GetKeyUp(crouchKey) && isCrouching) || !forceStopSlide) //Si termina el slide o se deja de agachar, volver a Default
        {
<<<<<<< Updated upstream
            
            StopCoroutine(slideTimer());
=======

            if (slideCR != null)
            {
                StopCoroutine(slideCR);
            }
>>>>>>> Stashed changes
            fac = 0;
            StopCoroutine(SmoothSpeed());
            targetSpeed = originalSpeed;
            StartCoroutine(RestartSmoothSpeed());
            isCrouching = false;
            isSliding = false;
            targetFOV = defaultFOV;
            forceStopSlide = true;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 2, transform.localScale.z);
        }
    }

    private void checkSprint()
    {
        if (Input.GetKey(sprintKey) && !isSprinting && !isCrouching && zDir > 0 && currentStamina > 0 && !isSliding) //Si el jugador Sprintea, se esta moviendo para adelante y no esta agachado, sprintear
        {
            accelMult = sprintMult;
            isSprinting = true;
            resting = false;
<<<<<<< Updated upstream
            StopCoroutine(SmoothSpeed());
            fac = 0;
            targetSpeed = sprintMult * originalSpeed;
            StopCoroutine(RechargeStamina());
=======
            if (smoothSpeedCR != null)
            {
                StopCoroutine(smoothSpeedCR);
            }
            targetSpeed = sprintMult * originalSpeed; 
            fac = 0;
            smoothSpeedCR = StartCoroutine(SmoothSpeed());
            if (staminaCR != null)
            {
                StopCoroutine(staminaCR);
            }
>>>>>>> Stashed changes
            targetFOV = FOV_Sprint;
        }
        else if ((Input.GetKeyUp(sprintKey) || zDir < 1 || currentStamina <= 0) && isSprinting) //Si el jugador deja de sprintear o se le acaba la Stamina, dejar de sprintear
        {
            StopCoroutine(SmoothSpeed());
            fac = 0;
            targetSpeed = originalSpeed;
            StartCoroutine(RestartSmoothSpeed());
            isSprinting = false;
            accelMult = 1;
            StartCoroutine(RechargeStamina());
            targetFOV = defaultFOV;
        }
    }

  
    private void Jump()
    {
        //Frenar todo movimiento vertical y aplicar fuerza de salto para que sea consistente
        jumping = true;
        Invoke("StopJumping", 0.5f);
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        anim.SetTrigger("Jump");
    }

    private void StopJumping()
    {
        jumping = false;
    }

    private void WallJump()
    {
        //Igual que salto pero en direccion opuesta a la pared
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce((transform.up - 2 * dir).normalized * jumpForce * 3, ForceMode.Impulse);
        currentStamina -= 0.5f;
        resting = false;
        walljumping = true;
        anim.SetTrigger("Jump");
    }

    public void ChangeWalljump(bool newWalljump) //Habilitar o deshabilitar wall jump
    {
        canWalljump = newWalljump;
    }

    public void GrappleTo(Vector3 grapplePos) //Grappling
    {
        isGrappling = true;
        grapplePosition = grapplePos;
        grappleLine = Instantiate(grappleLinePF, transform.position, Quaternion.identity);
        grappleLine.SetPositions(new Vector3[] { grapplePoint.position, grapplePosition });
    }
    public bool GetGrappleState()
    {
        return isGrappling;
    }

    public void StopGrapple()
    {
        forceStopGrapple = true;
    }
   
    private IEnumerator slideTimer() //Limitador de tiempo de Slide
    {
        float currentTime = 0;
        while (currentTime < slideTime)
        {
            currentTime += Time.deltaTime;
            yield return null;

        }
        if (isSliding)
        {
<<<<<<< Updated upstream
            StopCoroutine(SmoothSpeed());
=======
            if (smoothSpeedCR != null)
            {
                StopCoroutine(smoothSpeedCR);
            }
>>>>>>> Stashed changes
            targetSpeed = originalSpeed;
            fac = 0;
            StartCoroutine(RestartSmoothSpeed());
            forceStopSlide = false;
        }
        
    }

    private IEnumerator RechargeStamina() //Recargar stamina
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
        
    }

    private IEnumerator SmoothSpeed() //Cambiar la velocidad de forma suave
    {
        fac = 0;
        while (fac < 1)
        {
            fac += Time.deltaTime;
            maxSpeed = Mathf.Lerp(maxSpeed, targetSpeed, Mathf.Min(fac, 1));
            
            yield return null;
        }
        maxSpeed = targetSpeed;
        
        
    }
    private IEnumerator RestartSmoothSpeed() //Frenar cambio de velocidad
    {
        yield return null;
        StartCoroutine(SmoothSpeed());
        
    }

    private IEnumerator GrappleFail()
    {
        float blend = 0;
        while (blend < 1)
        {
            grappleLine.SetPosition(0, grapplePoint.position);
            grappleLine.SetPosition(1, Vector3.Lerp(grapplePoint.position, grapplePosition, blend));
            blend += Time.deltaTime * 5;
            yield return null;
        }
        grappleLine.SetPosition(1, grapplePosition);
        blend = 1;
        while (blend > 0)
        {
            grappleLine.SetPosition(0, grapplePoint.position);
            grappleLine.SetPosition(1, Vector3.Lerp(grapplePoint.position, grapplePosition, blend));
            blend -= Time.deltaTime * 7;
            yield return null;
        }
        Destroy(grappleLine.gameObject);
    }
}
