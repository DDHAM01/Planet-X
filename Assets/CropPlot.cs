using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class CropPlot : MonoBehaviour, IPointerClickHandler
{
    public enum State { Empty, Sprout, Grown, Dead }
    public Sprite emptySprite, sproutSprite, grownSprite, deadSprite;

    private State state = State.Empty;
    private SpriteRenderer sr;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = emptySprite;
    }

    // New Input System "click"
    public void OnPointerClick(PointerEventData eventData) {
        // cycle through states on click (same logic as before)
        if (state == State.Empty) {
            Plant();
            Debug.Log("Planted");
        }
        else if (state == State.Sprout) {
            Grow();
            Debug.Log("Grew to mature");
        }
        else if (state == State.Grown) {
            Kill();
            Debug.Log("Marked as dead");
        }
        else {
            Clear();
            Debug.Log("Cleared to empty");
        }
    }

    public void Plant() {
        state = State.Sprout; 
        sr.sprite = sproutSprite;
    }

    public void Grow() {
        if (state == State.Sprout) {
            state = State.Grown; 
            sr.sprite = grownSprite;
        }
    }

    public void Kill() {
        state = State.Dead; 
        sr.sprite = deadSprite;
    }

    public void Clear() {
        state = State.Empty; 
        sr.sprite = emptySprite;
    }
}
