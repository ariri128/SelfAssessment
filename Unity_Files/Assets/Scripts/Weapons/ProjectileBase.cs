using UnityEngine;

// ProjectileBase.cs
// Common parent for the linear, arched, and homing projectiles.
// Requires: Rigidbody + a non-trigger Collider on the prefab, Collision Detection = Continuous
// Dynamic (Rigidbody inspector) so fast-moving projectiles don't tunnel through thin colliders.
[RequireComponent(typeof(Rigidbody))]
public class ProjectileBase : MonoBehaviour
{
    public float damage = 20f;
    public float lifeSpan = 6f;

    // Set by WeaponSwitcher right after Instantiate so the projectile never damages its own shooter.
    [HideInInspector] public GameObject owner;

    protected Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // subclasses that want gravity (Arched) turn this on
    }

    protected virtual void Start()
    {
        Destroy(gameObject, lifeSpan);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == owner)
        {
            return;
        }

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        damageable?.TakeDamage(damage);

        // TODO: spawn your own impact particle/sound here (free asset or built-in primitive burst).
        Destroy(gameObject);
    }
}
