using UnityEngine;
using System.Collections;

// WeaponSwitcher.cs
// Add to your player. Handles switching between trace-fire (hitscan), linear projectile,
// arched projectile, and the homing missile, plus lock-on targeting for the missile.
//
// NOTE ON INPUT: this uses the legacy Input class (Input.GetKeyDown / Input.GetButtonDown).
// If your project only has Unity's newer Input System package installed, go to
// Edit > Project Settings > Player > Active Input Handling and set it to "Both" (or
// "Input Manager (Old)"), otherwise these calls will throw at runtime.
public enum WeaponMode { Hitscan, Linear, Arched, Homing }

public class WeaponSwitcher : MonoBehaviour
{
    [Header("General")]
    public Camera aimCamera;
    public Transform muzzle; // empty child transform roughly at the gun's barrel tip
    public WeaponMode currentMode = WeaponMode.Hitscan;

    [Header("Hitscan")]
    public float hitscanRange = 100f;
    public float hitscanDamage = 15f;

    [Header("Projectile Prefabs")]
    public ProjectileBase linearProjectilePrefab;
    public ProjectileBase archedProjectilePrefab;
    public Projectile_Homing homingProjectilePrefab;

    [Header("Arched Tuning")]
    [Tooltip("Seconds of flight time to reach the aim point. Higher = lazier/higher lob, lower = flatter/faster.")]
    public float archedFlightTime = 1.2f;
    [Tooltip("How far in front of the crosshair the arched shot aims at.")]
    public float archedAimDistance = 25f;

    [Header("Homing Lock-On")]
    public float lockOnRange = 60f;
    public float lockOnConeDegrees = 15f;
    public string lockOnTag = "AITarget"; // tag your WanderingAI (or any target) with this

    [Header("Hitscan Tracer")]
    public LineRenderer tracerLine;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentMode = WeaponMode.Hitscan;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentMode = WeaponMode.Linear;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentMode = WeaponMode.Arched;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentMode = WeaponMode.Homing;

        if (Input.GetButtonDown("Fire1")) // default: left mouse button / left ctrl
        {
            Fire();
        }
    }

    public void Fire()
    {
        switch (currentMode)
        {
            case WeaponMode.Hitscan: FireHitscan(); break;
            case WeaponMode.Linear: FireLinear(); break;
            case WeaponMode.Arched: FireArched(); break;
            case WeaponMode.Homing: FireHoming(); break;
        }
    }

    void FireHitscan()
    {
        Vector3 origin = aimCamera.transform.position;
        Vector3 dir = aimCamera.transform.forward;
        Vector3 endPoint = origin + dir * hitscanRange;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, hitscanRange))
        {
            hit.collider.GetComponent<IDamageable>()?.TakeDamage(hitscanDamage);
            endPoint = hit.point;
        }

        StartCoroutine(FlashTracer(muzzle.position, endPoint));
    }

    IEnumerator FlashTracer(Vector3 start, Vector3 end)
    {
        tracerLine.enabled = true;
        tracerLine.SetPosition(0, start);
        tracerLine.SetPosition(1, end);
        yield return new WaitForSeconds(0.05f);
        tracerLine.enabled = false;
    }

    void FireLinear()
    {
        if (!linearProjectilePrefab)
        {
            Debug.LogWarning("[Weapon] Assign linearProjectilePrefab in the inspector.");
            return;
        }
        ProjectileBase proj = Instantiate(linearProjectilePrefab, muzzle.position, aimCamera.transform.rotation);
        proj.owner = gameObject;
    }

    void FireArched()
    {
        if (!archedProjectilePrefab)
        {
            Debug.LogWarning("[Weapon] Assign archedProjectilePrefab in the inspector.");
            return;
        }
        Vector3 targetPoint = aimCamera.transform.position + aimCamera.transform.forward * archedAimDistance;
        ProjectileBase proj = Instantiate(archedProjectilePrefab, muzzle.position, Quaternion.identity);
        proj.owner = gameObject;

        Projectile_Arched arched = proj as Projectile_Arched;
        arched?.LaunchAt(targetPoint, archedFlightTime);
    }

    void FireHoming()
    {
        if (!homingProjectilePrefab)
        {
            Debug.LogWarning("[Weapon] Assign homingProjectilePrefab in the inspector.");
            return;
        }

        Transform target = FindLockOnTarget();
        if (target == null)
        {
            Debug.Log("[Weapon] No lock-on target in view - aim at an AI target first.");
            return;
        }

        Projectile_Homing proj = Instantiate(homingProjectilePrefab, muzzle.position, aimCamera.transform.rotation);
        proj.owner = gameObject;
        proj.target = target;
    }

    // Reads the live damage value straight off the assigned prefab/field for each mode, so
    // GameplayHUD's weapon list always matches whatever you've actually tuned in the
    // inspector instead of a second hardcoded copy of the numbers.
    public float GetWeaponDamage(WeaponMode mode)
    {
        switch (mode)
        {
            case WeaponMode.Hitscan: return hitscanDamage;
            case WeaponMode.Linear: return linearProjectilePrefab ? linearProjectilePrefab.damage : 0f;
            case WeaponMode.Arched: return archedProjectilePrefab ? archedProjectilePrefab.damage : 0f;
            case WeaponMode.Homing: return homingProjectilePrefab ? homingProjectilePrefab.damage : 0f;
            default: return 0f;
        }
    }

    Transform FindLockOnTarget()
    {
        GameObject[] candidates = GameObject.FindGameObjectsWithTag(lockOnTag);
        Transform best = null;
        float bestDot = Mathf.Cos(lockOnConeDegrees * Mathf.Deg2Rad);

        Vector3 origin = aimCamera.transform.position;
        Vector3 dir = aimCamera.transform.forward;

        foreach (GameObject candidate in candidates)
        {
            Vector3 toTarget = candidate.transform.position - origin;
            float dist = toTarget.magnitude;
            if (dist > lockOnRange || dist < 0.01f)
            {
                continue;
            }
            float dot = Vector3.Dot(dir, toTarget.normalized);
            if (dot > bestDot)
            {
                bestDot = dot;
                best = candidate.transform;
            }
        }
        return best;
    }
}
