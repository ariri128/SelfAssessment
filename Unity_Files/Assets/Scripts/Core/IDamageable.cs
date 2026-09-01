// IDamageable.cs
// Anything that can be hit by a weapon (hitscan, projectile, or homing missile) implements this.
public interface IDamageable
{
    void TakeDamage(float amount);
}
