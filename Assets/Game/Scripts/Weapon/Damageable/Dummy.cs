
public class Dummy : Damageable
{
    // collider
    // 0 mixamorig:LeftUpLeg, 1 mixamorig:LeftLeg, 2 mixamorig:RightUpLeg, 3 mixamorig:RightLeg, 4 mixamorig:Spine, 5 mixamorig:LeftArm,
    // 6 mixamorig:LeftForeArm, 7 mixamorig:Head, 8 mixamorig:RightArm, 9 mixamorig:RightForeArm, 
    // 頭 : 7
    // 胴 : 4
    // 手足 : その他

    // hp status HPはintなのか
    int _maxArmor = 100;
    int _maxHp = 100;
    int _armor;
    int _hp;

    // 部位によるダメージレート
    float _headDmgRate = 2f; // 頭
    float _limbsDmgRate = 0.8f; // 手足

    /// <summary>HPリセットまでの時間</summary>
    float _resetHpTime = 2;

    protected override void OnDamageTaken(int dmg, int colliderIndex)
    {
        int calcDmg = dmg; // 部位によるダメージ計算
        if (colliderIndex == 7) calcDmg = (int)(calcDmg * _headDmgRate); // 頭
        else if (colliderIndex != 4) calcDmg = (int)(calcDmg * _limbsDmgRate); // 手足

        if (_armor >= calcDmg) _armor -= calcDmg; // アーマーで受けきれる
        else if (_armor > 0) // アーマーで受けきれない
        {
            _hp -= calcDmg - _armor; // 超過分hpを減らす
            _armor = 0;
        }
        else // アーマーがない
        {
            _hp -= calcDmg;
            if (_hp < 0) _hp = 0; // 下限clamp
        }

        CancelInvoke(nameof(ResetHP));
        Invoke(nameof(ResetHP), _resetHpTime);
    }

    void ResetHP()
    {
        _armor = _maxArmor;
        _hp = _maxHp;
    }

    protected override void OnDamageTakenShare(int damage, int collierIndex) { } // 何とかならんか　眠い

    private void Awake()
    {
        ResetHP();
    }
}
