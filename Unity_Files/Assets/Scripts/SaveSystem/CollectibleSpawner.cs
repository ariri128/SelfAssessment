using System.Collections.Generic;
using UnityEngine;

/*
   Spawns collectibles in batches of `batchSize` 3
   Picking randomly between spherePrefab and cubePrefab for each one, at random points on `floor'
   Once every collectible in the current batch has actually been picked up (F), the next batch spawns.
*/
public class CollectibleSpawner : MonoBehaviour
{
    public GameObject spherePrefab;
    public GameObject cubePrefab;
    public Transform floor;

    [Tooltip("How many collectibles are active at once before the next batch spawns.")]
    public int batchSize = 3;

    [Tooltip("Keeps spawns this far in from the floor's edges.")]
    public float edgeMargin = 1.5f;

    [Tooltip("Tiny gap kept between the collectible's bottom and the floor surface, to avoid z-fighting.")]
    public float groundClearance = 0.02f;

    [Tooltip("Optional: retries a spawn point if something solid (like a wall) is already there.")]
    public float overlapCheckRadius = 0.35f;

    Bounds floorBounds;
    readonly List<CollectibleItem> activeBatch = new List<CollectibleItem>();

    void Start()
    {
        floorBounds = GetFloorBounds();
        SpawnBatch();
    }

    Bounds GetFloorBounds()
    {
        Renderer rend = floor.GetComponent<Renderer>();
        if (rend != null) return rend.bounds;

        Collider col = floor.GetComponent<Collider>();
        if (col != null) return col.bounds;

        Debug.LogWarning("CollectibleSpawner: floor has no Renderer or Collider - defaulting to a 10x10 area at the origin.");
        return new Bounds(Vector3.zero, new Vector3(10f, 1f, 10f));
    }

    void SpawnBatch()
    {
        activeBatch.Clear();

        for (int i = 0; i < batchSize; i++)
        {
            SpawnOne();
        }
    }

    void SpawnOne()
    {
        GameObject prefab = (Random.value < 0.5f) ? spherePrefab : cubePrefab;
        Vector3 spawnPos = RandomPointOnFloor();

        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);
        CollectibleItem item = instance.GetComponent<CollectibleItem>();

        if (item == null)
        {
            Debug.LogError($"CollectibleSpawner: {prefab.name} has no CollectibleItem component - can't use it as a collectible prefab.");
            Destroy(instance);
            return;
        }

        SnapToFloor(instance);

        // GUID-based so IDs never collide, even across separate Play sessions that share the same save file on disk
        item.itemID = "Collectible_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
        item.Collected += HandleItemCollected;
        activeBatch.Add(item);
    }

    void SnapToFloor(GameObject instance)
    {
        Renderer rend = instance.GetComponentInChildren<Renderer>();
        if (rend == null) return;

        float bottomToPivot = instance.transform.position.y - rend.bounds.min.y;
        Vector3 pos = instance.transform.position;
        pos.y = floorBounds.max.y + bottomToPivot + groundClearance;
        instance.transform.position = pos;
    }

    Vector3 RandomPointOnFloor()
    {
        const int maxAttempts = 8;
        const float probeHeight = 0.5f;
        Vector3 candidate = Vector3.zero;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(floorBounds.min.x + edgeMargin, floorBounds.max.x - edgeMargin);
            float z = Random.Range(floorBounds.min.z + edgeMargin, floorBounds.max.z - edgeMargin);
            candidate = new Vector3(x, floorBounds.max.y + probeHeight, z);

            bool blocked = Physics.CheckSphere(candidate, overlapCheckRadius, ~0, QueryTriggerInteraction.Ignore);
            if (!blocked) return candidate;
        }

        return candidate;
    }

    void HandleItemCollected(CollectibleItem item)
    {
        item.Collected -= HandleItemCollected;

        foreach (CollectibleItem member in activeBatch)
        {
            if (!member.IsCollected) return;
        }

        SpawnBatch();
    }
}
