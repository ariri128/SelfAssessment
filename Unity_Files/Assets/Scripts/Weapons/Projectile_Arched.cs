using UnityEngine;

// Projectile_Arched.cs - lobbed projectile (grenade-launcher style). Gravity is ON;
// WeaponSwitcher calls LaunchAt() right after spawning to solve the launch velocity
// that hits a target point in exactly `flightTime` seconds - this is what gives you a
// real, visible arc instead of a hand-tuned guess.
public class Projectile_Arched : ProjectileBase
{
    protected override void Awake()
    {
        base.Awake();
        rb.useGravity = true;
    }

    /// <summary>
    /// Solves for a launch velocity that reaches targetPoint in exactly flightTime seconds
    /// under the scene's current gravity. Higher flightTime = lazier/higher arc,
    /// lower flightTime = flatter/faster arc. Tune flightTime in WeaponSwitcher's inspector.
    /// </summary>
    public void LaunchAt(Vector3 targetPoint, float flightTime)
    {
        Vector3 displacement = targetPoint - transform.position;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);
        float g = Mathf.Abs(Physics.gravity.y);

        float horizontalSpeed = horizontalDisplacement.magnitude / flightTime;
        float verticalSpeed = (displacement.y + 0.5f * g * flightTime * flightTime) / flightTime;

        Vector3 velocity = horizontalDisplacement.normalized * horizontalSpeed + Vector3.up * verticalSpeed;
        rb.linearVelocity = velocity; // Unity 2022 LTS or earlier: use rb.velocity instead
    }
}
