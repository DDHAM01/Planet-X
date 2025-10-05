using UnityEngine;

[CreateAssetMenu(menuName="Defs/Crop")]
public class CropDef : ScriptableObject
{
     public string cropName;
    [TextArea] public string description;
    public Sprite emptySprite;
    public Sprite seedSprite;
    public Sprite growingSprite;
    public Sprite matureSprite;
    public Sprite deadSprite;

    [Header("Basic growth params")]
    [Range(0,1)] public float baseWaterNeed = 0.35f; // target moisture
    public float daysToMature = 20f;
    public AnimationCurve growthVsMoisture = AnimationCurve.Linear(0,0,1,1);
    [Range(0,1)] public float heatTolerance = 0.7f;
}
