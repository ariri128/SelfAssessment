using UnityEngine;

public class TestDummy : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount) => Debug.Log($"Dummy took {amount} damage");
}
