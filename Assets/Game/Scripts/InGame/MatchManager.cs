using Photon.Pun;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class MatchManager : MonoBehaviourPun
{
    [SerializeField] PointAreaManager _masterArea;
    //[SerializeField] PointAreaManager _otherArea;

    [SerializeField] float _areaCountUpSpeed = 1f;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI _myCountText;
    [SerializeField] TextMeshProUGUI _otherCountText;
    // �Q�[���I����
    [SerializeField, Tooltip("�Q�[���I�����ɕ\������L�����o�X")] GameObject _gameOverCanvas;
    [SerializeField] TextMeshProUGUI _winOrLossText;
    [SerializeField] Image _resultFadePanel;

    static MatchManager _instance;
    public static MatchManager Instance { get => _instance; }

    GameState _gameState = GameState.InGame;
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
        _gameOverCanvas.SetActive(false); // �I�����L�����o�X��\��
    }

    /// <summary>�G���A�Ƀv���C���[��o�^����</summary>
    public void SetPlayerToArea(Transform player)
    {
        if (_masterArea != null) _masterArea.SetPlayerTransform(player);
        //if (_otherArea != null) _otherArea.SetPlayerTransform(player, isMaster);
    }

    /// <summary>AreaOwner����J�E���g���X�V����</summary>
    void AreaCountUpdate()
    {
        if (_thisIsMaster && _gameState == GameState.InGame) // master����match�󋵂͏������邱�ƂƂ���
        {
            if (_masterArea.AreaOwner == AreaOwner.master)
            {
                _masterAreaCount += _areaCountUpSpeed * Time.deltaTime;

                if (_masterAreaCount >= _masterUICount + 1)
                {
                    _masterUICount++;
                    photonView.RPC(nameof(SynchroAreaCountText), RpcTarget.All, _masterUICount, _otherUICount);

                    if (_masterUICount >= 10) // test 10
                    {
                        photonView.RPC(nameof(GameOver), RpcTarget.All, true);
                    }
                }
            }
            else if (_masterArea.AreaOwner == AreaOwner.other)
            {
                _otherAreaCount += _areaCountUpSpeed * Time.deltaTime;

                if (_otherAreaCount >= _otherUICount + 1) 
                {
                    _otherUICount++;
                    photonView.RPC(nameof(SynchroAreaCountText), RpcTarget.All, _masterUICount, _otherUICount);

                    if (_otherUICount >= 10) // test 10
                    {
                        photonView.RPC(nameof(GameOver), RpcTarget.All, false);
                    }
                }
            }
        }
    }

    /// <summary>UI�X�V�����L</summary>
    [PunRPC]
    void SynchroAreaCountText(int masterCount, int otherCount)
    {
        if (_thisIsMaster)
        {
            _myCountText.text = masterCount.ToString("D2");
            _otherCountText.text = otherCount.ToString("D2");
        }
        else
        {
            _otherCountText.text = masterCount.ToString("D2");
            _myCountText.text = otherCount.ToString("D2");
        }
    }

    [PunRPC]
    void GameOver(bool winMaster)
    {
        if (_thisIsMaster)
        {
            StartCoroutine(GameOverUI(winMaster));
        }
        else
        {
            StartCoroutine(GameOverUI(!winMaster));
        }

        _gameState = GameState.Result;
    }

    IEnumerator GameOverUI(bool isWin)
    {
        if (isWin)
        {
            _gameOverCanvas.SetActive(true);
            _winOrLossText.text = "Victory";
        }
        else
        {
            _gameOverCanvas.SetActive(true);
            _winOrLossText.text = "Defeat";
        }

        yield return new WaitForSeconds(2); // fade�J�n�܂ł�delay

        _resultFadePanel.DOFade(1, 1);
    }

    private void Update()
    {
        AreaCountUpdate();
    }
}

public enum GameState
{
    Ready,
    InGame,
    Result
}