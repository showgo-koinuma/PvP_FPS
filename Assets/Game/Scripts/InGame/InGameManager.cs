using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    [SerializeField, Tooltip("playerのスポーン地点 [0]:Master, [1]:not Master")] Vector3[] _playerSpawnPoints;
    public Vector3[] PlayerSpawnPoints { get => _playerSpawnPoints; }
    static InGameManager _instance;
    public static InGameManager Instance { get => _instance; }
    public event Action UpdateAction;

    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else _instance = this;
    }

    private void Start()
    {
        Vector3 position;
        Quaternion forword;
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            position = InGameManager.Instance.PlayerSpawnPoints[0];
            forword = Quaternion.Euler(Vector3.forward);
        }
        else
        {
            position = InGameManager.Instance.PlayerSpawnPoints[1];
            forword = Quaternion.AngleAxis(180, Vector3.up);
        }
        PhotonNetwork.Instantiate("Player", position, forword);
    }

    public void FinishGame()
    {
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel(0);
    }

    private void Update()
    {
        UpdateAction?.Invoke();
    }
}