using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * speed * Time.deltaTime);
        transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * speed * Time.deltaTime);
    }
}
