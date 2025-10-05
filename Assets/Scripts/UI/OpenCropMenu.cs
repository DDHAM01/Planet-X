using UnityEngine;

public class OpenCropMenu : MonoBehaviour
{
    public GameObject CM;
    void Start()
    {
        CM.SetActive(false);
    }
    public void OpenCrops()
    {
        if (CM != null && !CM.activeSelf)
        {
            CM.SetActive(true);
            return;
        }
        else if (CM != null && CM.activeSelf)
        {
            CM.SetActive(false);
            return;
        }

    }
    public void CloseCrops()
    {
        if (CM != null && CM.activeSelf)
        {
            CM.SetActive(false);
            return;
        }
    }
}
