using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class CropPlot : MonoBehaviour, IPointerClickHandler
{
    public enum State { Empty, Sprout, Grown, Dead }

    [Header("Stats")]
    bool Occupied;
    string Name;
    int DayTilHarvest;

    public Sprite emptySprite, sproutSprite, grownSprite, deadSprite;

    private State state = State.Empty;
    private SpriteRenderer sr;

    //remember which crop this tile currently uses
    private CropDef activeCrop;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = emptySprite;
    }

    // New Input System "click"
    public void OnPointerClick(PointerEventData eventData)
    {
        // cycle through states on click (same logic as before)
        if (state == State.Empty)
        {
            Plant();
            Debug.Log("Planted");
        }
        else if (state == State.Sprout)
        {
            Grow();
            Debug.Log("Grew to mature");
        }
        else if (state == State.Grown)
        {
            Kill();
            Debug.Log("Marked as dead");
        }
        else
        {
            Clear();
            Debug.Log("Cleared to empty");
        }
    }

    public void Plant()
    {
        // pull the current selection (if any) and swap sprites for this tile
        if (GameUI.SelectedCrop != null) ApplyCrop(GameUI.SelectedCrop);

        state = State.Sprout;
        sr.sprite = sproutSprite;

        // NEW: tell PlotState which crop got planted
        GetComponent<PlotState>()?.OnPlanted(GameUI.SelectedCrop);
          // Notify PlotState (won’t crash if missing)
        var ps = GetComponent<PlotState>();
        if (ps) ps.OnPlanted(GameUI.SelectedCrop);
    }

    public void Grow()
    {
        if (state == State.Sprout)
        {
            state = State.Grown;
            sr.sprite = grownSprite;
        }
    }

    public void Kill()
    {
        state = State.Dead;
        sr.sprite = deadSprite;
    }

    public void Clear()
    {
        state = State.Empty;
        sr.sprite = emptySprite;
        activeCrop = null;
        // NEW: reset PlotState
        GetComponent<PlotState>()?.OnCleared();
        var ps = GetComponent<PlotState>();
        if (ps) ps.OnCleared();
    }
    
    // NEW: copy sprites from the selected CropDef into this tile's local refs
    private void ApplyCrop(CropDef def) {
        activeCrop = def;
        if (def.emptySprite)  emptySprite  = def.emptySprite;
        if (def.seedSprite) sproutSprite = def.seedSprite;
        if (def.matureSprite)  grownSprite  = def.matureSprite;
        if (def.deadSprite)   deadSprite   = def.deadSprite;
    }
}
