using UnityEngine;
[System.Serializable]
public class Crisis
{
    public string type;
    public string message;
    public string correctSolution;
    public string nasaDataReference;
    public Sprite crisisImage;
    public string region;

    public Crisis(string t, string m, string sol, string nasaRef, Sprite img = null, string reg = "")
    {
        type = t;
        message = m;
        correctSolution = sol;
        nasaDataReference = nasaRef;
        crisisImage = img;
        region = reg;
    }
}