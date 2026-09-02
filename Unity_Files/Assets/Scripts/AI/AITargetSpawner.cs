using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// AITargetSpawner.cs
// Spawns WanderingAI targets in batches (default 2) at random points on the baked NavMesh,
// spaced apart so they don't spawn on top of each other. Once every target in the current
// batch has been destroyed (health hit 0), the next batch spawns. Mirrors CollectibleSpawner's
// batch pattern, just driven by WanderingAI.Died instead of CollectibleItem.Collected.
//
// SETUP:
//  1. Turn your existing AI_Target GameObject into a prefab (drag it from the Hierarchy into
//     Assets/Prefabs), then delete the scene instance - this script spawns its own.
//  2. Put this script on an empty GameObject in the scene (e.g. "AITargetSpawner").
//  3. Assign aiTargetPrefab to that prefab, and drag your floor object into `floor`.
//  4. Bake the NavMesh BEFORE pressing Play - spawn points are sampled from it, so an unbaked
//     or stale NavMesh will make every spawn attempt fail and fall back to the floor center.
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

    // Samples random points on the baked NavMesh (not just raw floor bounds) so every spawn is
    // guaranteed walkable, and rejects candidates too close to another point already chosen for
    // this batch so the two targets don't spawn stacked on top of each other.
    Vector3 RandomPointOnNavMesh()
    {
        const float probeHeight = 0.5f; // arbitrary - just needs to be above the floor to sample down onto it
        // Starts as the floor's own position so there's always a sane point to fall back to even
        // if every attempt below misses the NavMesh entirely (e.g. it hasn't been baked yet).
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

        // Every attempt either missed the NavMesh or landed too close to the batch-mate - spawn
        // at the last valid point found anyway rather than skipping the spawn entirely.
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
