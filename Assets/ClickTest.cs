using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class PointerClickTest : MonoBehaviour, IPointerClickHandler {
    public void OnPointerClick(PointerEventData e) {
        Debug.Log("POINTER CLICK: " + name);
    }
}
