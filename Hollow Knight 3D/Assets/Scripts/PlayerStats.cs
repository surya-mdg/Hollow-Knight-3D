using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] float damageCooldown = 1f;

    private float buffer = 0f;
    private int contactCount = 0;
    private int needleCount = 0;

    private void Update()
    {
        buffer -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(buffer < damageCooldown && other.CompareTag("Needle"))
        {
            Debug.Log("Damage[" + ++needleCount + "]: Needle");
            buffer = damageCooldown;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (buffer < damageCooldown && collision.collider.CompareTag("Enemy"))
        {
            Debug.Log("Damage[" + ++contactCount + "]: Hornet");
            buffer = damageCooldown;
        }
    }
}
