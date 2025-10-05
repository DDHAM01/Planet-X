using UnityEngine;
[System.Serializable]
public class Crisis
{
    public string type;
    public string message;
    public string correctSolution;
    public string nasaDataReference;
    public Sprite crisisImage; // Add this line
    
    public Crisis(string t, string m, string sol, string nasaRef, Sprite img = null)
    {
        type = t;
        message = m;
        correctSolution = sol;
        nasaDataReference = nasaRef;
        crisisImage = img; // Add this line
    }
}