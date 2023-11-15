using UnityEngine;

public interface Damageable
{
    public void OnDamageTaken(int damage, Collider hitCollider);
}
