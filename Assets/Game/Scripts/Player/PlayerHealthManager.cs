using UnityEngine;
/// <summary>playerのHPを管理する</summary>
public class PlayerHealthManager : Damageable
{
    protected override void OnDamageTaken(int damage, int collierIndex, Vector3 objVectorDiff, int playerID)
    {
        throw new System.NotImplementedException();
    }
}
