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

    [Header("Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float castCooldown = 0.5f;
    [SerializeField] private float attackLengthForward = 2f;
    [SerializeField] private float attackLengthDown = 4f;
    [SerializeField] private float minDownAttackAngle = 70f;
    [SerializeField] private float attackImpulseForward = 5f;
    [SerializeField] private float attackImpulseDown = 15f;
    [SerializeField] private LayerMask hitLayers;

    RaycastHit hit;
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
            castBuffer = castCooldown;
            stats.IncreaseSoul(false);
            Instantiate(spellPrefab, shootPoint.position, shootPoint.rotation);
        }
    }

    private void Attack()
    {
        bool damage = Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, attackLength, hitLayers);
        attackBuffer = attackCooldown;
        if (damage)
        {
            stats.IncreaseSoul(true);
            soulParticles.Play();
            HornetStats hs = hit.collider.gameObject.GetComponent<HornetStats>();
            if(hs != null)
                hs.DecreaseHealth();

            if(attackLength == attackLengthForward)
            {
                rb.AddForce(attackImpulseForward * -cam.transform.forward, ForceMode.Impulse);
            }
            else
            {
                rb.velocity = new(0, 0, 0);
                rb.AddForce(attackImpulseDown * Vector3.up, ForceMode.Impulse);
            }
        }
    }
}
