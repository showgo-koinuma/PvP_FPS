using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>player��HP���Ǘ�����</summary>
public class PlayerHealthManager : Damageable
{
    [SerializeField] int _maxHp = 200;
    [SerializeField] Image _damagaeCanvasImage;
    PlayerManager _pManager;
    PlayerManager _lastHitPlayer;
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
        // 1v1�Ȃ̂�1�񂾂��Ζʂ�o�^
        if (!_lastHitPlayer) _lastHitPlayer = InGameManager.Instance.ViewGameObjects[playerID].GetComponent<PlayerManager>();
        CurrentHp -= damage;
        OnDamageTakenIsMine();
    }

    /// <summary>�_���[�W���󂯂��Ƃ��̏���</summary>
    private void OnDamageTakenIsMine()
    {
        if (!photonView.IsMine) return;
        Debug.Log("dame-ji");

        // ��ʂ�Ԃ����鏈��
        Color color = _damagaeCanvasImage.color;
        color.a = 0.3f;
        _damagaeCanvasImage.color = color;
        _damagaeCanvasImage.DOFade(0, 0.1f);
    }

    void OnDead()
    {
        Debug.Log("sinnda");
        _lastHitPlayer.AddScore();
        _currentHp = _maxHp;
        _pManager.Respawn();
    }
}
