using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class z_Move : MonoBehaviour
{
    public Transform pos;
    public float speed = 10f;
    public Transform slash;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            int rand = Random.Range(0, 360);
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, rand, transform.localRotation.eulerAngles.z);
            rand = Random.Range(0, 360);
            slash.localRotation = Quaternion.Euler(slash.localRotation.eulerAngles.x, rand, slash.localRotation.eulerAngles.z);
        }
    }
}
