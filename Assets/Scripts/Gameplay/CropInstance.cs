using UnityEngine;

public enum CropState { Seed, Growing, Mature, Dead }

public class CropInstance : MonoBehaviour {
    public CropDef def;
    public PlotState plot;
    public CropState state = CropState.Seed;
    public float ageDays;
    public float health = 1f; // 0..1

    public void Setup(CropDef d, PlotState p){
        def = d; plot = p;
        state = CropState.Seed;
        ageDays = 0f; health = 1f;

        if (plot.cropRenderer){
            plot.cropRenderer.sprite = def.seedSprite;
            plot.cropRenderer.color  = Color.white;
        }
    }
}