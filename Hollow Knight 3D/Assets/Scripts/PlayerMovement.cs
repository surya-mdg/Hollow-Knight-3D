using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;

//All temporary test code are under comment "_TEST_"

public class PlayerMovement : MonoBehaviour
{
    #region Pseudo-Public Variables
    [Header("References")]
    [SerializeField] private Rigidbody rb; //Rigidbody of player
    [SerializeField] private VisualEffect slashVFX;
    [SerializeField] private Transform orientation; //Used to calculate movement direction based on where camera is looking
    [SerializeField] private Transform groundDetectPoint; //Point at which the ground is detected
    [SerializeField] private Transform bodyModel;
    [SerializeField] private Transform bodyModelOffset;
    [SerializeField] private GameObject bodyDouble;
    [SerializeField] private GameObject handModel;
    [SerializeField] private GameManager gm;
    [SerializeField] private Animator playerBody;
    [SerializeField] private Animator playerHand;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 15f; //Walk speed of player on ground
    [SerializeField] private float reduceAirMovement = 8f; //Movement speed of player while in air
    [SerializeField] private float jumpForce = 15f; //Force applied to jump
    [SerializeField] private float jumpForceImpulse = 10f;
    [SerializeField] private float jumpTime = 0.4f;
    [SerializeField] private float counterForce = 10f; //Force applied to prevent slip for diagonal movement
    [SerializeField] private float groundDrag = 5f; //Drag applied while on ground
    [SerializeField] private float airDrag = 1f; //Drag applied while in air
    [SerializeField] private float jumpGravity = 12f; //Additional force applied to player while falling down in air
    [SerializeField] private float groundDetectRadius = 0.3f; //Radius of sphere used to detect ground
    [SerializeField] private LayerMask groundLayers; //Layers that are considered as walkable & jumpable

    [Header("Miscellaneous Settings")]
    [SerializeField] private float walkBendAngle = -4f;
    [SerializeField] private float attackCooldown = 0.5f; //Must be same as variable of same name in PlayerCombat.cs
    #endregion

    #region Private Variables
    private float horizontalAxis; //Horizontal Input
    private float verticalAxis; //Vertical Input
    private float jumpBuffer = 0f;
    private float attackBuffer = 0f;
    private float attackCooldownBuffer = 0f;
    private readonly float movementMultiplier = 10f;
    
    private bool isJumping = false;
    private bool onSlope = false;

    private Vector3 moveDirection;
    private Vector3 slopeMoveDirection;
    private Vector3 initialBodyPos;
    private RaycastHit slopeHit;
    #endregion

    [HideInInspector] public bool reviving = false;
    [HideInInspector] public bool isGrounded = false;

    private void Awake()
    {
        initialBodyPos = bodyModel.localPosition;
        jumpBuffer = jumpTime;
    }

    void Update()
    {
        AdjustDrag();
        CheckSlope();
        CounterForce();
        PlayerInput();
        SetAnimations();

        isGrounded = Physics.CheckSphere(groundDetectPoint.position, groundDetectRadius, groundLayers); //Detects ground

        if(Input.GetKeyDown(KeyCode.Mouse0) && !reviving && attackCooldownBuffer < 0 && !gm.paused)
        {
            attackCooldownBuffer = attackCooldown;
            handModel.SetActive(true);
            bodyDouble.SetActive(true);
            playerHand.Play("Base Layer.Slash", 0, 0);

            slashVFX.Play();
            
            bodyModel.localPosition = bodyModelOffset.localPosition;
            attackBuffer = 0.75f;
        }

        attackBuffer -= Time.deltaTime;
        attackCooldownBuffer -= Time.deltaTime;

        if (attackBuffer < 0)
        {
            handModel.SetActive(false);
            bodyDouble.SetActive(false);
            bodyModel.localPosition = initialBodyPos;
        }
    }

    private void FixedUpdate()
    {
        Walk();

        if (Input.GetKey(KeyCode.Space) && jumpBuffer > 0f && isJumping)
        {
            Jump();
            jumpBuffer -= Time.deltaTime;
        }
    }

