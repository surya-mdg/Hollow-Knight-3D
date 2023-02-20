using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Hornet : MonoBehaviour
{
    #region PublicVariables
    [Header("Refrences")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator anim;
    [SerializeField] private Animator animNormals; //Animator for another model used to fix transparent meshes
    [SerializeField] private GameObject normalsMesh; //Model used to fix mesh transparency issue
    [SerializeField] private GameObject needle;
    [SerializeField] private GameObject holdingNeedle;
    [SerializeField] private GameObject dashParticle;
    [SerializeField] private GameObject[] slashParticles;
    [SerializeField] private VisualEffect dustParticle;
    [SerializeField] private Transform target;
    [SerializeField] private Transform needleSpawn;
    [SerializeField] private Transform center;
    [SerializeField] private Transform respawnPoint;

    [Header("General Settings")]
    [SerializeField] private float attackWaitTime = 1.5f;
    [SerializeField] private float walkSpeed = 30f;
    [SerializeField] private float fakeJumpForce = 30f;
    [SerializeField] private Transform[] retrievePostions;

    [Header("Throw Needle Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float throwNeedleWaitTime = 1f;
    [SerializeField] private float radius = 2f;

    [Header("Jump Attack Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float shootForce = 50f;
    [SerializeField] private float maxHeight = 10f;
    [SerializeField] private float jumpFloatTime = 0.5f;

    [Header("Spin Needle Settings")]
    [SerializeField] private float spinJumpForce = 30f;
    [SerializeField] private float spinMaxHeight = 5f;
    [SerializeField] private float spinRadius = 4f;
    [SerializeField] private float spinTime = 1f;
    [SerializeField] private float spinChangeRate = 0.25f;

    [Header("Dash Attack Settings")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float dashWaitTime = 0.5f;
    [SerializeField] private float dashRotationSpeed = 8f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Misc")]
    [SerializeField] private bool showColliders = false;
    [SerializeField] private bool manualControl = false;
    [SerializeField] private bool testAttack = false;
    [SerializeField] private int testAttackNumber = 1;
    #endregion

    #region PrivateVariables
    private float attackID = 0;
    private int prevMove = 6;
    private int[] attackCount = new int[6];
    private readonly int[] attackWeights = new int[] {0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 5};
    private LayerMask playerLayer;
    private GameObject particles;

    private float bufferTime = 0f;
    private float spinBufferTime = 0f;
    private float attackBufferTime = 0f;
    private bool once = false;

    //Throw Needle
    private bool waitForNeedle = false;
    private GameObject thrownNeedle;
    //Jump Attack
    private int jumpStage = 0;
    //Spin Needle Attack
    private int spinStage = 0;
    private bool spinLock = false;
    //Dash Attack
    private bool isDashing = false;
    private Vector3 initialPos;
    //Walking
    private int prevWalk = 1;
    private int nextWalk = 0;
    #endregion

    private void Awake()
    {
        normalsMesh.SetActive(true);
        System.Array.Fill(attackCount, 0);
        playerLayer = LayerMask.NameToLayer("Player");
        attackBufferTime = attackWaitTime;
    }

    void Update()
    {
        if (attackID == 0 && !manualControl)
            attackBufferTime -= Time.deltaTime;

        if (attackBufferTime < 0f)
        {
            int nextMove = attackWeights[Random.Range(0, attackWeights.Length)];

            if (attackCount[nextMove] < 2)
            {
                if (prevMove != nextMove)
                    System.Array.Fill(attackCount, 0);

                if(testAttack)
                {
                    System.Array.Fill(attackCount, 0);
                    nextMove = testAttackNumber;
                }
                
                NextMove(nextMove);
                prevMove = nextMove;
                attackCount[nextMove]++;
                attackBufferTime = attackWaitTime;
            }
        }

        switch (attackID)
        {
            case 0:
                break;
            case 1:
                JumpAttack();
                break;
            case 2:
                ThrowNeedle();
                break;
            case 3:
                SpinNeedle();
                break;
            case 4:
                DashAttack();
                break;
            case 5:
                Walk();
                break;
            default:
                break;
        }

        //Development Inputs

        if (Input.GetKeyDown(KeyCode.Tilde))
            manualControl = !manualControl;

        if (Input.GetKeyUp(KeyCode.P))
        {
            attackID = 0;
            spinStage = 0;
            jumpStage = 0;
            waitForNeedle = false;
            bufferTime = throwNeedleWaitTime;

            anim.SetBool("InAir", false);
            anim.SetBool("JumpAttack", false);
            anim.SetBool("Dashing", false);
            anim.SetBool("ThrowNeedle", false);
            anim.SetBool("Walking", false);
            anim.SetBool("SpinAttack", false);
            anim.SetBool("JumpAttackShoot", false);
            anim.SetBool("SpinAttackHold", false);
        }    

        if (Input.GetKey(KeyCode.Tab))
        {
            transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
        }

        SlowTime();

        if (manualControl)
            ControlMoves();
    }

    private void NextMove(int nextMove)
    {
        switch (nextMove)
        {
            case 0:
                jumpStage = 0;
                attackID = 1;
                break;
            case 1:
                anim.SetTrigger("ThrowNeedleTrigger");
                anim.SetBool("ThrowNeedle", true);
                animNormals.SetTrigger("ThrowNeedleTrigger");
                animNormals.SetBool("ThrowNeedle", true);
                bufferTime = throwNeedleWaitTime;
                attackID = 2;
                break;
            case 2:
                spinStage = 0;
                attackID = 3;
                break;
            case 3:
                anim.SetTrigger("DashAttackTrigger");
                anim.SetBool("Dashing", true);
                animNormals.SetTrigger("DashAttackTrigger");
                animNormals.SetBool("Dashing", true);
                initialPos = transform.position;
                bufferTime = dashWaitTime;
                isDashing = true;
                attackID = 4;
                break;
            case 4:
                attackID = 5;
                break;
            case 5:
                anim.SetTrigger("JumpAttackTrigger");
                anim.SetBool("InAir", true);
                anim.SetBool("JumpAttack", true);
                animNormals.SetTrigger("JumpAttackTrigger");
                animNormals.SetBool("InAir", true);
                animNormals.SetBool("JumpAttack", true);
                StartCoroutine(nameof(FakeJump));
                attackID = 6;
                break;
            default:
                break;
        }
    }

    private void DashAttack()
    {
        if (bufferTime > 0f)
        {
            Rotate(false, dashRotationSpeed);
            bufferTime -= Time.deltaTime;
        }
        else if (isDashing)
        {
            if(!once)
            {
                particles = Instantiate(dashParticle, needleSpawn.position, Quaternion.LookRotation(-transform.forward,transform.up));
                once = true;
            }

            transform.Translate(dashSpeed * Time.deltaTime * Vector3.forward);

            if (Vector3.Distance(initialPos, transform.position) > maxDistance || Physics.CheckSphere(needleSpawn.position, 0.3f, groundLayers))
                isDashing = false;
        }
        else
        {
            anim.SetBool("Dashing", false);
            animNormals.SetBool("Dashing", false);

            attackID = 0;
            isDashing = false;
            once = false;

            Destroy(particles);
        }
    }

    IEnumerator FakeJump()
    {
        yield return new WaitForSeconds(0.2f);

        Rotate(true);
        Vector3 jumpDir = transform.up + (transform.forward / 8);
        rb.AddForce(fakeJumpForce * jumpDir, ForceMode.Impulse);
    }

    private void JumpAttack()
    {
        if (jumpStage == 0)
        {
            anim.SetTrigger("JumpAttackTrigger");
            anim.SetBool("JumpAttack", true);
            anim.SetBool("SpinAttack", true);
            animNormals.SetTrigger("JumpAttackTrigger");
            animNormals.SetBool("JumpAttack", true);
            animNormals.SetBool("SpinAttack", true);

            Rotate(true);
            Vector3 jumpDir = transform.up + (transform.forward / 4);
            rb.AddForce(jumpForce * jumpDir, ForceMode.Impulse);

            jumpStage++;
        }
        else if (jumpStage == 1)
        {
            if (transform.position.y >= maxHeight)
            {
                anim.SetBool("JumpAttackShoot", true);
                animNormals.SetBool("JumpAttackShoot", true);

                rb.constraints = RigidbodyConstraints.FreezePosition;
                bufferTime = jumpFloatTime;

                jumpStage++;  
            }
        }
        else if (bufferTime < 0f)
        {
            anim.SetBool("JumpAttackShoot", false);
            animNormals.SetBool("JumpAttackShoot", false);

            Rotate(true);
            rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            Vector3 dir = target.position - transform.position;

            if(!once)
            {
                particles = Instantiate(dashParticle, center.position, Quaternion.LookRotation(-dir));
                rb.AddForce(shootForce * dir.normalized, ForceMode.Impulse);
                once = true;
            }
        }
        else
        {
            bufferTime -= Time.deltaTime;
        }
    }

    private void SpinNeedle()
    {
        if (spinStage == 0)
        {
            anim.SetTrigger("JumpAttackTrigger");
            anim.SetBool("SpinAttack", true);
            anim.SetBool("JumpAttack", true);
            animNormals.SetTrigger("JumpAttackTrigger");
            animNormals.SetBool("SpinAttack", true);
            animNormals.SetBool("JumpAttack", true);

            Rotate(true);
            Vector3 jumpDir = transform.up + (transform.forward / 4);
            rb.AddForce(spinJumpForce * jumpDir, ForceMode.Impulse);

            spinStage++;
        }
        else if (spinStage == 1)
        {
            if (transform.position.y > spinMaxHeight && !spinLock)
            {
                    spinLock = true;
            }
            else if(transform.position.y <= spinMaxHeight && spinLock)
            {
                anim.SetBool("SpinAttackHold", true);
                animNormals.SetBool("SpinAttackHold", true);

                rb.constraints = RigidbodyConstraints.FreezePosition;
                holdingNeedle.SetActive(false);
                foreach (GameObject g in slashParticles)
                    g.SetActive(true);
                bufferTime = spinTime;

                spinLock = false;
                spinStage++;
            }
        }
        else if (bufferTime > 0f)
        {
            bufferTime -= Time.deltaTime;
            spinBufferTime -= Time.deltaTime;

            if (spinBufferTime < 0f)
            {
                foreach (GameObject g in slashParticles)
                {
                    Transform tran = g.transform;
                    int rand = Random.Range(0, 360);
                    tran.localRotation = Quaternion.Euler(tran.localRotation.eulerAngles.x, rand, tran.localRotation.eulerAngles.z);
                }

                spinBufferTime = spinChangeRate;
            }
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, spinRadius, playerLayer);
            if (colliders.Length > 0)
                Debug.Log("Hit");
        }
        else
        {
            anim.SetBool("SpinAttack", false);
            anim.SetBool("SpinAttackHold", false);
            anim.SetBool("JumpAttack", false);
            animNormals.SetBool("SpinAttack", false);
            animNormals.SetBool("SpinAttackHold", false);
            animNormals.SetBool("JumpAttack", false);

            foreach (GameObject g in slashParticles)
                g.SetActive(false);

            rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            holdingNeedle.SetActive(true);
        }
    }

    private void ThrowNeedle()
    {
        if(bufferTime > 0f)
        {
            Rotate(false,rotationSpeed);
            bufferTime -= Time.deltaTime;
        }
        else if(!waitForNeedle)
        {
            thrownNeedle = Instantiate(needle, needleSpawn.position, needleSpawn.rotation);
            holdingNeedle.SetActive(false);
            waitForNeedle = true;
        }
    }

    private void Walk()
    {
        if (prevWalk == nextWalk)
        {
            nextWalk = Random.Range(0, retrievePostions.Length);
            return;
        }

        anim.SetBool("Walking", true);
        animNormals.SetBool("Walking", true);
        Rotate(true, walkSpeed, true);
        Vector3 dir = (retrievePostions[nextWalk].position - transform.position).normalized;

        transform.Translate(walkSpeed * Time.deltaTime * dir, Space.World);

        if (Vector3.Distance(transform.position, retrievePostions[nextWalk].position) < 1f)
        {
            anim.SetBool("Walking", false);
            animNormals.SetBool("Walking", false);
            prevWalk = nextWalk;
            attackID = 0;
        }
    }

    private void Rotate(bool instant,float speed = 10f,bool walk = false)
    {
        if(walk)
        {
            Vector3 lookPos = retrievePostions[nextWalk].position - transform.position;
            lookPos.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(lookPos);
            transform.localRotation = lookRot;
            return;
        }

        if(instant)
        {
            Vector3 lookPos = target.position - transform.position;
            lookPos.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(lookPos);
            transform.localRotation = lookRot;
        }
        else
        {
            Vector3 lookPos = target.position - transform.position;
            lookPos.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(lookPos);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, lookRot, Time.deltaTime * speed);
        }  
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Untagged") && other.gameObject.CompareTag("Needle"))
        {
            anim.SetBool("ThrowNeedle", false);
            animNormals.SetBool("ThrowNeedle", false);

            waitForNeedle = false;
            holdingNeedle.SetActive(true);
            bufferTime = throwNeedleWaitTime;
            Destroy(thrownNeedle);
            attackID = 0;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (attackID == 1 && (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Player")))
        {
            anim.SetBool("JumpAttack", false);
            anim.SetBool("SpinAttack", false);
            animNormals.SetBool("JumpAttack", false);
            animNormals.SetBool("SpinAttack", false);
            dustParticle.Play();
            Destroy(particles);
            rb.velocity = new Vector3(0, 0, 0);
            once = false;
            attackID = 0;
        }
        else if(attackID == 6 || attackID == 3)
        {
            if(collision.gameObject.CompareTag("Ground"))
            {
                anim.SetBool("InAir", false);
                anim.SetBool("JumpAttack", false);
                animNormals.SetBool("InAir", false);
                animNormals.SetBool("JumpAttack", false);
                attackID = 0;
            }    
        }
    }

    //Functions below are made for analyzing game

    private void ControlMoves()
    {
        if (Input.GetKey(KeyCode.Z) && attackID == 0)
        {
            anim.SetTrigger("ThrowNeedleTrigger");
            anim.SetBool("ThrowNeedle", true);
            animNormals.SetTrigger("ThrowNeedleTrigger");
            animNormals.SetBool("ThrowNeedle", true);
            attackID = 1;
            bufferTime = throwNeedleWaitTime;
        }

        if (Input.GetKeyDown(KeyCode.X) && attackID == 0)
        {
            jumpStage = 0;
            attackID = 2;
        }

        if (Input.GetKeyDown(KeyCode.C) && attackID == 0)
        {
            spinStage = 0;
            attackID = 3;
        }

        if (Input.GetKeyDown(KeyCode.V) && attackID == 0)
        {
            anim.SetTrigger("DashAttackTrigger");
            anim.SetBool("Dashing", true);
            animNormals.SetTrigger("DashAttackTrigger");
            animNormals.SetBool("Dashing", true);
            initialPos = transform.position;
            bufferTime = dashWaitTime;
            isDashing = true;
            attackID = 4;
        }

        if (Input.GetKeyDown(KeyCode.N) && attackID == 0)
        {
            attackID = 5;
        }

        if (Input.GetKeyDown(KeyCode.M) && attackID == 0)
        {
            attackID = 6;
            anim.SetTrigger("JumpAttackTrigger");
            anim.SetBool("InAir", true);
            anim.SetBool("JumpAttack", true);
            animNormals.SetTrigger("JumpAttackTrigger");
            animNormals.SetBool("InAir", true);
            animNormals.SetBool("JumpAttack", true);
            StartCoroutine(nameof(FakeJump));
        }
    }

    private void SlowTime()
    {
        if(Input.GetKey(KeyCode.Backspace))
            Time.timeScale = 0.25f;
        else
            Time.timeScale = 1f;
    }

    private void OnDrawGizmos()
    {
        if(showColliders)
            Gizmos.DrawSphere(center.position, radius);
    }
}
