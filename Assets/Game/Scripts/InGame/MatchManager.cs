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
    int _masterUICount = 0;
    int _otherUICount = 0;

    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else _instance = this;

        _thisIsMaster = PhotonNetwork.IsMasterClient;
    }

    /// <summary>エリアにプレイヤーを登録する</summary>
    public void SetPlayerToArea(Transform player, bool isMaster)
    {
        if (_masterArea != null) _masterArea.SetPlayerTransform(player, isMaster);
        //if (_otherArea != null) _otherArea.SetPlayerTransform(player, isMaster);
        
        if (isMaster) _masterIsMine = true;
    }

    /// <summary>AreaOwnerからカウントを更新する</summary>
    void AreaCountUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (_masterArea.AreaOwner == AreaOwner.master)
            {
                _masterAreaCount += _areaCountUpSpeed * Time.deltaTime;

                if (_masterAreaCount >= _masterUICount + 1)
                {
                    _masterUICount++;
                    photonView.RPC(nameof(SynchroAreaCountText), RpcTarget.All);
                }
            }
            else if (_masterArea.AreaOwner == AreaOwner.other)
            {
                _otherAreaCount += _areaCountUpSpeed * Time.deltaTime;

                if (_otherAreaCount >= _otherUICount + 1) 
                {
                    _otherUICount++;
                    photonView.RPC(nameof(SynchroAreaCountText), RpcTarget.All);
                }
            }
        }
    }

    [PunRPC]
    void SynchroAreaCountText()
    {
        _masterCountText.text = _masterUICount.ToString("D2");
        _otherCountText.text = _otherUICount.ToString("D2");
    }

    private void Update()
    {
        AreaCountUpdate();
    }
}