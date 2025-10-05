using UnityEngine;

public class CloseInfoBook : MonoBehaviour
{
    public GameObject IB;

    public void CloseBook()
    {
        IB.SetActive(false);
    }
}
