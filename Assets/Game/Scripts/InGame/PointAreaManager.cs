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
    [SerializeField] PointColorChanger _colorChanger;
    [SerializeField] AudioSource _audioSource;
    // [SerializeField] AudioClip _takingAreaSound; iranaku natta
    [SerializeField] AudioClip _changeOwnerSound;

    [Header("Gizmos")]
    [SerializeField, Tooltip("Gizmosの表示切替")] bool _visible = true;
    [SerializeField] Color _color = Color.yellow;

    Transform _myPlayer;
    AudioSource _gaugeAudioSource;

    // Areaの状態
    bool _isMaster;
    bool _masterInArea;
    bool _otherInArea;
    AreaOwner _areaOwner = AreaOwner.anyone;
    public AreaOwner AreaOwner { get => _areaOwner; }
    AreaState _areaState = AreaState.anyone;
    public AreaState AreaState { get => _areaState; }
    AnyoneInArea _anyoneInArea = 0;

    // area value
    float _masterTakePer = 0;
    float _otherTakePer = 0;

    private void Awake()
    {
        _isMaster = PhotonNetwork.IsMasterClient;
        _gaugeAudioSource = GetComponent<AudioSource>();
        _gaugeAudioSource.enabled = false;
    }

    public void SetPlayerTransform(Transform player)
    {
        _myPlayer = player;
    }

    private void Update()
    {
        if (InGameManager.Instance.GameState != GameState.InGame) return;

        AreaStateUpdate();

        _colorChanger.UpdatePerUI(_areaState, Math.Max(_masterTakePer, _otherTakePer));

        if (_areaOwner == AreaOwner.master) _colorChanger.ChangeColor(_isMaster);
        else if (_areaOwner == AreaOwner.other) _colorChanger.ChangeColor(!_isMaster);
    }

    /// <summary>playerがエリア内にいるか判定し、perValueとstateを更新する</summary>
    void AreaStateUpdate()
    {
        if (_isMaster) // master側の処理
        {
            if (_masterInArea != CheckArea(_myPlayer))
            {
                _masterInArea = !_masterInArea;

                if (_masterInArea)
                {
                    _anyoneInArea |= AnyoneInArea.master;
                }
                else
                {
                    _anyoneInArea &= ~AnyoneInArea.master;
                }
            }

            AreaState lastAreaState = _areaState;
            AreaOwner lastAreaOwner = _areaOwner;
            _areaState = AreaState.anyone;

            // AnyoneInAreaから所有状況を計算
            if (_anyoneInArea == 0) // 誰もいない
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
            else if (_anyoneInArea == AnyoneInArea.master) // masterだけ
            {
                if (_areaOwner != AreaOwner.master)
                {
                    _masterTakePer += _takeAreaPerSpeed * Time.deltaTime;
                    _otherTakePer = 0;
                    _areaState = AreaState.masterTaking;

                    if (_masterTakePer >= 1)
                    {
                        _masterTakePer = 0;
                        _areaOwner = AreaOwner.master;
                    }
                }
                else if (_otherTakePer > 0)
                {
                    _otherTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                }
                else
                {
                    _otherTakePer = 0;
                }
            }
            else if (_anyoneInArea == AnyoneInArea.other) // otherだけ
            {
                if (_areaOwner != AreaOwner.other)
                {
                    _otherTakePer += _takeAreaPerSpeed * Time.deltaTime;
                    _masterTakePer = 0;
                    _areaState = AreaState.otherTaking;

                    if (_otherTakePer >= 1)
                    {
                        _otherTakePer = 0;
                        _areaOwner = AreaOwner.other;
                    }
                }
                else if (_masterTakePer > 0)
                {
                    _masterTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                }
                else
                {
                    _masterTakePer = 0;
                }
            }
            else // 両方いる
            {
                _areaState = AreaState.both;
            }

            if (lastAreaState != _areaState || lastAreaOwner != _areaOwner) // 状況が変わったら共有
            {
                if (lastAreaOwner != _areaOwner) _audioSource.PlayOneShot(_changeOwnerSound); // ownerが変わっていたSound

                // gauge sound set
                _gaugeAudioSource.enabled = _areaState == AreaState.masterTaking || _areaState == AreaState.otherTaking;

                photonView.RPC(nameof(SynchroAreaSituation), RpcTarget.Others, (int)_areaState, (int)_areaOwner); // otherに共有
            }
        }
        else // masterでない場合は状況を共有するだけ　最終的な判定はmasterが行う
        {
            if (_otherInArea != CheckArea(_myPlayer)) // 情報を共有
            {
                _otherInArea = !_otherInArea;
                photonView.RPC(nameof(SynchroInAreaOther), RpcTarget.Others, _otherInArea);
            }

            // エリア内判定から所有状況を計算
            if (_areaState == AreaState.both) return;

            if (_areaOwner == AreaOwner.master)
            {
                _masterTakePer = 0;

                if (_areaState != AreaState.otherTaking)
                {
                    if (_otherTakePer > 0) _otherTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                    else _otherTakePer = 0;
                }
                else
                {
                    _otherTakePer += _takeAreaPerSpeed * Time.deltaTime;
                }
            }
            else if (_areaOwner == AreaOwner.other)
            {
                _otherTakePer = 0;

                if (_areaState != AreaState.masterTaking)
                {
                    if (_masterTakePer > 0) _masterTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                    else _masterTakePer = 0;
                }
                else
                {
                    _masterTakePer += _takeAreaPerSpeed * Time.deltaTime;
                }
            }
            else // if owner == anyone
            {
                if (_areaState == AreaState.masterTaking)
                {
                    _masterTakePer += _takeAreaPerSpeed * Time.deltaTime;
                    _otherTakePer = 0;
                }
                else if (_areaState == AreaState.otherTaking)
                {
                    _otherTakePer += _takeAreaPerSpeed * Time.deltaTime;
                    _masterTakePer = 0;
                }
                else // if state == anyone
                {
                    if (_masterTakePer > 0) _masterTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                    else _masterTakePer = 0;
                    if (_otherTakePer > 0) _otherTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                    else _otherTakePer = 0;
                }
            }
        }
    }

    /// <summary>otherがエリアのplayer情報を共有するためのもの</summary>
    [PunRPC]
    void SynchroInAreaOther(bool isIn)
    {
        if (isIn)
        {
            _anyoneInArea |= AnyoneInArea.other;
        }
        else
        {
            _anyoneInArea &= ~AnyoneInArea.other;
        }
    }
    /// <summary>otherにエリア状況を共有する</summary>
    [PunRPC]
    void SynchroAreaSituation(int areaState, int areaOwner)
    {
        if (!_isMaster)
        {
            if (_areaOwner != (AreaOwner)Enum.ToObject(typeof(AreaOwner), areaOwner))
            { // ownerが変わっていたらSound
                _audioSource.PlayOneShot(_changeOwnerSound);
            }

            _areaState = (AreaState)Enum.ToObject(typeof(AreaState), areaState);
            _areaOwner = (AreaOwner)Enum.ToObject(typeof(AreaOwner), areaOwner);

            // gauge sound set
            _gaugeAudioSource.enabled = _areaState == AreaState.masterTaking || _areaState == AreaState.otherTaking;
        }
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

public enum AreaState
{
    anyone,       // 誰もいない 
    both,         // どちらもいる
    masterTaking,
    otherTaking,
}

[Flags]
enum AnyoneInArea
{
    master = 1,
    other = 2
}