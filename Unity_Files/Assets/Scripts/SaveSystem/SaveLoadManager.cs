using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
   R saves, T loads
   Uses Application.persistentDataPath so it works the same in the editor and in a built player
*/
public class SaveLoadManager : MonoBehaviour
{
    [Tooltip("Wire this up to something meaningful for your demo - e.g. increment it whenever a collectible is grabbed.")]
    public int progressValue = 0;

    HashSet<string> collectedItems = new HashSet<string>();

    string SavePath => Path.Combine(Application.persistentDataPath, "selfassessment_save.json");

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) SaveGame();
        if (Input.GetKeyDown(KeyCode.T)) LoadGame();
    }

    public void RegisterCollected(string itemID)
    {
        if (!string.IsNullOrEmpty(itemID))
        {
            collectedItems.Add(itemID);
            progressValue = collectedItems.Count;
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            playerPosition = transform.position,
            playerEulerRotation = transform.eulerAngles,
            progressValue = progressValue,
            collectedItemIDs = new List<string>(collectedItems)
        };

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log($"[Save] Saved to {SavePath} (progress={progressValue}, items={collectedItems.Count})");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[Load] No save file found.");
            return;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));

        CharacterController cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        transform.position = data.playerPosition;
        transform.eulerAngles = data.playerEulerRotation;

        if (cc) cc.enabled = true;

        progressValue = data.progressValue;
        collectedItems = new HashSet<string>(data.collectedItemIDs);
        ApplyLoadedCollectedState();

        Debug.Log($"[Load] Restored from {SavePath} (progress={progressValue}, items={collectedItems.Count})");
    }

    void ApplyLoadedCollectedState()
    {
        foreach (CollectibleItem item in FindObjectsOfType<CollectibleItem>(true))
        {
            item.SetCollectedSilently(collectedItems.Contains(item.itemID));
        }
    }
}
