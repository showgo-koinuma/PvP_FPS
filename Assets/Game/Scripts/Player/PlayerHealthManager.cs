using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>player‚ÌHP‚ğŠÇ—‚·‚é</summary>
public class PlayerHealthManager : Damageable
{
    [SerializeField] int _maxHp = 200;
    [SerializeField] Image _damagaeCanvasImage;
    PlayerManager _pManager;

    // collider
    // 0 MoveBody, 1 mixamorig:LeftUpLeg, 2 mixamorig:LeftLeg, 3 mixamorig:RightUpLeg, 4 mixamorig:RightLeg,
    // 5 mixamorig:Spine, 6 mixamorig:LeftArm, 7 mixamorig:LeftForeArm, 8 mixamorig:Head, 9 mixamorig:RightArm, 10mixamorig:RightForeArm, 
    // “ª : 8
    // “· : 5
    // è‘« : ‚»‚Ì‘¼(MoveBody‚Ìcollider‚É‚Í“–‚½‚ç‚È‚¢)

    int _currentHp;
    int CurrentHp
    {
        get => _currentHp;
        set
        {
            _currentHp = value;
            if (_currentHp <= 0) OnDead();
        }
    }

    private void Awake()
    {
        _pManager = GetComponent<PlayerManager>();
        _currentHp = _maxHp;
    }

    [PunRPC]
    protected override void OnDamageTakenShare(int damage, int collierIndex)
    {
        CurrentHp -= damage;
        OnDamageTakenIsMine();
    }

    /// <summary>©•ª‚ªƒ_ƒ[ƒW‚ğó‚¯‚½‚Æ‚«‚Ìˆ—</summary>
    private void OnDamageTakenIsMine()
    {
        if (!photonView.IsMine) return;
        Debug.Log("dame-ji");

        // ‰æ–Ê‚ğÔ‚­‚·‚éˆ—
        Color color = _damagaeCanvasImage.color;
        color.a = 0.3f;
        _damagaeCanvasImage.color = color; // a‚ğ‚ ‚°‚Ä
        _damagaeCanvasImage.DOFade(0, 0.1f); // 0.1•b‚Å–ß‚·
    }

    void OnDead()
    {
        Debug.Log("sinnda");
        //_lastHitPlayer.AddScore();
        _currentHp = _maxHp;
        _pManager.Respawn();
    }
}
