using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private GameObject needle;
    [SerializeField] private Transform target;
    [SerializeField] private Transform needleSpawn;
    [SerializeField] private Transform center;

    [Header("Settings")]
    [SerializeField] private float rotSpeed = 2f;
    [SerializeField] private float throwNeedleWaitTime = 1f;

    [Header("Misc")]
    [SerializeField] private bool showColliders = false;

    public float radius = 2f;
    private float attackID = 1;
    private float bufferTime = 0f;
    private bool waitForNeedle = false;
    private GameObject thrownNeedle;

    private void Awake()
    {
        bufferTime = throwNeedleWaitTime;
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Space) && attackID == 1)
        {
            attackID = 0;
        }

        if (Input.GetKeyUp(KeyCode.L))
        {
            attackID = 0;
            waitForNeedle = false;
            bufferTime = throwNeedleWaitTime;
        }
            
        if(attackID == 0)
        {
            ThrowNeedle();
        }
    }

    void ThrowNeedle()
    {
        if(bufferTime > 0f)
        {
            Vector3 lookPos = target.position - transform.position;
            lookPos.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(lookPos);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, lookRot, Time.deltaTime * rotSpeed);
            bufferTime -= Time.deltaTime;
        }
        else if(!waitForNeedle)
        {
            thrownNeedle = Instantiate(needle, needleSpawn.position, needleSpawn.rotation);
            waitForNeedle = true;
        }
        else
        {/*
            Collider[] colliders = Physics.OverlapSphere(center.position, radius);
            foreach(Collider i in colliders)
            {
                if(i.tag != null && i.tag=="Needle")
                {
                    attackID = 1;
                    
                    Destroy(thrownNeedle);
                }
            }*/
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered");
        if(other.gameObject.tag != null && other.gameObject.tag == "Needle")
        {
            attackID = 1;
            waitForNeedle = false;
            bufferTime = throwNeedleWaitTime;
            Destroy(thrownNeedle);
        }
    }

    private void OnDrawGizmos()
    {
        if(showColliders)
            Gizmos.DrawSphere(center.position, radius);
    }
}
