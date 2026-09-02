using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
   Spawns WanderingAI targets in batches of 2 at random points on the baked NavMesh
   They are spaced apart so they don't spawn on top of each other
   Once every target in the current batch has been destroyed (health hit 0), the next batch spawns
*/

public class AITargetSpawner : MonoBehaviour
{
    public GameObject aiTargetPrefab;
    public Transform floor;

    [Tooltip("How many AI targets are active at once before the next batch spawns.")]
    public int batchSize = 2;

    [Tooltip("Keeps spawn points this far in from the floor's edges.")]
    public float edgeMargin = 1.5f;

    [Tooltip("Minimum distance kept between the spawn points within the same batch, so two targets never spawn overlapping/stacked on each other.")]
    public float minSpawnSeparation = 4f;

    [Tooltip("How many random points to try per spawn before giving up and using the last valid one.")]
    public int maxSpawnAttempts = 8;

    Bounds floorBounds;
    readonly List<WanderingAI> activeBatch = new List<WanderingAI>();
    readonly List<Vector3> currentBatchSpawnPoints = new List<Vector3>();

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

        Debug.LogWarning("AITargetSpawner: floor has no Renderer or Collider - defaulting to a 10x10 area at the origin.");
        return new Bounds(Vector3.zero, new Vector3(10f, 1f, 10f));
    }

    void SpawnBatch()
    {
        activeBatch.Clear();
        currentBatchSpawnPoints.Clear();

        for (int i = 0; i < batchSize; i++)
        {
            SpawnOne();
        }
    }

    void SpawnOne()
    {
        if (aiTargetPrefab == null)
        {
            Debug.LogWarning("AITargetSpawner: assign aiTargetPrefab in the inspector.");
            return;
        }

        Vector3 spawnPos = RandomPointOnNavMesh();

        GameObject instance = Instantiate(aiTargetPrefab, spawnPos, Quaternion.identity);
        WanderingAI ai = instance.GetComponent<WanderingAI>();

        if (ai == null)
        {
            Debug.LogError($"AITargetSpawner: {aiTargetPrefab.name} has no WanderingAI component - can't use it as a spawn prefab.");
            Destroy(instance);
            return;
        }

        currentBatchSpawnPoints.Add(spawnPos);
        ai.Died += HandleTargetDied;
        activeBatch.Add(ai);
    }

    Vector3 RandomPointOnNavMesh()
    {
        const float probeHeight = 0.5f;
        Vector3 fallback = floor != null ? floor.position : transform.position;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            float x = Random.Range(floorBounds.min.x + edgeMargin, floorBounds.max.x - edgeMargin);
            float z = Random.Range(floorBounds.min.z + edgeMargin, floorBounds.max.z - edgeMargin);
            Vector3 candidate = new Vector3(x, floorBounds.max.y + probeHeight, z);

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, probeHeight + 1f, NavMesh.AllAreas))
            {
                continue;
            }

            fallback = hit.position;

            if (!IsTooCloseToBatch(hit.position))
            {
                return hit.position;
            }
        }

        return fallback;
    }

    bool IsTooCloseToBatch(Vector3 point)
    {
        foreach (Vector3 existing in currentBatchSpawnPoints)
        {
            if (Vector3.Distance(point, existing) < minSpawnSeparation)
            {
                return true;
            }
        }
        return false;
    }

    void HandleTargetDied(WanderingAI ai)
    {
        ai.Died -= HandleTargetDied;
        activeBatch.Remove(ai);

        if (activeBatch.Count == 0)
        {
            SpawnBatch();
        }
    }
}
