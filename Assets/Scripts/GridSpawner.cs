using UnityEngine;

public class GridSpawner : MonoBehaviour {
    public GameObject plotPrefab;
    public int rows = 3;
    public int cols = 3;
    public float cellSize = 1.2f;  // spacing between plots

    void Start() {
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                var pos = new Vector3(c * cellSize, -r * cellSize, 0); // top-left to bottom-right
                var plot = Instantiate(plotPrefab, transform);
                plot.transform.localPosition = pos;
            }
        }
        // Optional: center the grid around (0,0)
        var offset = new Vector3((cols-1)*0.5f*cellSize, -(rows-1)*0.5f*cellSize, 0);
        foreach (Transform child in transform) child.localPosition -= offset;
    }
}
