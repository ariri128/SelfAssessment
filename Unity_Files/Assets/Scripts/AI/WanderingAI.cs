using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// WanderingAI.cs
// A simple moving AI target for the homing missile to lock onto and track - this is the
// minimum needed to satisfy "dynamically tracks a moving AI" now that the full circuit-racing
// test has been dropped from scope. No checkpoints/laps needed, just real NavMesh movement
// (not a fixed back-and-forth patrol) so it reads as "AI" and not a scripted animation.
//
// SETUP:
//  1. If NavMeshAgent/NavMesh baking isn't already in your project, install the "AI Navigation"
//     package via Window > Package Manager (Unity 2022 LTS+ moved this out of the core engine).
//  2. Bake a NavMesh for your level: Window > AI > Navigation > Bake tab > Bake.
//  3. Tag this GameObject with the same tag WeaponSwitcher.lockOnTag expects (default "AITarget") -
//     add that tag first under Edit > Project Settings > Tags and Layers if it doesn't exist.
//  4. Give it a Renderer (e.g. a Capsule primitive's MeshRenderer) so the idle-pause and hit-flash
//     polish below actually show up - both are optional but cheap and read well on camera.
[RequireComponent(typeof(NavMeshAgent))]
public class WanderingAI : MonoBehaviour, IDamageable
{
    [Header("Wander")]
    public float wanderRadius = 20f;
    public float repickDistanceThreshold = 1.0f;
    [Tooltip("Random pause at each waypoint before picking the next one, so it doesn't look like it's gliding non-stop.")]
    public float minIdlePause = 0.5f;
    public float maxIdlePause = 2f;

    [Header("Avoid collectibles")]
    [Tooltip("Won't pick a wander destination this close to an uncollected collectible, so it doesn't end up parked on top of / clipping through one. Re-checked live against CollectibleSpawner's active batch, so it stays correct as items are picked up and new batches spawn.")]
    public float collectibleAvoidRadius = 2f;
    [Tooltip("How many random points to try before giving up and just using whatever it found (guarantees it never freezes in place).")]
    public int maxPickAttempts = 8;

    [Header("Health")]
    public float health = 100f;

    [Header("Hit feedback (optional polish)")]
    public Color hitFlashColor = Color.red;
    public float hitFlashDuration = 0.15f;

    NavMeshAgent agent;
    Renderer visualRenderer;
    Color baseColor;
    bool waitingToRepick;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        visualRenderer = GetComponentInChildren<Renderer>();
        if (visualRenderer != null)
        {
            baseColor = visualRenderer.material.color;
        }
        PickNewDestination();
    }

    void Update()
    {
        if (!waitingToRepick && !agent.pathPending && agent.remainingDistance < repickDistanceThreshold)
        {
            StartCoroutine(IdleThenRepick());
        }
    }

    IEnumerator IdleThenRepick()
    {
        waitingToRepick = true;
        yield return new WaitForSeconds(Random.Range(minIdlePause, maxIdlePause));
        PickNewDestination();
        waitingToRepick = false;
    }

    void PickNewDestination()
    {
        Vector3 fallbackPoint = transform.position;
        bool foundAnyValidPoint = false;

        for (int attempt = 0; attempt < maxPickAttempts; attempt++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
            if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                continue;
            }

            fallbackPoint = hit.position;
            foundAnyValidPoint = true;

            if (!IsNearAnyCollectible(hit.position))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }

        // Every attempt landed near a collectible (or the NavMesh sample failed) - move to the
        // last valid point anyway rather than freezing; it'll just repick again once it arrives.
        if (foundAnyValidPoint)
        {
            agent.SetDestination(fallbackPoint);
        }
    }

    bool IsNearAnyCollectible(Vector3 point)
    {
        CollectibleItem[] collectibles = FindObjectsOfType<CollectibleItem>();
        foreach (CollectibleItem item in collectibles)
        {
            if (item.IsCollected) continue;
            if (Vector3.Distance(point, item.transform.position) < collectibleAvoidRadius)
            {
                return true;
            }
        }
        return false;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log($"[AI] {name} took {amount} damage (health={health}).");
        if (visualRenderer != null)
        {
            StopCoroutine(nameof(FlashHit));
            StartCoroutine(FlashHit());
        }
        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator FlashHit()
    {
        visualRenderer.material.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        visualRenderer.material.color = baseColor;
    }
}
