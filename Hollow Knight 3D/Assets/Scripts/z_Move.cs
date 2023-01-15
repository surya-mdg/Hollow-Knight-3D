using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class z_Move : MonoBehaviour
{
    public Transform pos;
    public float speed = 10f;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.O))
        {
            Vector3 dir = (pos.position - transform.position).normalized;

            transform.Translate(speed * Time.deltaTime * dir);
        }
    }
}
