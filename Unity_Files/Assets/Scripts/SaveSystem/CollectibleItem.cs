using UnityEngine;

// CollectibleItem.cs
// A world pickup with a stable, unique ID (either set by hand in the inspector for a
// placed item, or assigned at runtime by CollectibleSpawner for a spawned one). The save
// system records which IDs have been collected so a reload can hide the right ones.
// SETUP: give this GameObject/prefab a Collider with "Is Trigger" checked.
// Standing in the trigger arms it - the player must then press F to actually pick it up
// (not automatic on touch).
public class CollectibleItem : MonoBehaviour
{
    [Tooltip("MUST be unique per instance, e.g. \"Coin_01\". Leave blank on a prefab meant to be spawned by CollectibleSpawner - it assigns one at spawn time.")]
    public string itemID;

    public bool IsCollected { get; private set; }

    // Fired once, the moment the player actually picks this up via F (never fires from a
    // silent state restore during Load - see SetCollectedSilently).
    public event System.Action<CollectibleItem> Collected;

    Renderer[] renderers;
    Collider col;

    bool playerInRange;
    SaveLoadManager pendingSaveComp;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        if (playerInRange && !IsCollected && Input.GetKeyDown(KeyCode.F))
        {
            Collect();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        SaveLoadManager saveComp = other.GetComponent<SaveLoadManager>();
        if (saveComp != null)
        {
            playerInRange = true;
            pendingSaveComp = saveComp;
        }
    }

    void OnTriggerExit(Collider other)
    {
        SaveLoadManager saveComp = other.GetComponent<SaveLoadManager>();
        if (saveComp != null && saveComp == pendingSaveComp)
        {
            playerInRange = false;
            pendingSaveComp = null;
        }
    }

    void Collect()
    {
        if (string.IsNullOrEmpty(itemID))
        {
            Debug.LogWarning($"CollectibleItem on {name} has no itemID set - give it a unique one!");
        }

        if (pendingSaveComp != null)
        {
            pendingSaveComp.RegisterCollected(itemID);
        }

        SetCollectedSilently(true);
        playerInRange = false;
        pendingSaveComp = null;

        Collected?.Invoke(this);
    }

    // Called on pickup, and by the save system on load to instantly reflect a
    // previously-collected state without re-triggering pickup FX/sound or the Collected event.
    public void SetCollectedSilently(bool collected)
    {
        IsCollected = collected;
        foreach (Renderer r in renderers) r.enabled = !collected;
        if (col) col.enabled = !collected;
    }
}
