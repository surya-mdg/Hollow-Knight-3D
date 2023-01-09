using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : MonoBehaviour
{
    [SerializeField] private Transform tip;
    [SerializeField] private float throwSpeed = 80f;
    [SerializeField] private float retrieveSpeed = 60f;
    [SerializeField] private float waitTime = 0.5f;
    [SerializeField] private float maxDistance = 15f;

    private bool retrieve = false;
    private float buffer = 0;
    private Vector3 initialPos;

    private void Awake()
    {
        buffer = waitTime;
        initialPos = transform.position;
    }

    void Update()
    {
        Attack();
    }

    void Attack()
    {
        if(!retrieve)
        {
            transform.Translate(Vector3.forward * throwSpeed * Time.deltaTime);
            Collider[] colliders = Physics.OverlapSphere(tip.position, 0.5f);
            foreach (Collider i in colliders)
            {
                if (i.tag != null && i.tag == "Ground")
                {
                    retrieve = true;
                }
            }

            if (Vector3.Distance(initialPos, transform.position) > maxDistance)
                retrieve = true;
        }
        else
        {
            if(buffer<0f)
                transform.Translate(-Vector3.forward * retrieveSpeed * Time.deltaTime);

            buffer -= Time.deltaTime;

            if (Vector3.Distance(initialPos, transform.position) > (maxDistance + 10f))
                Destroy(this.gameObject);
        }
    }
}
