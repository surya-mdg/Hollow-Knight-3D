using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image soulBar;
    [SerializeField] private Sprite[] soulBarSprites;
    [SerializeField] private GameObject[] healthBar;

    void Update()
    {
        
    }

    public void UpdateSoulUI(int index)
    {
        soulBar.sprite = soulBarSprites[index];
    }

    public void UpdateHealthUI(int index, bool state)
    {
        healthBar[index].SetActive(state);
    }
}
