using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>playerのHPを管理する</summary>
public class PlayerHealthManager : Damageable
{
    [SerializeField] int _maxHp = 200;
    [SerializeField] Image _damagaeCanvasImage;
    PlayerManager _pManager;
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
    protected override void OnDamageTakenShare(int damage, int collierIndex, Vector3 objVectorDiff, int playerID)
    {
        CurrentHp -= damage;
        OnDamageTakenIsMine();
        // 弾道表示
        StartCoroutine(InGameManager.Instance.ViewGameObjects[playerID].GetComponent<GunController>().DrawBallistic(transform.position + objVectorDiff));
    }

    private void OnDamageTakenIsMine()
    {
        if (!photonView.IsMine) return;
        Debug.Log("dame-ji");
        // dame-ji gamen akaku
        Color color = _damagaeCanvasImage.color;
        color.a = 0.3f;
        _damagaeCanvasImage.color = color;
        _damagaeCanvasImage.DOFade(0, 0.1f);
    }

    void OnDead()
    {
        Debug.Log("sinnda");
        _currentHp = _maxHp;
        _pManager.Respawn();
    }
}
