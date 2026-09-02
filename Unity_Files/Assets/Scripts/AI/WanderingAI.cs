using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
   A simple moving AI target for the homing missile to lock onto and track
   No checkpoints/laps needed, just real NavMesh movement - not a fixed back-and-forth patrol
*/

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

    // Fired the moment health hits 0, just before Destroy(gameObject)
	// Lets an AITargetSpawner react to this specific instance dying without polling every frame.
    public event System.Action<WanderingAI> Died;

    // Static, fired alongside Died for every instance
	// Lets GameplayHUD show a "Target Destroyed" message without having to subscribe to each WanderingAI individually
    public static event System.Action<WanderingAI> AnyTargetDestroyed;

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
            Died?.Invoke(this);
            AnyTargetDestroyed?.Invoke(this);
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
