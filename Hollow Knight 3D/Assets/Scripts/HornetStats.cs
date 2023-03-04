using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HornetStats : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerHealth;
    [SerializeField] private int maxHits = 3;

    private int currHits = 0;

    private Hornet hornet;

    private void Awake()
    {
        hornet = GetComponent<Hornet>();
        playerHealth.text = "" + currHits;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            DecreaseHealth();
    }

    public void DecreaseHealth()
    {
        currHits++;
        
        if (currHits == maxHits)
        {
            hornet.rest = true;
            currHits = 0;
        }

        playerHealth.text = "" + currHits;
    }
}
