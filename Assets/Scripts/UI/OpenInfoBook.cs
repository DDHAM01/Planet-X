using UnityEngine;

public class OpenInfoBook : MonoBehaviour
{
    public GameObject IB;
    void Start()
    {
        IB.SetActive(false);
    }
    public void OpenBook()
    {
        Debug.Log("Open Information Book");
        IB.SetActive(true);
    }
}
