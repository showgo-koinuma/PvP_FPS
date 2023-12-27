using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class MatchManager : MonoBehaviourPun
{
    [SerializeField] PointAreaManager _masterArea;
    //[SerializeField] PointAreaManager _otherArea;

    [SerializeField] float _areaCountUpSpeed = 1f;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI _masterCountText;
    [SerializeField] TextMeshProUGUI _otherCountText;

    static MatchManager _instance;
    public static MatchManager Instance { get => _instance; }

    bool _masterIsMine = false;
    bool _thisIsMaster;

    float _masterAreaCount = 0;
    float _otherAreaCount = 0;
    int _nextSynchroCount = 1;

    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else _instance = this;

        _thisIsMaster = PhotonNetwork.IsMasterClient;
    }

    public void SetPlayerToArea(Transform player, bool isMaster)
    {
        if (_masterArea != null) _masterArea.SetPlayerTransform(player, isMaster);
        //if (_otherArea != null) _otherArea.SetPlayerTransform(player, isMaster);
        
        if (isMaster) _masterIsMine = true;
    }

    void AreaCountUpdate()
    {
        if (_masterArea.AreaOwner == AreaOwner.master && _thisIsMaster)
        {
            _masterAreaCount += _areaCountUpSpeed * Time.deltaTime;

            if (_masterAreaCount >= _nextSynchroCount)
            {
                photonView.RPC(nameof(SynchroAreaCountText), RpcTarget.All);
                _nextSynchroCount++;
            }
        }
        else if (_masterArea.AreaOwner == AreaOwner.other && !_thisIsMaster)
        {
            _otherAreaCount += _areaCountUpSpeed * Time.deltaTime;

            if (_otherAreaCount >= _nextSynchroCount)
            {
                photonView.RPC(nameof(SynchroAreaCountText), RpcTarget.All);
                _nextSynchroCount++;
            }
        }
    }

    [PunRPC]
    void SynchroAreaCountText()
    {
        if (_thisIsMaster)
        {
            _masterCountText.text = _nextSynchroCount.ToString("D2");
        }
        else
        {
            _otherCountText.text = _nextSynchroCount.ToString("D2");
        }
    }

    private void Update()
    {
        AreaCountUpdate();
    }
}