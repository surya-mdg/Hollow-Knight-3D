using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : MonoBehaviour
{
    [SerializeField] private Transform tip;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float waitTime = 0.5f;

    private bool retrieve = false;
    private float buffer = 0;

    private void Awake()
    {
        buffer = waitTime;
    }

    void Update()
    {
        Attack();
    }

    void Attack()
    {
        if(!retrieve)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            Collider[] colliders = Physics.OverlapSphere(tip.position, 0.5f);
            foreach (Collider i in colliders)
            {
                if (i.tag != null && i.tag == "Ground")
                {
                    Debug.Log(i);
                    retrieve = true;
                }
            }
        }
        else
        {
            if(buffer<0f)
                transform.Translate(-Vector3.forward * speed * Time.deltaTime);

            buffer -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag !=  null && collision.gameObject.tag == "Ground")
        {
            //retrieve = true;
        }
    }
}
