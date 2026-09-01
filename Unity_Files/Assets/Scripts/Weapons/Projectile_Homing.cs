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

        transform.rotation = Quaternion.LookRotation(newForward);
        rb.linearVelocity = transform.forward * speed; // Unity 2022 LTS or earlier: use rb.velocity
    }
}
