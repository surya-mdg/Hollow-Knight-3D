using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI playerHealth;
    [SerializeField] ParticleSystem focusParticles;
    [SerializeField] ParticleSystem focusParticlesBurst;

    [SerializeField] float damageCooldown = 1f;
    [SerializeField] float reviveHoldTime = 1.5f;
    [SerializeField] int maxHealth = 5;

    private float buffer = 0f;
    private float reviveBuffer = 0f;
    private int contactCount = 0;
    private int needleCount = 0;
    private int health = 0;

    private PlayerMovement pm;

    private void Awake()
    {
        health = maxHealth;
        playerHealth.text = "" + health;
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        buffer -= Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Q))
            focusParticles.Play();

        if (Input.GetKey(KeyCode.Q) && pm.isGrounded)
        {
            if (reviveBuffer < 0f)
            {
                health++;
                playerHealth.text = "" + health;
                focusParticlesBurst.Play();
                StartCoroutine(nameof(SpawnParticles));

                reviveBuffer = reviveHoldTime;
            }

            pm.reviving = true;
            reviveBuffer -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            focusParticles.Stop();
            pm.reviving = false;
            reviveBuffer = reviveHoldTime;
        }
    }

    public void DecreaseHealth()
    {
        if(buffer < 0f)
        {
            health--;
            playerHealth.text = "" + health;
            buffer = damageCooldown;
        }
    }

    IEnumerator SpawnParticles()
    {
        yield return new WaitForSeconds(0.1f);
        focusParticlesBurst.Play();
        yield return new WaitForSeconds(0.1f);
        focusParticlesBurst.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(buffer < 0f && other.CompareTag("Needle"))
        {
            Debug.Log("Damage[" + ++needleCount + "]: Needle");
            DecreaseHealth();
            buffer = damageCooldown;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (buffer < 0f && collision.collider.CompareTag("Enemy"))
        {
            Debug.Log("Damage[" + ++contactCount + "]: Hornet");
            DecreaseHealth();
            buffer = damageCooldown;
        }
    }
}
