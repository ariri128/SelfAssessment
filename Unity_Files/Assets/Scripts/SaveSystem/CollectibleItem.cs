using UnityEngine;

// CollectibleItem.cs
// A world pickup with a stable, unique ID (set per-instance in the inspector). The save
// system records which IDs have been collected so a reload can hide the right ones.
// SETUP: give this GameObject a Collider with "Is Trigger" checked.
public class CollectibleItem : MonoBehaviour
{
    [Tooltip("MUST be unique per placed instance, e.g. \"Coin_01\", \"Coin_02\".")]
    public string itemID;

    Renderer[] renderers;
    Collider col;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        col = GetComponent<Collider>();

        if (string.IsNullOrEmpty(itemID))
        {
            Debug.LogWarning($"CollectibleItem on {name} has no itemID set - give it a unique name!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        SaveLoadManager saveComp = other.GetComponent<SaveLoadManager>();
        if (saveComp != null)
        {
            saveComp.RegisterCollected(itemID);
            SetCollectedSilently(true);
        }
    }

    // Called on pickup, and by the save system on load to instantly reflect a
    // previously-collected state without re-triggering pickup FX/sound.
    public void SetCollectedSilently(bool collected)
    {
        foreach (Renderer r in renderers) r.enabled = !collected;
        if (col) col.enabled = !collected;
    }
}
