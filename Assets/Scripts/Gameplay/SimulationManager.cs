using UnityEngine;
using System.Linq;

public class SimulationManager : MonoBehaviour {
    [Header("Time")]
    public float secondsPerDay = 0.5f;
    public int dayCount = 0;
    public bool running = true;
    public int speed = 1; // 1x, 2x, 4x...

    [Header("Environment")]
    [Range(0,1)] public float ambientTemp01 = 0.6f; // for future
    public float et0 = 0.25f;     // evapotranspiration per day
    public float infil = 0.6f;    // how much irrigation adds per day
float acc;
    PlotState[] plots;

    void Start(){ plots = FindObjectsOfType<PlotState>(); EnsurePlotsCached();}

    void Update(){
        if (!running) return;
        acc += Time.deltaTime * Mathf.Max(1, speed);
        while (acc >= secondsPerDay){
            StepDay();
            acc -= secondsPerDay;
        }
    }

    void StepDay(){
        foreach (var ps in plots){
            // Moisture balance
            float add  = ps.irrigation * infil;
            float loss = et0 * 0.6f; // simple; tuned to feel right
            ps.moisture = Mathf.Clamp01(ps.moisture + add - loss);

            // Crop growth/health only if planted
            if (ps.cropDef != null && ps.plot != null){
                // if tile got cleared to Empty, ignore until re-planted
                // (CropPlot drives visible state; we drive conditions)
                var stateField = GetState(ps.plot); // local helper below

                if (stateField == CropPlot.State.Sprout || stateField == CropPlot.State.Grown){
                    float target = ps.cropDef.baseWaterNeed;   // 0..1
                    float dist   = Mathf.Abs(ps.moisture - target);

                    // health loss if too far from target moisture
                    ps.health -= Mathf.Clamp01(dist * 0.06f);
                    ps.health = Mathf.Clamp01(ps.health);

                    // growth factor: best near target moisture
                    float growthFactor = Mathf.Lerp(0.3f, 1f, 1f - Mathf.Clamp01(dist*2f));
                    ps.ageDays += growthFactor;

                    // transitions
                    if (ps.health <= 0.0f && stateField != CropPlot.State.Dead){
                        ps.plot.Kill();
                    }
                    else if (ps.ageDays >= ps.cropDef.daysToMature && stateField == CropPlot.State.Sprout){
                        ps.plot.Grow(); // your existing method swaps to mature sprite
                    }
                }
            }
        }
    dayCount++;

    BudE.Instance?.OnDailyUpdate(plots);

    GameUI.Instance?.RefreshStats();
    }
    public void ResetDays(){ dayCount = 0; }

    // Read the private 'state' via a public method if you have one.
    // If not, add this small helper in CropPlot: public CropPlot.State GetState() => state;
    CropPlot.State GetState(CropPlot cp){
        // Implement properly if you expose a getter. For now, assume a public field or method exists:
        return (CropPlot.State)cp.GetType().GetField("state", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cp);
    }
    void EnsurePlotsCached() {
    if (plots == null || plots.Length == 0) {
        plots = FindObjectsOfType<PlotState>();
    }
}
    // Metrics for scoring later:
    public float YieldRate()  => plots.Count(p => p.plot && IsState(p.plot, CropPlot.State.Grown)) /(float)plots.Length;
    public float DeathsRate() => plots.Count(p => p.plot && IsState(p.plot, CropPlot.State.Dead))  /(float)plots.Length;
    public float WaterUseNorm()=> plots.Average(p => p.irrigation);

    bool IsState(CropPlot cp, CropPlot.State target){
        var s = GetState(cp);
        return s == target;
    }

    // UI hooks
    public void TogglePlay(){ running = !running; }
    public void SetSpeed(int s){ speed = Mathf.Clamp(s, 1, 8); }
}