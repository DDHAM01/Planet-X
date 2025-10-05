using UnityEngine;

public class PlotState : MonoBehaviour
{
    [Header("Link your tile's crop sprite here")]
    public CropPlot plot;
    public SpriteRenderer cropRenderer;

    [Header("Runtime")]
    [Range(0, 1)] public float moisture = 0.5f;
    [Range(0, 1)] public float irrigation = 0.0f;
    public int consecutiveBadDays = 0;   // add this (tracks lethal drought/overwater streak)
    public CropDef cropDef;
    public float ageDays;
    [Range(0, 1)] public float health = 1f;

    void Awake()
    {
        if (plot == null) plot = GetComponent<CropPlot>();
        if (!plot) plot = GetComponent<CropPlot>();
    }
    public CropInstance crop;

    // Called when the tile is planted (see tiny patch in CropPlot)
    public void OnPlanted(CropDef def)
    {
        cropDef = def;
        ageDays = 0f;
        health = 1f;
        // keep moisture as-is; player water will drive outcomes
    }

    public void Plant(CropDef def)
    {
        if (crop != null) Destroy(crop);
        crop = gameObject.AddComponent<CropInstance>();
        crop.Setup(def, this);
    }

    void OnMouseDown()
    {
        if (GameUI.SelectedCrop != null) Plant(GameUI.SelectedCrop);
    }
    
        // Optional: clear when tile is reset to empty
    public void OnCleared(){
        cropDef = null;
        ageDays = 0f;
        health  = 1f;
    }

}