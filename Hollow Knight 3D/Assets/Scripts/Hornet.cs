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
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;

    [Header("Audio")]
    [SerializeField] private AudioSource dash;
    [SerializeField] private AudioSource dashSkid; //Also used in Jump Attack
    [SerializeField] private AudioSource throwNeedleEgalis;
    [SerializeField] private AudioSource throwNeedleSwish;
    [SerializeField] private AudioSource jumpSound; //Used in Jump Attack, Spin Needle, Fake Jump & Rest
    [SerializeField] private AudioSource jumpAttackShaw;
    [SerializeField] private AudioSource spinNeedleAdiros;
    [SerializeField] private AudioSource spinNeedleRope;
    [SerializeField] private AudioSource walk;
    [SerializeField] private AudioSource restHit;
    [SerializeField] private AudioSource restNeedleHit;
    [SerializeField] private AudioSource restFinalHit;
    [SerializeField] private AudioSource restFinalBlast;
    [SerializeField] private AudioSource exitSound;
    [SerializeField] private AudioSource startSound;
    [SerializeField] private AudioSource dodgeSound0;
    [SerializeField] private AudioSource dodgeSound1;

    [Header("General Settings")]
    [SerializeField] private float attackWaitTime = 1.5f;
    [SerializeField] private float walkSpeed = 30f;
    [SerializeField] private float fakeJumpForce = 30f;
    [SerializeField] private Transform[] retrievePostions;

    [Header("Throw Needle Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float throwNeedleWaitTime = 1f;

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
    private readonly int[] attackWeights = new int[] {0, 0, 0, 1, 1, 2, 2, 3, 3, 3, 4, 5};
    private GameObject particles;

    private float bufferTime = 0f;
    private float spinBufferTime = 0f;
    private float attackBufferTime = 0f;
    private bool once = false;
    private bool exitSoundOnce = true;
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
    //Dodge
    private bool dodging = false;
    private float dodgeBuffer = 0f;
    private float dodgeLimitBuffer = 0f;
    //Rest Time
    private float restTimeBuffer = 0;
    private int restCount = 0;
    private bool restOnce = true;
    private int restStage = 5;
    private bool startOnce = true;
    private Vector3 ropePos = new Vector3(0, 0, 0);
    private PlayerStats stats;
    [HideInInspector] public bool rest = false;
    #endregion

    private void Awake()
    {
        normalsMesh.SetActive(true);
        System.Array.Fill(attackCount, 0);
        attackBufferTime = attackWaitTime + 0.75f;
        
        stats = target.gameObject.GetComponent<PlayerStats>();
    }

    void Update()
    {
        if(rest && restOnce)
        {
            RestReset();
            restOnce = false;
        }

        if (start && attackID == 0 && !manualControl)
        {
            if(startOnce)
            {
                startSound.Play();
                anim.SetBool("Start", true);
                animNormals.SetBool("Start", true);
                StartCoroutine(nameof(ResetStart));
                startOnce = false;
            }

            attackBufferTime -= Time.deltaTime;
        }

        if(attackID == 0 && !dodging && Physics.Raycast(groundCheck.position, -Vector3.up, 0.1f, groundLayers))
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

        if (start && attackBufferTime < 0f && !dodging)
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

        if (dodgeBuffer >= 0)
            dodgeBuffer -= Time.deltaTime;

        if(dodging)
        {
            dodgeLimitBuffer += Time.deltaTime;
            if(dodgeLimitBuffer > 1f)
            {
                anim.SetBool("InAir", false);
                anim.SetBool("JumpAttack", false);
                animNormals.SetBool("InAir", false);
                animNormals.SetBool("JumpAttack", false);
                rb.velocity = new(0, 0, 0);
                attackBufferTime = 0.5f;
                dodgeLimitBuffer = 0;
                dodging = false;
            }
        }

        if(attackID == 6 || attackID == 3)
        {
            dodgeLimitBuffer += Time.deltaTime;
            if (dodgeLimitBuffer > 0.75f)
                CheckGround();
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

        if (manualControl)
            ControlMoves();
    }

    private void NextMove(int nextMove)
    {
        rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;

        switch (nextMove)
        {
            case 0:
                jumpStage = 0;
                attackID = 1;
                break;
            case 1:
                throwNeedleEgalis.Play();
                anim.SetTrigger("ThrowNeedleTrigger");
                anim.SetBool("ThrowNeedle", true);
                animNormals.SetTrigger("ThrowNeedleTrigger");
                animNormals.SetBool("ThrowNeedle", true);
                bufferTime = throwNeedleWaitTime;
                attackID = 2;
                rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                break;
            case 2:
                spinStage = 0;
                attackID = 3;
                break;
            case 3:
                dash.Play();
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
                walk.Play();
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
                if (restCount < maxRestCount)
                {
                    restHit.Play();
                    restNeedleHit.Play();
                }
                else
                {
                    restTimeBuffer = 3f;
                    gm.StopMusic();
                    restFinalBlast.Play();
                }
                rest = false;
                restOnce = true;
                attackID = 7;
                break;
            default:
                break;
        }
    }

    private void CheckGround() //Checks if it is touching the base ground
    {
        if (Physics.Raycast(groundCheck.position, -Vector3.up, 0.1f, groundLayers))
        {
            anim.SetBool("InAir", false);
            anim.SetBool("JumpAttack", false);
            animNormals.SetBool("InAir", false);
            animNormals.SetBool("JumpAttack", false);
            dodgeLimitBuffer = 0f;
            attackID = 0;
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
                dashSkid.Play();
                particles = Instantiate(dashParticle, needleSpawn.position, Quaternion.LookRotation(-transform.forward,transform.up));
                once = true;
            }

            if (Vector3.Distance(initialPos, transform.position) < maxDistance && !Physics.CheckSphere(wallCheck.position, 1f, groundLayers))
                transform.Translate(dashSpeed * Time.deltaTime * Vector3.forward);
            else
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
        jumpSound.Play();
        Vector3 jumpDir = transform.up + (transform.forward / 8);
        rb.AddForce(fakeJumpForce * jumpDir, ForceMode.Impulse);
    }

    IEnumerator Dodge()
    {
        anim.SetTrigger("JumpAttackTrigger");
        anim.SetBool("InAir", true);
        anim.SetBool("JumpAttack", true);
        animNormals.SetTrigger("JumpAttackTrigger");
        animNormals.SetBool("InAir", true);
        animNormals.SetBool("JumpAttack", true);

        yield return new WaitForSeconds(0.2f);

        Rotate(true);
        int rand = Random.Range(0, 3);
        if (rand == 0)
            dodgeSound1.Play();
        else
            dodgeSound0.Play();
        Vector3 jumpDir = (transform.up / 4) + -transform.forward;
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
            jumpSound.Play();
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

                jumpAttackShaw.Play();
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
                dashSkid.Play();
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

            Vector3 lookPos = exitPoint.position - transform.position;
            lookPos.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(lookPos);
            transform.localRotation = lookRot;

            restStage++;
        }
        else if(restStage == 1 && restTimeBuffer > 0f)
        {
            restTimeBuffer -= Time.deltaTime;
            ropePos = Vector3.Lerp(holdingNeedle.transform.position, exitPoint.position, 1f);
            lr.SetPosition(1, ropePos);

            if (exitSoundOnce)
            {
                exitSound.Play();
                jumpSound.Play();
                exitSoundOnce = false;
            }
        }
        else if(restStage == 1)
        {
            Vector3 dir = (exitPoint.position - transform.position).normalized;
            transform.Translate(3 * walkSpeed * Time.deltaTime * dir, Space.World);
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
            anim.SetBool("InAir", true);
            animNormals.SetTrigger("JumpAttackTrigger");
            animNormals.SetBool("InAir", true);
            animNormals.SetBool("SpinAttack", true);
            animNormals.SetBool("JumpAttack", true);

            Rotate(true);
            jumpSound.Play();
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
                spinNeedleAdiros.Play();
                spinNeedleRope.Play();
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
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

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
            
            if(Vector3.Distance(target.position,transform.position) <= spinRadius)
            {
                stats.DecreaseHealth();
            }
        }
        else
        {
            spinNeedleRope.Stop();
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
            throwNeedleSwish.Play();
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
            walk.Stop();
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
            walk.Stop();
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

    public void DodgeTrigger() //Starts the dodge move
    {
        if(attackID == 0 && attackBufferTime > 0f && !dodging && dodgeBuffer < 0)
        {
            dodgeBuffer = 2f;
            dodging = true;
            rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            StartCoroutine(nameof(Dodge));
        }
    }

    IEnumerator ResetStart() //Reset animation after starting sequence
    {
        yield return new WaitForSeconds(0.6f);

        anim.SetBool("Start", false);
        animNormals.SetBool("Start", false);
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

            Destroy(particles);
            rb.velocity = new Vector3(0, 0, 0);
            once = false;
            attackID = 0;
        }
        else if(dodging)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                anim.SetBool("InAir", false);
                anim.SetBool("JumpAttack", false);
                animNormals.SetBool("InAir", false);
                animNormals.SetBool("JumpAttack", false);
                rb.velocity = new(0, 0, 0);
                attackBufferTime = 0.5f;
                dodgeLimitBuffer = 0;
                dodging = false;
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

    private void OnDrawGizmos()
    {
        if(showColliders)
            Gizmos.DrawSphere(transform.position, spinRadius);
    }
}
