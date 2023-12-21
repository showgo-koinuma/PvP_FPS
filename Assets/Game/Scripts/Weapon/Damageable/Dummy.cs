
public class Dummy : Damageable
{
    // collider
    // 0 mixamorig:LeftUpLeg, 1 mixamorig:LeftLeg, 2 mixamorig:RightUpLeg, 3 mixamorig:RightLeg, 4 mixamorig:Spine, 5 mixamorig:LeftArm,
    // 6 mixamorig:LeftForeArm, 7 mixamorig:Head, 8 mixamorig:RightArm, 9 mixamorig:RightForeArm, 
    // �� : 7
    // �� : 4
    // �葫 : ���̑�

    // hp status HP��int�Ȃ̂�
    int _maxArmor = 100;
    int _maxHp = 100;
    int _armor;
    int _hp;

    // ���ʂɂ��_���[�W���[�g
    float _headDmgRate = 2f; // ��
    float _limbsDmgRate = 0.8f; // �葫

    /// <summary>HP���Z�b�g�܂ł̎���</summary>
    float _resetHpTime = 2;

    protected override void OnDamageTaken(int dmg, int colliderIndex)
    {
        int calcDmg = dmg; // ���ʂɂ��_���[�W�v�Z
        if (colliderIndex == 7) calcDmg = (int)(calcDmg * _headDmgRate); // ��
        else if (colliderIndex != 4) calcDmg = (int)(calcDmg * _limbsDmgRate); // �葫

        if (_armor >= calcDmg) _armor -= calcDmg; // �A�[�}�[�Ŏ󂯂����
        else if (_armor > 0) // �A�[�}�[�Ŏ󂯂���Ȃ�
        {
            _hp -= calcDmg - _armor; // ���ߕ�hp�����炷
            _armor = 0;
        }
        else // �A�[�}�[���Ȃ�
        {
            _hp -= calcDmg;
            if (_hp < 0) _hp = 0; // ����clamp
        }

        CancelInvoke(nameof(ResetHP));
        Invoke(nameof(ResetHP), _resetHpTime);
    }

    void ResetHP()
    {
        _armor = _maxArmor;
        _hp = _maxHp;
    }

    protected override void OnDamageTakenShare(int damage, int collierIndex) { } // ���Ƃ��Ȃ�񂩁@����

    private void Awake()
    {
        ResetHP();
    }
}
