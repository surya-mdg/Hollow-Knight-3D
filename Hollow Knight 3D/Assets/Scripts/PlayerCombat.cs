using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private GameManager gm;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private ParticleSystem soulParticles;

    [Header("Audio")]
    [SerializeField] private AudioSource slash;
    [SerializeField] private AudioSource cast;
    [SerializeField] private AudioSource causeDamage;
    [SerializeField] private AudioSource groundHit;

    [Header("Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float castCooldown = 0.5f;
    [SerializeField] private float attackLengthForward = 2f;
    [SerializeField] private float attackLengthDown = 4f;
    [SerializeField] private float minDownAttackAngle = 70f;
    [SerializeField] private float attackImpulseForward = 5f;
    [SerializeField] private float attackImpulseDown = 15f;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private LayerMask ground;

    RaycastHit hit;
    RaycastHit hit1;
    float attackLength = 0;
    float attackBuffer = 0;
    float castBuffer = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && castBuffer < 0 && !stats.reviving && !gm.paused)
            Cast();

        if (Input.GetKeyDown(KeyCode.Mouse0) && attackBuffer < 0 && !stats.reviving && !gm.paused)
            Attack();

        if (cam.transform.localRotation.eulerAngles.x < minDownAttackAngle)
            attackLength = attackLengthForward;
        else
        {
            attackLength = attackLengthDown;
        }

        attackBuffer -= Time.deltaTime;
        castBuffer -= Time.deltaTime;
    }

    private void Cast()
    {
        if(stats.soulPoints >= 3)
        {
            cast.Play();
            castBuffer = castCooldown;
            stats.IncreaseSoul(false);
            Instantiate(spellPrefab, shootPoint.position, shootPoint.rotation);
        }
    }

    private void Attack()
    {
        bool damage = Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, attackLength, hitLayers);
        bool hitGround = Physics.Raycast(cam.transform.position, cam.transform.forward, out hit1, attackLength, ground);
        attackBuffer = attackCooldown;
        slash.Play();

        if (damage)
        {
            stats.IncreaseSoul(true);
            causeDamage.Play();
            soulParticles.Play();
            if (attackLength == attackLengthForward)
            {
                rb.AddForce(attackImpulseForward * -cam.transform.forward, ForceMode.Impulse);
            }
            else
            {
                rb.velocity = new(0, 0, 0);
                rb.AddForce(attackImpulseDown * Vector3.up, ForceMode.Impulse);
            }

            HornetStats hs = hit.collider.gameObject.GetComponent<HornetStats>();
            if(hs != null)
                hs.DecreaseHealth();
        }
        else if(hitGround)
        {
            groundHit.Play();
        }
    }
}
