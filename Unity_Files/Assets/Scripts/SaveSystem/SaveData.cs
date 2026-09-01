using System.Collections.Generic;
using UnityEngine;

// SaveData.cs
// What actually gets written to disk. Location + orientation + progress + collected items,
// exactly what the rubric asks for. [System.Serializable] + JsonUtility keeps this dependency-free.
[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public Vector3 playerEulerRotation;

    // Free-form progress marker - wire this up to whatever "progress" means for your demo
    // (e.g. number of collectibles found, or a simple stage/level counter).
    public int progressValue;

    public List<string> collectedItemIDs = new List<string>();
}
