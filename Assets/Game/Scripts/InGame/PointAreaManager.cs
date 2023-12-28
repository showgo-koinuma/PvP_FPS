using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PointAreaManager : MonoBehaviourPun
{
    [Header("Area renge")]
    [SerializeField, Tooltip("obj位置からのdelta")] Vector3 _center = Vector3.zero;
    [SerializeField, Tooltip("centerを中心としたサイズ")] Vector3 _size = Vector3.one;

    [Header("Area system")]
    [SerializeField, Tooltip("areaを確保するスピード(per/s)")] float _takeAreaPerSpeed;
    [SerializeField] float _lostAreaPerSpeed;

    [Header("UI")]
    [SerializeField] Image _takingPerImage;
    [SerializeField] Image _areaOwnerImage;
    [SerializeField] Color[] _teamColor;
 
    [Header("Gizmos")]
    [SerializeField, Tooltip("Gizmosの表示切替")] bool _visible = true;
    [SerializeField] Color _color = Color.yellow;

    Transform _master, _other;
    Transform _myPlayer;
    bool _isMaster;
    bool _masterInArea;
    bool _otherInArea;

    AreaOwner _areaOwner = AreaOwner.anyone;
    public AreaOwner AreaOwner { get => _areaOwner; }
    WhoTaking _whoTaking = WhoTaking.anyone;

    // area value
    float _masterTakePer;
    float _otherTakePer;

    public void SetPlayerTransform(Transform player, bool isMaster)
    {
        if (isMaster) _master = player;
        else _other = player;

        _myPlayer = player;
        _isMaster = isMaster;
    }

    private void Update()
    {
        AreaStateUpdate();
        UIUpdate();
    }

    /// <summary>playerがエリア内にいるか判定し、perValueとstateを更新する</summary>
    void AreaStateUpdate()
    {
        if (PhotonNetwork.IsMasterClient) // master側の処理
        {
            if (_masterInArea != CheckArea(_myPlayer))
            {
                _masterInArea = !_masterInArea;
            }

            WhoTaking lastWhoTaking = _whoTaking;
            AreaOwner lastAreaOwner = _areaOwner;
            _whoTaking = WhoTaking.anyone;

            // エリア内判定から所有状況を計算
            if (_masterInArea && !_otherInArea && _areaOwner != AreaOwner.master) // masterだけ && masterが所有してない
            {
                if (_masterTakePer < 1)
                {
                    _masterTakePer += _takeAreaPerSpeed * Time.deltaTime;
                    _whoTaking = WhoTaking.master;
                }
                else
                {
                    _masterTakePer = 0;
                    _areaOwner = AreaOwner.master; // 所有者変更
                }
            }
            else if (!_masterInArea && _otherInArea && _areaOwner != AreaOwner.other) // otherだけ && otherが所有してない
            {
                if (_otherTakePer < 1)
                {
                    _otherTakePer += _takeAreaPerSpeed * Time.deltaTime;
                    _whoTaking = WhoTaking.other;
                }
                else
                {
                    _otherTakePer = 0;
                    _areaOwner = AreaOwner.other; // 所有者変更
                }
            }
            else if (!_masterInArea && !_otherInArea && _areaOwner == AreaOwner.anyone) // どちらもいない && 誰も所有してない = 減少
            {
                if (_masterTakePer > 0) _masterTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                else _masterTakePer = 0;
                if (_otherTakePer > 0) _otherTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                else _otherTakePer = 0;
            } // その他不変

            if (lastWhoTaking != _whoTaking || lastAreaOwner != _areaOwner) // 状況が変わったら
            {
                photonView.RPC(nameof(SynchroAreaSituation), RpcTarget.All, (int)_whoTaking, (int)_areaOwner); // otherに共有
            }
        }
        else // masterでない場合は状況を共有するだけ　最終的な判定はmasterが行う
        {
            if (_otherInArea != CheckArea(_myPlayer))
            {
                photonView.RPC(nameof(SynchroInArea), RpcTarget.All, !_otherInArea);
            }

            // エリア内判定から所有状況を計算
            if (_areaOwner != AreaOwner.master && _whoTaking == WhoTaking.master)
            {
                _masterTakePer += _takeAreaPerSpeed * Time.deltaTime;
                _otherTakePer = 0;
            }
            else if (_areaOwner != AreaOwner.other && _whoTaking == WhoTaking.other)
            {
                _otherTakePer += _takeAreaPerSpeed * Time.deltaTime;
                _masterTakePer = 0;
            }
            else
            {
                if (_masterTakePer > 0)
                {
                    _masterTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                }
                else
                {
                    _masterTakePer = 0;
                }
                if (_otherTakePer > 0)
                {
                    _otherTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                }
                else
                {
                    _otherTakePer = 0;
                }
            }
        }
    }

    /// <summary>otherがエリアのplayer情報を共有するためのもの</summary>
    [PunRPC]
    void SynchroInArea(bool isIn)
    {
        _otherInArea = isIn;
    }
    /// <summary>otherにエリア状況を共有する</summary>
    [PunRPC]
    void SynchroAreaSituation(int whoTaking, int areaOwner)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            _whoTaking = (WhoTaking)Enum.ToObject(typeof(WhoTaking), whoTaking);
            _areaOwner = (AreaOwner)Enum.ToObject(typeof(AreaOwner), areaOwner);
        }
    }

    /// <summary>内部データ更新をUIに反映させる</summary>
    void UIUpdate()
    {
        _takingPerImage.fillAmount = Mathf.Max(_masterTakePer, _otherTakePer);

        if (_whoTaking == WhoTaking.master) _takingPerImage.color = _teamColor[0];
        else _takingPerImage.color = _teamColor[1];

        if (_areaOwner == AreaOwner.master) _areaOwnerImage.color = _teamColor[0];
        else if (_areaOwner == AreaOwner.other) _areaOwnerImage.color= _teamColor[1];
        else _areaOwnerImage.color = _teamColor[2];
    }

    bool CheckArea(Transform player)
    {
        Vector3 distance = transform.position - player.position;
        return Mathf.Abs(distance.x) < _size.x / 2 &&
            Mathf.Abs(distance.y) < _size.y / 2 &&
            Mathf.Abs(distance.z) < _size.z / 2;
    }

    /// <summary>areaの所有状況をリセットさせる</summary> // タイミングむずいか
    public void ResetArea()
    {
        _masterTakePer = 0;
        _otherTakePer = 0;
    }

    void OnDrawGizmos()
    {
        if (!_visible)
        {
            return;
        }

        Gizmos.color = _color;

        Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
        Gizmos.DrawWireCube(_center, _size);
    }
}

public enum AreaOwner
{
    anyone,
    master,
    other
}

public enum WhoTaking
{
    anyone,
    master,
    other
}