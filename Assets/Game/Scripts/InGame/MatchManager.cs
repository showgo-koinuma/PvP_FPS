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

    /// <summary>�G���A�Ƀv���C���[��o�^����</summary>
    public void SetPlayerToArea(Transform player, bool isMaster)
    {
        if (_masterArea != null) _masterArea.SetPlayerTransform(player);
        //if (_otherArea != null) _otherArea.SetPlayerTransform(player, isMaster);
        
        if (isMaster) _masterIsMine = true;
    }

    /// <summary>AreaOwner����J�E���g���X�V����</summary>
    void AreaCountUpdate()
    {
        if (PhotonNetwork.IsMasterClient) // master����match�󋵂͏������邱�ƂƂ���
        {
            if (_masterArea.AreaOwner == AreaOwner.master)
            {
                _masterAreaCount += _areaCountUpSpeed * Time.deltaTime;

                if (_masterAreaCount >= _masterUICount + 1)
                {
                    _masterUICount++;
                    photonView.RPC(nameof(SynchroAreaCountText), RpcTarget.All, _masterUICount, _otherUICount);
                }
            }
            else if (_masterArea.AreaOwner == AreaOwner.other)
            {
                _otherAreaCount += _areaCountUpSpeed * Time.deltaTime;

                if (_otherAreaCount >= _otherUICount + 1) 
                {
                    _otherUICount++;
                    photonView.RPC(nameof(SynchroAreaCountText), RpcTarget.All, _masterUICount, _otherUICount);
                }
            }
        }
    }

    [PunRPC]
    void SynchroAreaCountText(int masterCount, int otherCount)
    {
        _masterCountText.text = masterCount.ToString("D2");
        _otherCountText.text = otherCount.ToString("D2");
    }

    private void Update()
    {
        AreaCountUpdate();
    }
}