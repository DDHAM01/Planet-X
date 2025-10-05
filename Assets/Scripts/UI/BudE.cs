using UnityEngine;
using TMPro;
using System.Reflection;

public class BudE : MonoBehaviour {
    public static BudE Instance;

    [Header("Visual States (UI Images or any UI GOs)")]
    public GameObject leaves, bud, flower;

    [Header("Tip UI")]
    public TextMeshProUGUI tipText;     // small TMP text on Canvas
    public float tipSeconds   = 2.5f;   // how long a tip stays
    public float tipCooldown  = 3.0f;   // min seconds between daily tips

    float tipTimer;
    float cooldownTimer;

    // quick tips (from UI actions like the water slider)
    [Header("Quick Tip Settings")]
    public float quickTipCooldown = 1.5f;
    float quickCooldownTimer;
    float lastWater = -1f; // remembers previous slider value for "big change" tips

    // --- Water guidance tuning ---
    const float TOL          = 0.12f; // target band half-width (±12%)
    const float MIN_MSG_LOW  = 0.03f; // only say "near zero" below this
    const float MAX_MSG_HIGH = 0.97f; // only say "maxed out" above this
    const float FAR          = 0.22f; // far from target
    const float SLIGHT       = TOL + 0.04f; // slight miss just outside band

    enum WaterBand { ExtremeLow, FarLow, SlightLow, OnTarget, SlightHigh, FarHigh, ExtremeHigh }

    // cache last daily signal so we don't repeat the same daily tip
    enum Signal { None, TooDry, TooWet, Good, HarvestSoon }
    Signal lastSignal = Signal.None;

    void Awake() {
        Instance = this;
        HideTip();
        SetPhase(0f); // start with Leaves visible
    }

    void Update() {
        // hide tip after a few seconds
        if (tipText && tipText.gameObject.activeSelf) {
            tipTimer -= Time.deltaTime;
            if (tipTimer <= 0f) HideTip();
        }
        if (cooldownTimer > 0f)      cooldownTimer -= Time.deltaTime;
        if (quickCooldownTimer > 0f) quickCooldownTimer -= Time.deltaTime;
    }

    // -------- public API: called from SimulationManager each day --------
    public void OnDailyUpdate(PlotState[] plots) {
        if (plots == null || plots.Length == 0) return;

        int mature = 0, dead = 0, tooDry = 0, tooWet = 0, inBand = 0, planted = 0;

        foreach (var p in plots) {
            if (p == null || p.cropDef == null) continue;
            planted++;
            var st = GetState(p.plot);
            if (st == CropPlot.State.Grown) mature++;
            if (st == CropPlot.State.Dead)  dead++;

            float m = p.moisture;
            float stress = Mathf.Abs(m - p.cropDef.baseWaterNeed);
            if (stress <= TOL) inBand++;
            if (m < 0.05f) tooDry++;
            if (m > 0.95f) tooWet++;
        }

        // If nothing is planted yet, guide the player and stop (don’t spam daily signals)
        if (planted == 0) {
            ShowTip("Plant a crop to get started — select Wheat/Tomato, then click plots.");
            return;
        }

        float maturePct = (float)mature / planted;
        SetPhase(maturePct);

        // choose ONE daily tip (priority order)
        Signal sig = Signal.None;
        if (tooDry > planted * 0.30f)      sig = Signal.TooDry;
        else if (tooWet > planted * 0.20f) sig = Signal.TooWet;
        else if (maturePct >= 0.70f)       sig = Signal.HarvestSoon;
        else if (inBand > planted * 0.50f) sig = Signal.Good;

        MaybeShowDailyTip(sig);
    }

