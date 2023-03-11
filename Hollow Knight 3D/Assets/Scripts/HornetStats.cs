using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HornetStats : MonoBehaviour
{
    [SerializeField] GameObject hitCount;
    [SerializeField] TextMeshProUGUI playerHealth;
    [SerializeField] private int maxHits = 3;

    private int currHits = 0;
    private bool showCount = false;

    private Hornet hornet;

    private void Awake()
    {
        hornet = GetComponent<Hornet>();

        if(playerHealth != null)
            playerHealth.text = "" + currHits;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            DecreaseHealth();

        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            if (!showCount)
            {
                hitCount.SetActive(true);
                showCount = true;
            }
            else
            {
                hitCount.SetActive(false);
                showCount = false;
            }
        }
    }

    public void DecreaseHealth()
    {
        currHits++;
        
        if (currHits == maxHits)
        {
            hornet.rest = true;
            currHits = 0;
        }

        if (playerHealth != null)
            playerHealth.text = "" + currHits;
    }
}
