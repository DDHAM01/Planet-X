using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "PlanBScene";
    [SerializeField] private int targetSceneIndex = 1;

    void Start()
    {
        // Get the Button component and add listener
        Button button = GetComponent<Button>();
        button.onClick.AddListener(ChangeScene);
    }

    public void ChangeScene()
    {
        // You can use either method:
        SceneManager.LoadScene(targetSceneName);
        // OR
        // SceneManager.LoadScene(targetSceneIndex);
    }
}