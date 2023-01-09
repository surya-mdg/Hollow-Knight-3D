using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject needle;
    [SerializeField] private Transform target;
    [SerializeField] private Transform needleSpawn;
    [SerializeField] private Transform center;
    [SerializeField] private Transform respawnPoint;

    [Header("Throw Needle Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float throwNeedleWaitTime = 1f;
    [SerializeField] private float radius = 2f;

    [Header("Jump Attack Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float shootForce = 50f;
    [SerializeField] private float maxHeight = 10f;
    [SerializeField] private float jumpFloatTime = 0.5f;


    [Header("Misc")]
    [SerializeField] private bool showColliders = false;

    private float attackID = 0;
    private float bufferTime = 0f;

    //Throw Needle
    private bool waitForNeedle = false;
    private GameObject thrownNeedle;
    //Jump Attack
    private int jumpStage = 0;

    private void Awake()
    {
       
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Z) && attackID == 0)
        {
            attackID = 1;
            bufferTime = throwNeedleWaitTime;
        }

        if (Input.GetKeyDown(KeyCode.X) && attackID == 0)
        {
            jumpStage = 0;
            attackID = 2;
        }

        if (attackID == 1)
        {
            ThrowNeedle();
        }
        else if (attackID == 2)
        {
            JumpAttack();
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            attackID = 0;
            waitForNeedle = false;
            bufferTime = throwNeedleWaitTime;

            jumpStage = 0;
        }    

        if (Input.GetKey(KeyCode.Tab))
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }

        SlowTime();
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
            waitForNeedle = true;
        }
    }

    private void JumpAttack()
    {
        if(jumpStage == 0)
        {
            anim.SetTrigger("JumpAttackTrigger");
            anim.SetBool("JumpAttack", true); 
            Rotate(true);
            Vector3 jumpDir = transform.up + (transform.forward / 4);
            rb.AddForce(jumpForce * jumpDir, ForceMode.Impulse);
            jumpStage++;
        }
        else if(jumpStage == 1)
        {
            if(transform.position.y >= maxHeight)
            {
                anim.SetBool("JumpAttackShoot", true);
                rb.constraints = RigidbodyConstraints.FreezePosition;
                jumpStage++;
                bufferTime = jumpFloatTime;
            }
        }
        else if(bufferTime < 0f)
        {
            anim.SetBool("JumpAttackShoot", false);
            rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            Rotate(true);
            Vector3 dir = target.position - transform.position;
            rb.AddForce(shootForce * dir.normalized, ForceMode.Impulse);
        }
        else
        {
            bufferTime -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    { 
        if(other.gameObject.tag != null && other.gameObject.tag == "Needle")
        {
            attackID = 0;
            waitForNeedle = false;
            bufferTime = throwNeedleWaitTime;
            Destroy(thrownNeedle);
        }
    }

    private void Rotate(bool instant,float speed = 10f)
    {
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

    private void OnCollisionEnter(Collision collision)
    {
        if(attackID==2)
        {
            if (collision.gameObject.tag != null && (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Player"))
            {
                anim.SetBool("JumpAttack", false);
                rb.velocity = new Vector3(0, 0, 0);
                attackID = 0;
            }
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
