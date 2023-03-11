using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform entryGate;
    [SerializeField] Transform exitGate;
    [SerializeField] Hornet hornet;

    [Header("UI")]
    [SerializeField] GameObject pauseScreen;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject pauseSettings;
    [SerializeField] GameObject endScreen;
    [SerializeField] GameObject timeScreen;
    [SerializeField] GameObject deathScreen;
    [SerializeField] GameObject crosshair;
    [SerializeField] TextMeshProUGUI timeSpent;
    [SerializeField] Image startImage;
    [SerializeField] private float startTransitionSpeed = 0.22f;

    [Header("Settings")]
    [SerializeField] private float gateSpeed = 1f;

    [HideInInspector] public bool running = false;
    private bool once1 = true;
    private bool once2 = true;
    private bool once3 = true;
    [HideInInspector] public bool paused = false;
    private bool gameOver = false;
    private bool settingsOpen = false;
    private Vector3 entryGateOpenPos;
    private Vector3 exitGateOpenPos;
    private Vector3 entryGateClosePos;
    private Vector3 exitGateClosePos;
    Color colorDefault = new(0, 0, 0, 0);
    private string timeTaken;

    private float timer = 0;

    private void Awake()
    {
        entryGateOpenPos = new(entryGate.position.x, 9.6f, entryGate.position.z);
        entryGateClosePos = new(entryGate.position.x, 6.3f, entryGate.position.z);

        exitGateOpenPos = new(exitGate.position.x, 4.1f, exitGate.position.z);
        exitGateClosePos = new(exitGate.position.x, -0.04f, exitGate.position.z);

        startImage.gameObject.SetActive(true);
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
            int minutes = (int)(timer / 60);
            int seconds = (int)(timer % 60);
            timeTaken = minutes + " minutes " + seconds + " seconds";
            Debug.Log("Minutes: " + minutes + " Seconds: " + seconds);
            once2 = false;
        }
        else
        {
            ControlGates(false);
        }

        if(Input.GetKeyDown(KeyCode.Keypad9))
        {
            running = false;
        }

        if (once3)
            StartTransition();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!paused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;
                pauseScreen.SetActive(true);
                paused = true;
            }
            else if(settingsOpen)
            {
                pauseMenu.SetActive(true);
                pauseSettings.SetActive(false);
                settingsOpen = false;
            }
            else if(!gameOver)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
                pauseScreen.SetActive(false);
                paused = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
            GameOver();
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

    public void Continue()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        paused = false;
    }

    public void GameOver()
    {
        endScreen.SetActive(true);
        crosshair.SetActive(false);

        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        timeScreen.SetActive(false);
        deathScreen.SetActive(true);
        gameOver = true;
        paused = true;
    }

    public void Retry()
    {
        gameOver = false;
        paused = false;
        Time.timeScale = 1;

        Debug.Log("Retry");
        crosshair.SetActive(true);
        startImage.color = new Color(0, 0, 0, 1);
        startImage.gameObject.SetActive(true);
        SceneManager.LoadScene("Greenpath");
    }

    private void StartTransition()
    {
        startImage.color = Color.Lerp(startImage.color, colorDefault, startTransitionSpeed);
        if (startImage.color.a < 0.002f)
        {
            startImage.gameObject.SetActive(false);
            once3 = false;
        }
    }

    public void OpenSettings()
    {
        pauseMenu.SetActive(false);
        pauseSettings.SetActive(true);
        settingsOpen = true;
    }

    public void Exit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (once1 && other.CompareTag("Player"))
        {
            hornet.start = true;
            running = true;
            once1 = false;
        }

        if(!running && !once2 && other.CompareTag("Player"))
        {
            endScreen.SetActive(true);
            crosshair.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            timeSpent.text = timeTaken;
        }
    }
}
