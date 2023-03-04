using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private ParticleSystem soulParticles;

    [Header("Settings")]
    [SerializeField] private float attackLength = 3f;
    [SerializeField] private LayerMask hitLayers;

    RaycastHit hit;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            Instantiate(spellPrefab, shootPoint.position, shootPoint.rotation);

        if (Input.GetKeyDown(KeyCode.Mouse0))
            Attack();
    }

    private void Attack()
    {
        bool damage = Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, attackLength, hitLayers);
        if(damage)
        {
            soulParticles.Play();
            HornetStats hs = hit.collider.gameObject.GetComponent<HornetStats>();
            hs.DecreaseHealth();
        }
    }
}
