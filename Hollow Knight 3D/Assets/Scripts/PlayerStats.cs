using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerStats : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI playerHealth;
    [SerializeField] ParticleSystem focusParticles;
    [SerializeField] ParticleSystem focusParticlesBurst;
    [SerializeField] UIManager ui;
    [SerializeField] Image indicator;
    [SerializeField] Volume volume;

    [Header("UI Settings")]
    [SerializeField] float lowHealthIntensity = 0.5f;
    [SerializeField] float vignetteChangeRate = 0.1f;
    [SerializeField] float indicatorChangeRate = 10f;
    [SerializeField] Color indicatorEnd;
    [SerializeField] Color indicatorStart;

    [Header("Game Settings")]
    [SerializeField] float damageCooldown = 1f;
    [SerializeField] float reviveRate = 1.5f;
    [SerializeField] float reviveStartTime = 1f;
    [SerializeField] int maxHealth = 5;
    [SerializeField] int maxSoulPoints = 9;

    private float normalVignette = 0.145f;
    private float buffer = 0f;
    private float reviveBuffer = 0f;
    private float reviveStartTimeBuffer = 1f;
    private int contactCount = 0;
    private int needleCount = 0;
    private int health = 0;
    private bool once = true;
    private bool increase = true;
    [HideInInspector] public bool reviving = false;
    Color indicatorDefault = new(0, 0, 0, 0);

    [HideInInspector] public int soulPoints = 0;

    private PlayerMovement pm;
    private Vignette vignette;

    private void Awake()
    {
        health = maxHealth;
        reviveBuffer = reviveRate;
        reviveStartTimeBuffer = reviveStartTime;
        playerHealth.text = "" + health;
        pm = GetComponent<PlayerMovement>();
        volume.profile.TryGet<Vignette>(out vignette);
    }

    private void Update()
    {
        buffer -= Time.deltaTime;

        if (Input.GetKey(KeyCode.Q) && pm.isGrounded && soulPoints >= 3)
        {
            if(reviveStartTimeBuffer < 0f)
            {
                reviveBuffer -= Time.deltaTime;
                pm.reviving = true;
                reviving = true;
                if(once)
                {
                    focusParticles.Play();
                    once = false;
                }
            }

            if (reviveBuffer < 0f)
            {
                health++;
                IncreaseSoul(false);
                ui.UpdateHealthUI(maxHealth - health, false);
                playerHealth.text = "" + health;
                focusParticlesBurst.Play();
                StartCoroutine(nameof(SpawnParticles));

                reviveBuffer = reviveRate;
            }
            
            reviveStartTimeBuffer -= Time.deltaTime;
        }

        if (soulPoints < 3 || Input.GetKeyUp(KeyCode.Q))
        {
            focusParticles.Stop();
            pm.reviving = false;
            reviving = false;
            once = true;
            reviveStartTimeBuffer = reviveStartTime;
            reviveBuffer = reviveRate;
        }

        if (health == 1)
        {
            ControlVignette(true);
        }
        else
            ControlVignette(false);

        if (buffer > 0)
        {
            IndicateDamage();
        }
        else
        {
            indicator.color = Color.Lerp(indicator.color, indicatorDefault, indicatorChangeRate);
        }
    }

    public void DecreaseHealth()
    {
        if(buffer < 0f)
        {
            ui.UpdateHealthUI(maxHealth - health, true);
            health--;
            playerHealth.text = "" + health;
            buffer = damageCooldown;
        }
    }

    public void ControlVignette(bool state)
    {
        if (state)
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, lowHealthIntensity, vignetteChangeRate);
        }
        else
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, normalVignette, vignetteChangeRate);
        }
    }

    IEnumerator SpawnParticles()
    {
        yield return new WaitForSeconds(0.1f);
        focusParticlesBurst.Play();
        yield return new WaitForSeconds(0.1f);
        focusParticlesBurst.Play();
    }

    public void IncreaseSoul(bool increase)
    {
        if(increase)
        {
            if (soulPoints < maxSoulPoints)
            {
                soulPoints++;
                ui.UpdateSoulUI(soulPoints);
            }
                
        }
        else
        {
            if (soulPoints >= 2)
            {
                soulPoints = soulPoints - 2;
                ui.UpdateSoulUI(soulPoints);
            }
        }
    }

    private void IndicateDamage()
    {
        if(increase)
        {
            indicator.color = Color.Lerp(indicator.color, indicatorEnd, indicatorChangeRate);
            if (indicator.color.a >= (indicatorEnd.a - 0.001))
                increase = false;
        }
        else
        {
            indicator.color = Color.Lerp(indicator.color, indicatorStart, indicatorChangeRate);
            if (indicator.color.a <= (indicatorStart.a + 0.001))
                increase = true;
        }
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
