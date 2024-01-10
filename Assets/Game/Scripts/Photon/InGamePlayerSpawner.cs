using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// MonoBehaviourPunCallbacks���p�����āAPUN�̃R�[���o�b�N���󂯎���悤�ɂ���
/// <summary>�}�b�`���O�V�X�e���̂Ȃ�����connect</summary>
public class InGamePlayerSpawner : MonoBehaviourPunCallbacks
{
    /* �}�X�^�[�T�[�o�[ -> ���r�[ -> �w�薼��JoinOrCreateRoom
     * ���� -> onCreateRoom��onJoinedRoom�łP�l�ڂQ�l�ڂ𔻒�
     * ���s -> ���s�R�[���o�b�N -> room�������܂��͂��̑��̃G���[
    */

    private void Start()
    {
        // PhotonServerSettings�̐ݒ���e���g���ă}�X�^�[�T�[�o�[�֐ڑ�����
        if (PhotonNetwork.IsConnected == false) PhotonNetwork.ConnectUsingSettings();
    }

    // �}�X�^�[�T�[�o�[�ւ̐ڑ��������������ɌĂ΂��R�[���o�b�N
    public override void OnConnectedToMaster()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;
        // "Room"�Ƃ������O�̃��[���ɎQ������i���[�������݂��Ȃ���΍쐬���ĎQ������j
        PhotonNetwork.JoinOrCreateRoom("Room", roomOptions, TypedLobby.Default);
    }

    // room�ɎQ�������Ƃ�
    public override void OnJoinedRoom()
    {
        // Player�𐶐����ARespawn��Trasform������������
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity).GetComponent<PlayerManager>().RespawnPosition();
    }

    /// <summary>room�ւ̎Q���Ɏ��s����</summary>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed: " + message);
    }
}