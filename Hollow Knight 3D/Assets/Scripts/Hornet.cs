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
    [SerializeField] private Transform exitPoint;
    [SerializeField] private LineRenderer lr;
    [SerializeField] private GameManager gm;

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
    [SerializeField] private LayerMask playerLayer;

    [Header("Dash Attack Settings")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float dashWaitTime = 0.5f;
    [SerializeField] private float dashRotationSpeed = 8f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Rest Settings")]
    [SerializeField] private float restTime = 4f;
    [SerializeField] private int maxRestCount = 3;

    [Header("Misc")]
    [SerializeField] private bool showColliders = false;
    [SerializeField] private bool manualControl = false;
    [SerializeField] private bool testAttack = false;
    [SerializeField] private int testAttackNumber = 1;
    #endregion

    #region PrivateVariables
    private float attackID = 0;
    private int prevMove = 6;
    private int[] attackCount = new int[7];
    private readonly int[] attackWeights = new int[] {0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 5};
    private GameObject particles;

    private float bufferTime = 0f;
    private float spinBufferTime = 0f;
    private float attackBufferTime = 0f;
    private bool once = false;
    [HideInInspector] public bool start = false;

    //Throw Needle
    private bool waitForNeedle = false;
    private GameObject thrownNeedle;
    private float needleTimer = 0f;
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
    //Rest Time
    private float restTimeBuffer = 0;
    private int restCount = 0;
    private bool restOnce = true;
    private int restStage = 5;
    private Vector3 ropePos = new Vector3(0, 0, 0);
    [HideInInspector] public bool rest = false;
    #endregion

    private void Awake()
    {
        normalsMesh.SetActive(true);
        System.Array.Fill(attackCount, 0);
        attackBufferTime = attackWaitTime;
    }

    void Update()
    {
        if(rest && restOnce)
        {
            RestReset();
            restOnce = false;
        }

        if (attackID == 0 && !manualControl)
            attackBufferTime -= Time.deltaTime;

        if (start && attackBufferTime < 0f)
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

                if (rest)
                    nextMove = 6;
                
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
            case 7:
                Rest();
                break;
            default:
                break;
        }

        if(transform.position.y < -5f)
        {
            RestReset();
            transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
        }

        //Development Inputs

        if (Input.GetKeyDown(KeyCode.Tilde))
            manualControl = !manualControl;

        if (Input.GetKeyUp(KeyCode.P) && Input.GetKey(KeyCode.L))
        {
            attackID = 0;
            spinStage = 0;
            jumpStage = 0;
            waitForNeedle = false;
            isDashing = true;
            bufferTime = throwNeedleWaitTime;

            foreach (GameObject g in slashParticles)
                g.SetActive(false);

            rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            holdingNeedle.SetActive(true);

            anim.SetBool("InAir", false);
            anim.SetBool("JumpAttack", false);
            anim.SetBool("Dashing", false);
            anim.SetBool("ThrowNeedle", false);
            anim.SetBool("Walking", false);
            anim.SetBool("SpinAttack", false);
            anim.SetBool("JumpAttackShoot", false);
            anim.SetBool("SpinAttackHold", false);

            animNormals.SetBool("InAir", false);
            animNormals.SetBool("JumpAttack", false);
            animNormals.SetBool("Dashing", false);
            animNormals.SetBool("ThrowNeedle", false);
            animNormals.SetBool("Walking", false);
            animNormals.SetBool("SpinAttack", false);
            animNormals.SetBool("JumpAttackShoot", false);
            animNormals.SetBool("SpinAttackHold", false);
        }    

        if(Input.GetKey(KeyCode.P)  && Input.GetKeyDown(KeyCode.Tilde))
        {
            if(attackID==1)
            {
                anim.SetBool("JumpAttack", false);
                anim.SetBool("SpinAttack", false);
                anim.SetBool("JumpAttackShoot", false);
                animNormals.SetBool("JumpAttack", false);
                animNormals.SetBool("SpinAttack", false);
                animNormals.SetBool("JumpAttackShoot", false);
                
                rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
                if (particles != null)
                    Destroy(particles);
                rb.velocity = new Vector3(0, 0, 0);
                once = false;
                attackID = 0;
            }
            else if( attackID == 2)
            {
                anim.SetBool("ThrowNeedle", false);
                animNormals.SetBool("ThrowNeedle", false);

                if(particles != null)
                    Destroy(particles);
                waitForNeedle = false;
                holdingNeedle.SetActive(true);
                bufferTime = throwNeedleWaitTime;
                if (thrownNeedle != null)
                    Destroy(thrownNeedle);
                attackID = 0;
            }
            else if(attackID == 3)
            {
                spinStage = 2;
                bufferTime = -1f;
            }
            else if (isDashing)
            {
                bufferTime = 0f;
                isDashing = false;
            }
            else if(attackID == 5)
            {
                anim.SetBool("Walking", false);
                animNormals.SetBool("Walking", false);
                prevWalk = nextWalk;
                attackID = 0;
            }
            else if(attackID == 6)
            {
                anim.SetBool("InAir", false);
                anim.SetBool("JumpAttack", false);
                animNormals.SetBool("InAir", false);
                animNormals.SetBool("JumpAttack", false);
                attackID = 0;
            }

            attackBufferTime = 0f;
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
            case 6:
                restCount++;
                restTimeBuffer = restTime;
                rest = false;
                restOnce = true;
                attackID = 7;
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

            if (particles != null)
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

    private void Rest()
    {
        if(restStage > 3)
        {
            restTimeBuffer -= Time.deltaTime;
            anim.SetBool("Resting", true);
            animNormals.SetBool("Resting", true);

            if (restTimeBuffer < 0f)
            {
                if (restCount == maxRestCount)
                {
                    anim.SetBool("Resting", false);
                    animNormals.SetBool("Resting", false);
                    restStage = 0;
                }
                else
                {
                    anim.SetBool("Resting", false);
                    animNormals.SetBool("Resting", false);
                    attackID = 0;
                }
            }
        }
        else if(restStage == 0)
        {
            anim.SetTrigger("JumpAttackTrigger");
            anim.SetBool("InAir", true);
            anim.SetBool("JumpAttack", true);
            animNormals.SetTrigger("JumpAttackTrigger");
            animNormals.SetBool("InAir", true);
            animNormals.SetBool("JumpAttack", true);
            lr.positionCount = 2;
            lr.SetPosition(0, holdingNeedle.transform.position);
            restTimeBuffer = 0.2f;
            holdingNeedle.SetActive(false);
            rb.useGravity = false;
            restStage++;
        }
        else if(restStage == 1 && restTimeBuffer > 0f)
        {
            restTimeBuffer -= Time.deltaTime;
            ropePos = Vector3.Lerp(holdingNeedle.transform.position, exitPoint.position, 1f);
            lr.SetPosition(1, ropePos);
        }
        else if(restStage == 1)
        {
            Vector3 dir = (exitPoint.position - transform.position).normalized;
            transform.Translate(2 * walkSpeed * Time.deltaTime * dir, Space.World);
            lr.SetPosition(0, holdingNeedle.transform.position);

            if (Vector3.Distance(transform.position, exitPoint.position) < 1f)
            {
                gm.running = false;
                Destroy(this.gameObject);
            }
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
            {
                if(colliders[0].gameObject != null && colliders[0].CompareTag("Player"))
                    colliders[0].gameObject.GetComponent<PlayerStats>().DecreaseHealth();
                Debug.Log("Hit");
            }
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
            needleTimer = 0f;
            bufferTime -= Time.deltaTime;
        }
        else if(!waitForNeedle)
        {
            thrownNeedle = Instantiate(needle, needleSpawn.position, needleSpawn.rotation);
            particles = Instantiate(dashParticle, needleSpawn.position, Quaternion.LookRotation(-transform.forward));
            holdingNeedle.SetActive(false);
            waitForNeedle = true;
        }
        else
        {
            needleTimer += Time.deltaTime;
            if (needleTimer > 4f)
            {
                anim.SetBool("ThrowNeedle", false);
                animNormals.SetBool("ThrowNeedle", false);

                Destroy(particles);
                waitForNeedle = false;
                holdingNeedle.SetActive(true);
                bufferTime = throwNeedleWaitTime;
                if(thrownNeedle != null)
                    Destroy(thrownNeedle);
                attackID = 0;
            }
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

    private void RestReset()
    {
        if (attackID == 1)
        {
            anim.SetBool("JumpAttack", false);
            anim.SetBool("SpinAttack", false);
            anim.SetBool("JumpAttackShoot", false);
            animNormals.SetBool("JumpAttack", false);
            animNormals.SetBool("SpinAttack", false);
            animNormals.SetBool("JumpAttackShoot", false);

            rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            if (particles != null)
                Destroy(particles);
            rb.velocity = new Vector3(0, 0, 0);
            once = false;
            attackID = 0;
        }
        else if (attackID == 2)
        {
            anim.SetBool("ThrowNeedle", false);
            animNormals.SetBool("ThrowNeedle", false);

            if (particles != null)
                Destroy(particles);
            waitForNeedle = false;
            holdingNeedle.SetActive(true);
            bufferTime = throwNeedleWaitTime;
            if (thrownNeedle != null)
                Destroy(thrownNeedle);
            attackID = 0;
        }
        else if (attackID == 3)
        {
            spinStage = 2;
            bufferTime = -1f;
        }
        else if (isDashing)
        {
            bufferTime = 0f;
            isDashing = false;
        }
        else if (attackID == 5)
        {
            anim.SetBool("Walking", false);
            animNormals.SetBool("Walking", false);
            prevWalk = nextWalk;
            attackID = 0;
        }
        else if (attackID == 6)
        {
            anim.SetBool("InAir", false);
            anim.SetBool("JumpAttack", false);
            animNormals.SetBool("InAir", false);
            animNormals.SetBool("JumpAttack", false);
            attackID = 0;
        }

        attackBufferTime = 0f;
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

            Destroy(particles);
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
            //dustParticle.Play();
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
