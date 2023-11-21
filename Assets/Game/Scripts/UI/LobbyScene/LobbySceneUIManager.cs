using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// メニューシーンのマネージャーコンポーネント
/// ネットワーク系の処理を行う
/// </summary>
public class LobbySceneUIManager : MonoBehaviourPunCallbacks
{
    [Header("Default")]
    [SerializeField] GameObject _defaultButtonsObj;

    [Header("Room作成")]
    [SerializeField] GameObject _createRoomObj;
    [SerializeField] TextMeshProUGUI _inputRoomName;

    [Header("Room参加")]
    [SerializeField] GameObject _joinRoomObj;
    //[SerializeField] scroll viewに表示するためになんか必要だろ room list

    [Header("PlayerName設定")]
    [SerializeField] GameObject _InputPlayerNameObj;
    [SerializeField] TextMeshProUGUI _inputPlayerName;

    [Header("ゲームスタート待機")]
    [SerializeField] GameObject _waitingStartGameObj;
    //[SerializeField] scroll viewに表示するためになんか必要だろ player name

    [Header("Loading")]
    [SerializeField] GameObject _loadingObj;
    [SerializeField] TextMeshProUGUI _loadingText;

    /// <summary>現在アクティブになっているUIオブジェクト</summary>
    GameObject _activeObj;

    private void Awake()
    {
        ChangeUIObj(_defaultButtonsObj);
    }

    public void BackButton()
    {
        if (_activeObj != _defaultButtonsObj) ChangeUIObj(_defaultButtonsObj);
    }

    /// <summary>指定したUIObjに遷移する</summary>
    public void ChangeUIObj(GameObject toUIObj)
    {
        CloseAllUI();
        (_activeObj = toUIObj).SetActive(true);
    }

    /// <summary>全てのUIを非表示にする</summary>
    void CloseAllUI()
    {
        _defaultButtonsObj.SetActive(false);
        _createRoomObj.SetActive(false);
        _joinRoomObj.SetActive(false);
        _InputPlayerNameObj.SetActive(false);
        _waitingStartGameObj.SetActive(false);
        _loadingObj.SetActive(false);
    }
}