    // -------- public API: called from GameUI slider instantly --------
    public void NotifyWaterLevel(float water01, CropDef currentCrop) {
        if (quickCooldownTimer > 0f) return; // throttle quick tips

        bool bigChange = (lastWater >= 0f) && (Mathf.Abs(water01 - lastWater) >= 0.28f);
        lastWater = water01;

        string msg = null;

        // Extreme absolute bounds (rare, only at the edges)
        if (water01 <= MIN_MSG_LOW)         msg = "Water is extremely low — seedlings may wilt.";
        else if (water01 >= MAX_MSG_HIGH)   msg = "Water is maxed out — high risk of root rot.";
        else if (currentCrop != null) {
            // Crop-aware message
            float target = currentCrop.baseWaterNeed;   // 0..1
            float delta  = water01 - target;            // negative = low, positive = high
            float ad     = Mathf.Abs(delta);

            WaterBand band =
                (ad <= TOL)    ? WaterBand.OnTarget :
                (ad <= SLIGHT) ? (delta < 0 ? WaterBand.SlightLow  : WaterBand.SlightHigh) :
                (ad <= FAR)    ? (delta < 0 ? WaterBand.FarLow     : WaterBand.FarHigh) :
                                 (delta < 0 ? WaterBand.ExtremeLow : WaterBand.ExtremeHigh);

            switch (band) {
                case WaterBand.OnTarget:
                    msg = $"Good: near {currentCrop.cropName} target ({target:0.35} ± {TOL:0.45})."; break;
                case WaterBand.SlightLow:
                    msg = $"Slightly low: nudge water up toward {target:0.20}."; break;
                case WaterBand.FarLow:
                    msg = $"Too low: raise water closer to {target:0.00}."; break;
                case WaterBand.ExtremeLow:
                    msg = $"Much too low: increase water — plants will stall."; break;
                case WaterBand.SlightHigh:
                    msg = $"Slightly high: lower water toward {target:0.75}."; break;
                case WaterBand.FarHigh:
                    msg = $"Too high: reduce water closer to {target:1.00}."; break;
                case WaterBand.ExtremeHigh:
                    msg = $"Much too high: drain water — risk of root rot."; break;
            }
        } else {
            // No crop selected → generic guidance
            if      (water01 < 0.15f) msg = "Water is quite low — pick a crop and adjust toward its target.";
            else if (water01 > 0.85f) msg = "Water is quite high — pick a crop and adjust toward its target.";
            else                      msg = "Pick a crop to see its target water band.";
        }

        if (msg == null && bigChange) msg = "Big water change — watch plants for stress.";

        if (msg != null) {
            ShowTip(msg);
            quickCooldownTimer = quickTipCooldown;
        }
    }

    // -------- visuals --------
    void SetPhase(float maturePct) {
        if (leaves) leaves.SetActive(maturePct < 0.3f);
        if (bud)    bud.SetActive(maturePct >= 0.3f && maturePct < 0.7f);
        if (flower) flower.SetActive(maturePct >= 0.7f);
    }

    // -------- daily tips (cooldown + de-dupe) --------
    void MaybeShowDailyTip(Signal sig) {
        if (sig == Signal.None) return;
        if (sig == lastSignal && cooldownTimer > 0f) return;
        lastSignal = sig;
        cooldownTimer = tipCooldown;

        switch (sig) {
            case Signal.TooDry:      ShowTip("Many plots are too dry — raise water a bit."); break;
            case Signal.TooWet:      ShowTip("Many plots are too wet — lower water slightly."); break;
            case Signal.Good:        ShowTip("Most plots are in the target band — keep it steady."); break;
            case Signal.HarvestSoon: ShowTip("Most crops are mature — consider replanting to experiment."); break;
        }
    }

    void ShowTip(string msg) {
        if (!tipText) { Debug.Log($"Bud-E: {msg}"); return; }
        tipText.text = msg;
        tipText.gameObject.SetActive(true);
        tipTimer = tipSeconds;
    }

    void HideTip() { if (tipText) tipText.gameObject.SetActive(false); }

    // -------- safe state access (no need to change CropPlot) --------
    private CropPlot.State GetState(CropPlot cp) {
        if (cp == null) return CropPlot.State.Empty;

        var mi = cp.GetType().GetMethod("GetState",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (mi != null) {
            var val = mi.Invoke(cp, null);
            if (val != null) return (CropPlot.State)val;
        }
        var fi = cp.GetType().GetField("state",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (fi != null) {
            var v = fi.GetValue(cp);
            if (v != null) return (CropPlot.State)v;
        }
        return CropPlot.State.Empty;
    }
}
