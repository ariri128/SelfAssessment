using UnityEngine;

// Projectile_Homing.cs
// Requires a lock-on target (set by WeaponSwitcher.FireHoming right after Instantiate).
// Curves toward the target at a capped turn rate instead of snapping directly onto it -
// that turn-rate cap is what makes it read as a "gameplay speed" homing missile rather
// than a magic instant-hit.
public class Projectile_Homing : ProjectileBase
{
    [Tooltip("Try 18-28 m/s for a mid-size level - fast enough to feel dangerous, slow enough to dodge/track on camera.")]
    public float speed = 22f;

    [Tooltip("Degrees/sec it can turn. Lower = lazier arc, higher = snaps on target harder.")]
    public float turnRateDegPerSec = 180f;

    [HideInInspector] public Transform target;

    protected override void Awake()
    {
        base.Awake();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        Vector3 desiredForward = target != null
            ? (target.position - transform.position).normalized
            : transform.forward;

        Vector3 newForward = Vector3.RotateTowards(
            transform.forward, desiredForward,
            turnRateDegPerSec * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);

        // MoveRotation (not transform.rotation =) since rb is a non-kinematic Rigidbody -
        // writing transform directly fights the physics engine's own interpolation/collision
        // solve and can jitter, especially with Continuous Dynamic collision detection.
        // MoveRotation doesn't apply until the physics step runs, so build velocity from
        // newRotation directly rather than reading transform.forward right after - that would
        // still be last frame's facing.
        Quaternion newRotation = Quaternion.LookRotation(newForward);
        rb.MoveRotation(newRotation);
        rb.linearVelocity = newRotation * Vector3.forward * speed; // Unity 2022 LTS or earlier: use rb.velocity
    }
}
