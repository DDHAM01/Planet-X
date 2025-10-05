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
    public Image crisisImageDisplay; // Add this - drag your Image UI element here
    
    [Header("Game Settings")]
    public int score = 0;
    public float gameTime = 60f;
    public float crisisInterval = 12f;
    
    [Header("Crisis Images")]
    public Sprite droughtImage;
    public Sprite pestImage;
    public Sprite floodImage;
    public Sprite frostImage;
    // Add more as needed
    
    private Crisis currentCrisis;
    private bool gameActive = true;
    
    // Crisis database - now with images!
    private List<Crisis> allCrises;

    void Awake() { instance = this; }
    
    void Start()
    {
        InitializeCrises();
        UpdateScore();
        StartCoroutine(GameTimer());
        StartCoroutine(CrisisSpawner());
    }
    
    void InitializeCrises()
    {
        allCrises = new List<Crisis>()
        {
            new Crisis("Drought", "Soil moisture critical! Crops wilting.", "smart_irrigation", "NASA SMAP Soil Moisture Data", droughtImage),
            new Crisis("Pest", "Unusual crop stress patterns detected.", "drone_spraying", "Landsat Vegetation Indices", pestImage),
            new Crisis("Flood", "Heavy rainfall predicted for region.", "drainage_system", "GPM Rainfall Data", floodImage),
            new Crisis("Frost", "Unexpected cold front moving in.", "frost_fans", "MODIS Land Surface Temp", frostImage)
            // Add more crises with their respective images
        };
    }
    
    IEnumerator GameTimer()
    {
        while(gameTime > 0 && gameActive)
        {
            gameTime -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.RoundToInt(gameTime);
            yield return null;
        }
        gameActive = false;
        crisisAlertPanel.SetActive(false);
        // Game over logic here
    }
    
    IEnumerator CrisisSpawner()
    {
        while(gameActive)
        {
            yield return new WaitForSeconds(crisisInterval);
            if(gameActive) TriggerRandomCrisis();
        }
    }
    
    void TriggerRandomCrisis()
    {
        currentCrisis = allCrises[Random.Range(0, allCrises.Count)];
        crisisText.text = currentCrisis.message;
        
        // Set the crisis image - ADD THIS SECTION
        if(currentCrisis.crisisImage != null)
        {
            crisisImageDisplay.sprite = currentCrisis.crisisImage;
            crisisImageDisplay.gameObject.SetActive(true);
        }
        else
        {
            crisisImageDisplay.gameObject.SetActive(false);
        }
        
        crisisAlertPanel.SetActive(true);
    }
    
    public void OnSolutionSelected(string solution)
    {
        if(!gameActive) return;
        
        crisisAlertPanel.SetActive(false);
        crisisImageDisplay.gameObject.SetActive(false); // Hide image when answer selected
        
        if(solution == currentCrisis.correctSolution)
        {
            score += 100;
            // Play success sound
        }
        else
        {
            score -= 50;
            // Play error sound
        }
        
        UpdateScore();
    }
    
    void UpdateScore()
    {
        scoreText.text = "Score: " + score;
    }
}