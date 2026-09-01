using UnityEngine;

// Projectile_Linear.cs - straight-line firing projectile (no gravity, no homing).
public class Projectile_Linear : ProjectileBase
{
    public float speed = 40f; // m/s - straight-line travel speed

    protected override void Awake()
    {
        base.Awake();
        rb.useGravity = false;
    }

    protected override void Start()
    {
        base.Start();
        // NOTE: Rigidbody.velocity was renamed to Rigidbody.linearVelocity in Unity 6.
        // If you're on Unity 2022 LTS or earlier and this doesn't compile, change every
        // `rb.linearVelocity` in this file (and the other projectile scripts) to `rb.velocity`.
        rb.linearVelocity = transform.forward * speed;
    }
}
