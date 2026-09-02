using UnityEngine;

/*
   A world pickup with a stable, unique ID assigned at runtime by CollectibleSpawner for spawned items
   The save system records which IDs have been collected so a reload can hide the right ones
*/
public class CollectibleItem : MonoBehaviour
{
    [Tooltip("MUST be unique per instance, e.g. \"Coin_01\". Leave blank on a prefab meant to be spawned by CollectibleSpawner - it assigns one at spawn time.")]
    public string itemID;

    public bool IsCollected { get; private set; }

    public event System.Action<CollectibleItem> Collected;

    // Static, fired by every instance to let GameplayHUD show "Press [F] to collect" without having to individually subscribe to every collectible in the scene
    public static event System.Action<CollectibleItem> AnyArmed;
    public static event System.Action<CollectibleItem> AnyDisarmed;

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
            if (!IsCollected) AnyArmed?.Invoke(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        SaveLoadManager saveComp = other.GetComponent<SaveLoadManager>();
        if (saveComp != null && saveComp == pendingSaveComp)
        {
            playerInRange = false;
            pendingSaveComp = null;
            AnyDisarmed?.Invoke(this);
        }
    }

    void OnDisable()
    {
        if (playerInRange)
        {
            playerInRange = false;
            AnyDisarmed?.Invoke(this);
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
        AnyDisarmed?.Invoke(this);

        Collected?.Invoke(this);
    }

    public void SetCollectedSilently(bool collected)
    {
        IsCollected = collected;
        foreach (Renderer r in renderers) r.enabled = !collected;
        if (col) col.enabled = !collected;
    }
}
