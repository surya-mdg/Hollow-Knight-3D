using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform entryGate;
    [SerializeField] Transform exitGate;
    [SerializeField] Hornet hornet;

    [Header("Settings")]
    [SerializeField] private float gateSpeed = 1f;

    [HideInInspector] public bool running = false;
    private bool once1 = true;
    private bool once2 = true;
    private Vector3 entryGateOpenPos;
    private Vector3 exitGateOpenPos;
    private Vector3 entryGateClosePos;
    private Vector3 exitGateClosePos;

    private float timer = 0;

    private void Awake()
    {
        entryGateOpenPos = new(entryGate.position.x, 9.6f, entryGate.position.z);
        entryGateClosePos = new(entryGate.position.x, 6.3f, entryGate.position.z);

        exitGateOpenPos = new(exitGate.position.x, 4.1f, exitGate.position.z);
        exitGateClosePos = new(exitGate.position.x, -0.04f, exitGate.position.z);
    }

    void Update()
    {
        if(running && !once1)
        {
            timer += Time.deltaTime;

            ControlGates(true);
        }
        else if(once2 && !once1)
        {
            float minutes = timer / 60f;
            float seconds = timer % 60f;
            Debug.Log("Minutes: " + minutes + " Seconds: " + seconds);
            once2 = false;
        }
        else
        {
            ControlGates(false);
        }
    }

    private void ControlGates(bool state)
    {
        if(state)
        {
            entryGate.position = Vector3.Lerp(entryGate.position, entryGateClosePos, gateSpeed);
            exitGate.position = Vector3.Lerp(exitGate.position, exitGateClosePos, gateSpeed);
        }
        else
        {
            entryGate.position = Vector3.Lerp(entryGate.position, entryGateOpenPos, gateSpeed);
            exitGate.position = Vector3.Lerp(exitGate.position, exitGateOpenPos, gateSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (once1 && other.CompareTag("Player"))
        {
            hornet.start = true;
            running = true;
            once1 = false;
        }
    }
}
