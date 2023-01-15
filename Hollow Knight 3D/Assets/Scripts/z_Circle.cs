using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class z_Circle : MonoBehaviour
{
    private float time = 0f;
    public float speed = 2f;
    public float radius = 4f;
    public float angle = 4f;
    void Update()
    {
        time += Time.deltaTime * speed;

        float x = Mathf.Cos(time) * radius;
        float y = Mathf.Sin(time) * radius;
        float z = Mathf.Sin(time) * angle;

        transform.position = new Vector3(x, y, z);
    }
}
