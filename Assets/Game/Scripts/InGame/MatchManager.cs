using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MatchManager : MonoBehaviour
{
    static MatchManager _instance;
    public static MatchManager Instance { get => _instance; }

    [SerializeField] PointAreaManager _masterArea;
    [SerializeField] PointAreaManager _otherArea;

    bool _masterIsMine = false;

    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else _instance = this;
    }

    public void SetPlayerToArea(Transform player, bool isMine, bool isMaster)
    {
        if (_masterArea != null) _masterArea.SetPlayerTransform(player, isMaster);
        if (_otherArea != null) _otherArea.SetPlayerTransform(player, isMaster);
        
        if (isMine && isMaster) _masterIsMine = true;
    }
}
