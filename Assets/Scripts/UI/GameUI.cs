using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }
    public static CropDef SelectedCrop { get; private set; }

    public GameObject toolsDrawer;   // assign in Inspector
    public Toggle pesticideToggle;   // optional (only if you add it to ToolsDrawer)


    [Header("Phase 2 UI")]
    public Slider waterSlider; // 0..1
    public TMP_Text statsText;
    public SimulationManager sim; // drag in Inspector

    public void ToggleTools() { if (toolsDrawer) toolsDrawer.SetActive(!toolsDrawer.activeSelf); }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    // Hook this on your Wheat/Tomato buttons
    public void OnSelectCrop(CropDef def)
    {
        SelectedCrop = def;
        Debug.Log($"Selected crop: {def?.cropName}");
    }
    
        // Hook this to the Water slider OnValueChanged(float)
    public void OnWaterChanged(float v){
        foreach (var ps in FindObjectsOfType<PlotState>()) ps.irrigation = v;
        RefreshStats();
    }
    
    public void RefreshStats()
    {
        var plots = FindObjectsOfType<PlotState>();
        if (plots.Length == 0) { if (statsText) statsText.text = "No plots"; return; }

        int grown = 0, dead = 0;
        float avgM = 0f;
        foreach (var p in plots)
        {
            avgM += p.moisture;
            if (p.plot != null)
            {
                CropPlot.State st = CropPlot.State.Empty;

                if (sim != null) {
                    var mi = p.plot.GetType().GetMethod("GetState",
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

                    if (mi != null) {
                        var val = mi.Invoke(p.plot, null);   // could be null if method returns object
                        if (val != null) st = (CropPlot.State)val;
                        // if val is null, we keep st = Empty
                    }
                    // if mi is null, we keep st = Empty
                }
                if (st == CropPlot.State.Grown) grown++;
                else if (st == CropPlot.State.Dead) dead++;
            }
        }
        avgM /= plots.Length;
        int day = sim ? sim.dayCount : 0;
        if (statsText) statsText.text = $"Day: {day}\n" + $"Tiles: {plots.Length}\nAvg Moisture: {avgM:0.00}\nMature: {grown}\nDead: {dead}\nWater: {waterSlider?.value:0.00}";
    }

    // Optional buttons
    public void OnTogglePlay(){ sim?.TogglePlay(); }
    public void OnSpeed1(){ sim?.SetSpeed(1); }
    public void OnSpeed2(){ sim?.SetSpeed(2); }
    public void OnSpeed4(){ sim?.SetSpeed(4); }

}