    private void AdjustDrag() //Adjusts drag & applies additional gravity based on the conditions
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        } 
        else
            rb.drag = airDrag;

        if (rb.velocity.y < -0.1f)
            rb.AddForce(jumpGravity * Vector3.down, ForceMode.Acceleration);
    }

    private void CheckSlope() //Detects & adjusts move direction based on ground slope normal
    {
        if (Physics.Raycast(groundDetectPoint.position, Vector3.down, out slopeHit, 0.5f, groundLayers))
        {
            if (slopeHit.normal != Vector3.up)
            {
                onSlope = true;
                slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
            }
            else
            {
                onSlope = false;
            }
        }
    }

    private void CounterForce() //Applies counter force for snappy ground movement
    {
        Vector2 mag = FindVelRelativeToLook();

        if(Input.GetButton("Vertical") && !Input.GetButton("Horizontal"))
        {
            if (mag.x > 0.1f || mag.x < -0.1f)
            {
                rb.AddForce(-1 * mag.x * counterForce * orientation.right, ForceMode.Acceleration);
            }   
        }
        else if(Input.GetButton("Horizontal") && !Input.GetButton("Vertical"))
        {
            if(mag.y > 0.1f || mag.y < -0.1f)
            {
                rb.AddForce(-1 * mag.y * counterForce * orientation.forward, ForceMode.Acceleration);
            }
        }
        else if(!Input.GetButton("Horizontal") && isGrounded)
        {
            if (mag.x > 0.1f || mag.x < -0.1f)
            {
                rb.AddForce(-1 * mag.x * counterForce * orientation.right, ForceMode.Acceleration);
            }

            if (mag.y > 0.1f || mag.y < -0.1f)
            {
                rb.AddForce(-1 * mag.y * counterForce * orientation.forward, ForceMode.Acceleration);
            }
        }
    }

    public Vector2 FindVelRelativeToLook() //Returns magnitude of velocity in the X & Z axis with respect to the orientation
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float zMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, zMag);
    }

    private void Jump() //Used to apply jump force (in case you weren't able to figure it out)
    {
        rb.AddForce(jumpForce * Vector3.up, ForceMode.Acceleration);
    }

    private void PlayerInput() //All input code go here
    {
        if(reviving)
        {
            horizontalAxis = 0;
            verticalAxis = 0;
            return;
        }

        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isJumping = true;
            rb.AddForce(jumpForceImpulse * Vector3.up, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(KeyCode.Space) && isJumping)
        {
            if(rb.velocity.y > 0.1f)
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            isJumping = false;
            jumpBuffer = jumpTime;
        }   
    }

    private void SetAnimations()
    {
        if (rb.velocity.magnitude > 0.25f && isGrounded)
        {
            playerBody.SetBool("Running", true);
            playerBody.SetBool("Jumping", false);
            bodyModel.localRotation = Quaternion.Euler(walkBendAngle, 180, 0);
        }
        else if (!isGrounded)
        {
            bodyModel.localRotation = Quaternion.Euler(0, 180, 0);
            playerBody.SetBool("Running", false);
            playerBody.SetBool("Jumping", true);
        }
        else
        {
            bodyModel.localRotation = Quaternion.Euler(0, 180, 0);
            playerBody.SetBool("Running", false);
            playerBody.SetBool("Jumping", false);
        }
    }

    private void Walk() //Used to calculate direction & move player on the X & Z axis
    {
        moveDirection = orientation.forward * verticalAxis + orientation.right * horizontalAxis;

        if (isGrounded && !onSlope)
            rb.AddForce(walkSpeed * movementMultiplier * moveDirection.normalized, ForceMode.Acceleration);
        else if (isGrounded && onSlope)
            rb.AddForce(walkSpeed * movementMultiplier * slopeMoveDirection.normalized, ForceMode.Acceleration);
        else
            rb.AddForce(walkSpeed * (movementMultiplier / reduceAirMovement) * moveDirection.normalized, ForceMode.Acceleration);
    }
}
