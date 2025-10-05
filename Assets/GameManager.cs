using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Elements")]
    public Text crisisText;
    public Text scoreText;
    public Text timerText;
    public GameObject crisisAlertPanel;
    public Image crisisImageDisplay;

    [Header("Region System")]
    public GameObject regionsPanel; // Parent of all region buttons
    public GameObject answersPanel; // Parent of all answer buttons
    public List<GameObject> regionAnswerSets; // CVQ, CQ, IQ, Crops etc.

    [Header("Game Settings")]
    public int score = 0;
    public float gameTime = 60f;
    public float crisisInterval = 12f;

    [Header("Pause System")]
    public Button pauseButton;
    public GameObject pausePanel; //panel showing "PAUSED" text
    public Text pauseButtonText;

    [Header("Dynamic Timer Settings")]
    public float startingCrisisTime = 15f; // Starting time between crises
    public float minimumCrisisTime = 5f;   // Never faster than this
    public float timeDecreaseRate = 0.5f;  // How much time decreases per crisis
    public float currentCrisisInterval;    // Current time between crises

    [Header("Crisis Images")]
    public Sprite droughtImage;
    public Sprite pestImage;
    public Sprite floodImage;
    public Sprite frostImage;

    public Sprite dryTomatoImage;
    public Sprite coldImage;
    public Sprite locustImage;
    public Sprite drownImage;
    public Sprite salintyImage;


    private Crisis currentCrisis;
    private bool gameActive = true;
    private int crisesSolved = 0;
    private bool waitingForRegionSelection = false;
    private bool isPaused = false;
    private Coroutine gameTimerCoroutine;
    private Coroutine crisisSpawnerCoroutine;

    private List<Crisis> allCrises;

    void Awake() { instance = this; }

    void Start()
    {
        currentCrisisInterval = startingCrisisTime;
        InitializeCrises();
        UpdateScore();
        ShowRegionsPanel(false);
        ShowAnswersPanel(false);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        StartCoroutine(GameTimer());
        StartCoroutine(CrisisSpawner());
        UpdateTimerDisplay();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
        
        UpdatePauseUI();
    }

    void PauseGame()
    {
        Time.timeScale = 0f; // This pauses all time-based operations
        
        // Optional: Show pause panel
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
        Debug.Log("Game Paused");
    }
    
    void ResumeGame()
    {
        Time.timeScale = 1f; // Resume normal time
        
        // Hide pause panel
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        Debug.Log("Game Resumed");
    }
    
    void UpdatePauseUI()
    {
        // Update button text
        if (pauseButtonText != null)
        {
            pauseButtonText.text = isPaused ? "Resume" : "Pause";
        }
        
        // You could also change the button color or add other visual feedback
    }

    void InitializeCrises()
    {
        allCrises = new List<Crisis>()
        {
            new Crisis("Drought", "Soil moisture critical! Crops wilting.", "smart_irrigation", "NASA SMAP Soil Moisture Data", droughtImage, "CV"),
            new Crisis("Pest", "Unusual crop stress patterns detected.", "drone_spraying", "Landsat Vegetation Indices", pestImage, "CV"),
            new Crisis("Flood", "Heavy rainfall predicted for region.", "drainage_system", "GPM Rainfall Data", floodImage, "CV"),
            new Crisis("Frost", "Unexpected cold front moving in.", "frost_fans", "MODIS Land Surface Temp", frostImage, "CV"),

            new Crisis("Drought", "The Central Valley is in a drought and cannot grow its usual crops, what to do?", "Suggest_Crop_rotation", "NASA SMAP Soil Moisture Data", droughtImage, "C"),
            new Crisis("Pest", "Rising temperatures are drying out topsoil", "Find_soil_management_practices_that_hold_moisture_longer", "Landsat Vegetation Indices", pestImage, "C"),
            new Crisis("Flood", "Crops are losing fertilizer to irrigation", "drip_irrigation_fertigation", "GPM Rainfall Data", floodImage, "C"),
            new Crisis("Frost", "Our lettuce crops have too much nitrate runoff", "Apply_fertilizer_in_smaller_split_doses", "MODIS Land Surface Temp", frostImage, "C"),

            new Crisis("Drought", "Plants on one end of our drip sprinkler system are flooding, while others are left thirsty, why?", "Drip_Irrigation_Line_Has_uneven_output", "NASA SMAP Soil Moisture Data", droughtImage, "I"),
            new Crisis("Pest", "Vineyard Sprinklers use 30% more water than expected on hot days, how to solve?", "irrigation_scheduling_system", "Landsat Vegetation Indices", pestImage, "I"),
            new Crisis("Flood", "Tomato crop shows leaf burn and reduced growth, why?", "Salinity_Build_Up", "GPM Rainfall Data", salintyImage, "I"),
            new Crisis("Frost", "Our crops don't appear to be recieving any water", "make_sure_irrigation_system_is_operating", "MODIS Land Surface Temp", frostImage, "I"),

            new Crisis("Tomato", "Tomato crops are shriveling up!.", "Increase_Water_Supply", "NASA SMAP Soil Moisture Data", dryTomatoImage, "Crops"),
            new Crisis("Wheat", "Unusual crop stress patterns detected.  Insects?", "Drone_Spraying", "Landsat Vegetation Indices", locustImage, "Crops"),
            new Crisis("Cauliflower", "Heavy rainfall predicted for region.", "Drainage_System", "GPM Rainfall Data", drownImage, "Crops"),
            new Crisis("Apple", "Unexpected cold front moving in. What to do?", "Frost_Fans", "MODIS Land Surface Temp", coldImage, "Crops")

        };
    }

    IEnumerator GameTimer()
    {
        while (gameTime > 0 && gameActive)
        {
            if (!isPaused) // Only count down when not paused
            {
                gameTime -= Time.deltaTime;
                timerText.text = "Time: " + Mathf.RoundToInt(gameTime);
            }
            yield return null;
        }
        if (gameActive) // Only end game if it wasn't paused at game over
        {
            gameActive = false;
            crisisAlertPanel.SetActive(false);
            crisisImageDisplay.gameObject.SetActive(false);
            ShowRegionsPanel(false);
            ShowAnswersPanel(false);
            Debug.Log("Game Over! Final Score: " + score);
        }
    }

    IEnumerator CrisisSpawner()
    {
        while (gameActive)
        {
            while(gameActive)
        {
            float waitTime = currentCrisisInterval;
            
            // Wait for the interval, but account for pause time
            while (waitTime > 0)
            {
                if (!isPaused)
                {
                    waitTime -= Time.deltaTime;
                }
                yield return null;
            }
            
            if(gameActive && !isPaused) 
            {
                TriggerRandomCrisis();
            }
        }
        }
    }

    void TriggerRandomCrisis()
    {
        currentCrisis = allCrises[Random.Range(0, allCrises.Count)];

        // PHASE 1: Show region selection
        crisisText.text = $"There is a crisis in {currentCrisis.region} region!";
        crisisAlertPanel.SetActive(true);
        crisisImageDisplay.gameObject.SetActive(false); // No image yet
        waitingForRegionSelection = true;

        // Show region buttons, hide answer buttons
        ShowRegionsPanel(true);
        ShowAnswersPanel(false);

        Debug.Log($"New crisis in {currentCrisis.region} region: {currentCrisis.type}");
    }

    public void OnRegionSelected(string selectedRegion)
    {
        if (!waitingForRegionSelection || !gameActive || isPaused) return;

        // Check if correct region was selected
        if (selectedRegion == currentCrisis.region)
        {
            // Correct region - proceed to question phase
            waitingForRegionSelection = false;
            crisisText.text = currentCrisis.message;

            // Show crisis image
            if (currentCrisis.crisisImage != null)
            {
                crisisImageDisplay.sprite = currentCrisis.crisisImage;
                crisisImageDisplay.gameObject.SetActive(true);
            }

            // Show answers for this region, hide region buttons
            ShowRegionsPanel(false);
            ShowAnswerSetForRegion(selectedRegion);
            ShowAnswersPanel(true);
        }
        else
        {
            // Wrong region - penalty
            score -= 25;
            UpdateScore();
            Debug.Log($"Wrong region! Selected: {selectedRegion}, Correct: {currentCrisis.region}");
            // Optional: Show error message or sound
        }
    }

    void ShowAnswerSetForRegion(string region)
    {
        // Hide all answer sets first
        foreach (var answerSet in regionAnswerSets)
        {
            answerSet.SetActive(false);
        }

        // Find and show the correct answer set
        GameObject targetSet = regionAnswerSets.Find(set => set.name == region);
        if (targetSet != null)
        {
            targetSet.SetActive(true);
        }
        else
        {
            Debug.LogError($"No answer set found for region: {region}");
        }
    }

    public void OnSolutionSelected(string solution)
    {
        if(!gameActive || waitingForRegionSelection || isPaused) return;

        crisisAlertPanel.SetActive(false);
        crisisImageDisplay.gameObject.SetActive(false); // Hide image when answer selected
        ShowAnswersPanel(false);

        if (solution == currentCrisis.correctSolution)
        {
            score += 100;
            crisesSolved++;
            DecreaseCrisisInterval();
            // Play success sound
        }
        else
        {
            score -= 50;
            // Play error sound
        }

        UpdateScore();
    }
    void DecreaseCrisisInterval()
    {
        // Calculate new interval (never go below minimum)
        float newInterval = Mathf.Max(
            startingCrisisTime - (crisesSolved * timeDecreaseRate),
            minimumCrisisTime
        );

        // Only update if it actually changed
        if (newInterval != currentCrisisInterval)
        {
            currentCrisisInterval = newInterval;
            UpdateTimerDisplay();
            Debug.Log($"Crisis interval decreased to: {currentCrisisInterval:F1}s");
        }
    }

    void UpdateTimerDisplay()
    {
        // add a separate UI element for this or modify existing timer
        // For now just log it or add a new text element
        Debug.Log($"Next crisis in: {currentCrisisInterval:F1}s");

        // Optional: Add a UI text element to show the current speed
        // speedText.text = $"Speed: {currentCrisisInterval:F1}s";
    }
    void UpdateScore()
    {
        scoreText.text = "Score: " + score;
    }
    
    void ShowRegionsPanel(bool show)
    {
        if (regionsPanel != null)
            regionsPanel.SetActive(show);
    }
    
    void ShowAnswersPanel(bool show)
    {
        if (answersPanel != null)
            answersPanel.SetActive(show);
    }
}