using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ���j���[�V�[���̃}�l�[�W���[�R���|�[�l���g
/// �l�b�g���[�N�n�̏������s��
/// </summary>
public class LobbySceneUIManager : MonoBehaviourPunCallbacks
{
    [Header("Default")]
    [SerializeField] GameObject _defaultButtonsObj;

    [Header("Room�쐬")]
    [SerializeField] GameObject _createRoomObj;
    [SerializeField] TextMeshProUGUI _inputRoomName;

    [Header("Room�Q��")]
    [SerializeField] GameObject _joinRoomObj;
    //[SerializeField] scroll view�ɕ\�����邽�߂ɂȂ񂩕K�v���� room list

    [Header("PlayerName�ݒ�")]
    [SerializeField] GameObject _InputPlayerNameObj;
    [SerializeField] TextMeshProUGUI _inputPlayerName;

    [Header("�Q�[���X�^�[�g�ҋ@")]
    [SerializeField] GameObject _waitingStartGameObj;
    //[SerializeField] scroll view�ɕ\�����邽�߂ɂȂ񂩕K�v���� player name

    [Header("Loading")]
    [SerializeField] GameObject _loadingObj;
    [SerializeField] TextMeshProUGUI _loadingText;

    /// <summary>���݃A�N�e�B�u�ɂȂ��Ă���UI�I�u�W�F�N�g</summary>
    GameObject _activeObj;

    private void Awake()
    {
        ChangeUIObj(_defaultButtonsObj);
    }

    public void BackButton()
    {
        if (_activeObj != _defaultButtonsObj) ChangeUIObj(_defaultButtonsObj);
    }

    /// <summary>�w�肵��UIObj�ɑJ�ڂ���</summary>
    public void ChangeUIObj(GameObject toUIObj)
    {
        CloseAllUI();
        (_activeObj = toUIObj).SetActive(true);
    }

    /// <summary>�S�Ă�UI���\���ɂ���</summary>
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
