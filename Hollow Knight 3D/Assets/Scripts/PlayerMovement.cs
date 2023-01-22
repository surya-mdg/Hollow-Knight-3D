using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//All temporary test code are under comment "_TEST_"

public class PlayerMovement : MonoBehaviour
{
    #region Pseudo-Public Variables
    [Header("References")]
    [SerializeField] private Rigidbody rb; //Rigidbody of player
    [SerializeField] private Transform orientation; //Used to calculate movement direction based on where camera is looking
    [SerializeField] private Transform groundDetectPoint; //Point at which the ground is detected

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 15f; //Walk speed of player on ground
    [SerializeField] private float reduceAirMovement = 8f; //Movement speed of player while in air
    [SerializeField] private float jumpForce = 15f; //Force applied to jump
    [SerializeField] private float groundDrag = 5f; //Drag applied while on ground
    [SerializeField] private float counterdDrag = 15f; //Counter drag applied while on ground to prevent slip
    [SerializeField] private float airDrag = 1f; //Drag applied while in air
    [SerializeField] private float jumpGravity = 12f; //Additional force applied to player while falling down in air
    [SerializeField] private float groundDetectRadius = 0.3f; //Radius of sphere used to detect ground
    [SerializeField] private LayerMask groundLayers; //Layers that are considered as walkable & jumpable
    #endregion

    #region Private Variables
    private float horizontalAxis; //Horizontal Input
    private float verticalAxis; //Vertical Input
    private readonly float movementMultiplier = 10f;
    private bool isGrounded = false;
    private bool onSlope = false;

    private Vector3 moveDirection;
    private Vector3 slopeMoveDirection;
    private RaycastHit slopeHit;
    #endregion

    public float counterMovement = 0.175f;
    public float threshold = 0.01f;
    public float maxSpeed = 20;
    float x, y;

    void Update()
    {
        AdjustDrag();
        CheckSlope();
        PlayerInput();

        isGrounded = Physics.CheckSphere(groundDetectPoint.position, groundDetectRadius, groundLayers); //Detects ground
    }

    private void FixedUpdate()
    {
        Walk();
    }

    private void AdjustDrag() //Adjusts drag & applies additional gravity based on the conditions
    {
        if (isGrounded)
        {
            if (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical"))
            {
                rb.drag = counterdDrag;
            }
            else
                rb.drag = groundDrag;
        } 
        else
            rb.drag = airDrag;

        if (rb.velocity.y < 0.1f)
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

    private void Jump() //Used to apply jump force (in case you weren't able to figure it out)
    {
        rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
    }

    private void PlayerInput() //All input code go here
    {
        horizontalAxis = Input.GetAxis("Horizontal");
        verticalAxis = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
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

        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        //_TEST_
        Vector2 mag = FindVelRelativeToLook();
        CounterMovement(x, y, mag);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!isGrounded) return;

        //Counter movement
        Debug.Log(Mathf.Abs(x));
        if (Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            Debug.Log("Nice");
            rb.AddForce(walkSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            Debug.Log("Nice");
            rb.AddForce(walkSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        //Debug.Log("X: " + xMag + " Y: " + yMag);

        return new Vector2(xMag, yMag);
    }
}
