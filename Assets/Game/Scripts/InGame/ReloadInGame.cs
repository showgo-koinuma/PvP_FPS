using Photon.Pun;

public class ReloadInGame : MonoBehaviourPun
{
    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient) photonView.RPC(nameof(ReloadScene), RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    void ReloadScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }
}
