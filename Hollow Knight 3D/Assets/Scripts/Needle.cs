using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : MonoBehaviour
{
    [SerializeField] private Transform tip;
    [SerializeField] private Transform end;
    [SerializeField] private LineRenderer rope;
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
        rope.positionCount = 2;
        rope.SetPosition(0, end.position - (end.forward * 2.5f));
    }

    void Update()
    {
        Attack();
    }

    void Attack()
    {
        if(!retrieve)
        {
            transform.Translate(throwSpeed * Time.deltaTime * Vector3.forward);
            Collider[] colliders = Physics.OverlapSphere(tip.position, 0.5f);
            rope.SetPosition(1, end.position);
            foreach (Collider i in colliders)
            {
                if (i.CompareTag("Ground"))
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
                transform.Translate(retrieveSpeed * Time.deltaTime * -Vector3.forward);

            buffer -= Time.deltaTime;
            rope.SetPosition(1, end.position);

            if (Vector3.Distance(initialPos, transform.position) > (maxDistance + 10f))
                Destroy(this.gameObject);
        }
    }
}
