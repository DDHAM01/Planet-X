using UnityEngine;
using TMPro;
using System.Reflection;

public class BudE : MonoBehaviour {
    public static BudE Instance;

    [Header("Visual States (UI Images or any UI GOs)")]
    public GameObject leaves, bud, flower;

    [Header("Tip UI")]
    public TextMeshProUGUI tipText;   // put a small TMP text on Canvas
    public float tipSeconds = 2.5f;   // how long a tip stays
    public float tipCooldown = 3.0f;  // min seconds between tips

    float tipTimer;
    float cooldownTimer;

    // cache last signal so we don't repeat the same tip
    enum Signal { None, TooDry, TooWet, Good, HarvestSoon }
    Signal lastSignal = Signal.None;

    void Awake() {
        Instance = this;
        HideTip();
        SetPhase(0f); // start with Leaves visible
    }

    void Update() {
        if (tipText && tipText.gameObject.activeSelf) {
            tipTimer -= Time.deltaTime;
            if (tipTimer <= 0f) HideTip();
        }
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    // -------- public API called from SimulationManager each day --------
    public void OnDailyUpdate(PlotState[] plots) {
        if (plots == null || plots.Length == 0) return;

        int mature = 0, dead = 0, tooDry = 0, tooWet = 0, inBand = 0, planted = 0;
        const float tol = 0.12f; // moisture tolerance band

        foreach (var p in plots) {
            if (p == null || p.cropDef == null) continue;
            planted++;
            var st = GetState(p.plot);
            if (st == CropPlot.State.Grown) mature++;
            if (st == CropPlot.State.Dead)  dead++;

            float m = p.moisture;
            float stress = Mathf.Abs(m - p.cropDef.baseWaterNeed);
            if (stress <= tol) inBand++;
            if (m < 0.05f) tooDry++;
            if (m > 0.95f) tooWet++;
        }

        float maturePct = planted > 0 ? (float)mature / planted : 0f;
        SetPhase(maturePct);

        // choose ONE tip per day (priority order)
        Signal sig = Signal.None;
        if (planted > 0) {
            if (tooDry > planted * 0.30f) sig = Signal.TooDry;
            else if (tooWet > planted * 0.20f) sig = Signal.TooWet;
            else if (maturePct >= 0.70f) sig = Signal.HarvestSoon;
            else if (inBand > planted * 0.50f) sig = Signal.Good;
        }

        MaybeShowTip(sig);
    }

    // -------- visuals --------
    void SetPhase(float maturePct) {
        if (leaves) leaves.SetActive(maturePct < 0.3f);
        if (bud)    bud.SetActive(maturePct >= 0.3f && maturePct < 0.7f);
        if (flower) flower.SetActive(maturePct >= 0.7f);
    }

    // -------- tips --------
    void MaybeShowTip(Signal sig) {
        if (sig == Signal.None) return;
        if (sig == lastSignal && cooldownTimer > 0f) return; // same signal, still cooling down
        lastSignal = sig;
        cooldownTimer = tipCooldown;

        switch (sig) {
            case Signal.TooDry:     ShowTip("Many plots are wilting — bump water up a bit."); break;
            case Signal.TooWet:     ShowTip("Overwatering risk — lower water to save roots."); break;
            case Signal.Good:       ShowTip("Nice — most plots are in the sweet spot!"); break;
            case Signal.HarvestSoon:ShowTip("Looking great — you could replant soon to keep learning."); break;
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
