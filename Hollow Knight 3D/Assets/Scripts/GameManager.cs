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
    [SerializeField] GameObject designScreen;
    [SerializeField] GameObject timeScreen;
    [SerializeField] GameObject creditsScreen;
    [SerializeField] GameObject deathScreen;
    [SerializeField] GameObject controlsScreen;
    [SerializeField] GameObject crosshair;
    [SerializeField] TextMeshProUGUI timeSpent;
    [SerializeField] TextMeshProUGUI volumeText;
    [SerializeField] Image startImage;
    [SerializeField] private float startTransitionSpeed = 0.22f;

    [Header("Audio")]
    [SerializeField] AudioSource backgroundMusic;
    [SerializeField] AudioSource ambientNoise;
    [SerializeField] AudioSource gatesClose;
    [SerializeField] AudioSource gatesOpen;
    [SerializeField] AudioSource playerDeath;
    [SerializeField] AudioSource gameWon;
    public AudioSource uiClick;

    [Header("Settings")]
    [SerializeField] private float gateSpeed = 1f;
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float ambientVolume = 0.75f;
    [SerializeField] private float pauseVolume = 0.1f;
    [SerializeField] private float pauseAmbientVolume = 0.1f;
    [SerializeField] private string sceneName = "Greenpath";

    [HideInInspector] public bool running = false;
    private bool once1 = true;
    private bool once2 = true;
    private bool once3 = true;
    private bool once4 = false;
    private bool once5 = true;
    private bool once6 = true;
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
        controlsScreen.SetActive(true);
        backgroundMusic.volume = musicVolume;
        volumeText.text = string.Format("{0:0.00}", musicVolume);
        ambientNoise.Play();
        ambientNoise.volume = ambientVolume;
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
            once4 = true;
        }
        else
        {
            if (once4)
            {
                gatesOpen.Play();
                once4 = false;
            }

            ControlGates(false);
        }

        if (once3)
            StartTransition();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!paused)
            {
                uiClick.Play();
                backgroundMusic.volume = pauseVolume;
                ambientNoise.volume = pauseAmbientVolume;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;
                pauseScreen.SetActive(true);
                paused = true;
            }
            else if(settingsOpen)
            {
                uiClick.Play();
                pauseMenu.SetActive(true);
                pauseSettings.SetActive(false);
                settingsOpen = false;
            }
            else if(!gameOver)
            {
                Continue();
            }
        }

        if (Input.GetKey(KeyCode.P) && Input.GetKeyDown(KeyCode.Keypad9))
        {
            StopMusic();
            running = false;
        }

        if (Input.GetKey(KeyCode.P) && Input.GetKeyDown(KeyCode.Keypad3))
            GameOver();

        if (once6 && (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Escape)))
        {
            uiClick.Play();
            controlsScreen.SetActive(false);
            once6 = false;
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

    public void Continue()
    {
        uiClick.Play();
        backgroundMusic.volume = musicVolume;
        ambientNoise.volume = ambientVolume;

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
        backgroundMusic.Stop();
        ambientNoise.Stop();
        playerDeath.Play();

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
        uiClick.Play();
        gameOver = false;
        paused = false;
        Time.timeScale = 1;

        Debug.Log("Retry");
        crosshair.SetActive(true);
        startImage.color = new Color(0, 0, 0, 1);
        startImage.gameObject.SetActive(true);
        SceneManager.LoadScene(sceneName);
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
   
    public void StopMusic()
    {
        backgroundMusic.Stop();
        ambientNoise.Play();
    }

    public void ControlVolume(bool increase)
    {
        uiClick.Play();

        if(increase && musicVolume <= 0.95)
        {
            musicVolume += 0.05f;
            volumeText.text = string.Format("{0:0.00}", musicVolume);
        }
        else if(musicVolume >= 0.05)
        {
            musicVolume -= 0.05f;
            volumeText.text = string.Format("{0:0.00}", musicVolume);
        }
    }

    public void OpenSettings()
    {
        uiClick.Play();
        pauseMenu.SetActive(false);
        pauseSettings.SetActive(true);
        settingsOpen = true;
    }

    public void Exit()
    {
        uiClick.Play();
        Debug.Log("Quit");
        Application.Quit();
    }

    public void DisplayCredits()
    {
        timeScreen.SetActive(false);
        deathScreen.SetActive(false);
        designScreen.SetActive(false);
        creditsScreen.SetActive(true);
    }

    public void HideCredits()
    {
        if(!gameOver)
            timeScreen.SetActive(true);
        else
            deathScreen.SetActive(true);

        designScreen.SetActive(true);
        creditsScreen.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (once1 && other.CompareTag("Player"))
        {
            gatesClose.Play();
            backgroundMusic.Play();
            ambientNoise.Stop();
            hornet.start = true;
            running = true;
            once1 = false;
        }

        if(!running && !once2 && other.CompareTag("Player") && once5)
        {
            ambientNoise.volume = pauseAmbientVolume;
            gameWon.Play();
            endScreen.SetActive(true);
            crosshair.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            timeSpent.text = timeTaken;
            once5 = false;
        }
    }
}
