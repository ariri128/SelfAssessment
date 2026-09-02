using System.Collections.Generic;
using UnityEngine;

/*
   What actually gets written to disk: location + orientation + progress + collected items
   [System.Serializable] + JsonUtility keeps this dependency-free
*/
[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public Vector3 playerEulerRotation;

    public int progressValue;

    public List<string> collectedItemIDs = new List<string>();
}
