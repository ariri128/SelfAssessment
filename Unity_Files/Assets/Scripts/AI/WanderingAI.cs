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
[RequireComponent(typeof(NavMeshAgent))]
public class WanderingAI : MonoBehaviour, IDamageable
{
    public float wanderRadius = 20f;
    public float health = 100f;
    public float repickDistanceThreshold = 1.0f;

    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        PickNewDestination();
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < repickDistanceThreshold)
        {
            PickNewDestination();
        }
    }

    void PickNewDestination()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log($"[AI] {name} took {amount} damage (health={health}).");
        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
