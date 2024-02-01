using Photon.Pun;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class MatchManager : MonoBehaviourPun
{
    [Header("GameRule")]
    [SerializeField] int _winCount = 5;
    [SerializeField] float _respawnTime;

    [Header("Area")]
    [SerializeField] PointAreaManager _masterArea;
    //[SerializeField] PointAreaManager _otherArea;
    [SerializeField] float _areaCountUpSpeed = 1f;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI _myCountText;
    [SerializeField] Image _myCoutnBackImage;
    [SerializeField] TextMeshProUGUI _otherCountText;
    [SerializeField] Image _otherCoutnBackImage;
    [SerializeField] Color[] _uiColors; // [0]:my color, [1]:other color, [2]:default back

    [Header("ゲーム終了時")]
    [SerializeField, Tooltip("inGameのキャンバス")] GameObject _inGameCanvas;
    [SerializeField] GameObject _pointCanves;
    [SerializeField, Tooltip("ゲーム終了時に表示するキャンバス")] GameObject _gameOverCanvas;
    [SerializeField] GameObject _winText;
    [SerializeField] GameObject _lossText;
    [SerializeField] GameObject _resultObj;
    [SerializeField] ResultManager _resultManager;
    [SerializeField, Tooltip("Fade用パネル")] Image _resultFadePanel;

    static MatchManager _instance;
    public static MatchManager Instance { get => _instance; }
    public float RespawnTime {  get => _respawnTime; }

    PlayerManager _minePlayer, _otherPlayer;
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

        _inGameCanvas.SetActive(true);
        _gameOverCanvas.SetActive(false); // 終了時キャンバス非表示
        _resultObj.SetActive(false);
    }

    /// <summary>エリアにプレイヤーを登録する</summary>
    public void SetPlayer(PlayerManager pManager, bool isMine)
    {
        if (isMine)
        {
            _minePlayer = pManager;
            if (_masterArea != null) _masterArea.SetPlayerTransform(pManager.transform);
            //if (_otherArea != null) _otherArea.SetPlayerTransform(player, isMaster);
        }
        else
        {
            _otherPlayer = pManager;
        }
    }

    /// <summary>AreaOwnerからカウントを更新する</summary>
    void AreaCountUpdate()
    {
        if (_thisIsMaster && InGameManager.Instance.GameState == GameState.InGame) // master側でmatch状況は処理することとする
        {
            if (_masterArea.AreaOwner == AreaOwner.master)
            {
                _masterAreaCount += _areaCountUpSpeed * Time.deltaTime;

                if (_masterAreaCount >= _masterUICount + 1)
                {
                    _masterUICount++;
                    photonView.RPC(nameof(SynchroAreaCountText), RpcTarget.All, _masterUICount, _otherUICount);

                    if (_masterUICount >= _winCount) // test 10
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

                    if (_otherUICount >= _winCount) // test 10
                    {
                        photonView.RPC(nameof(GameOver), RpcTarget.All, false);
                    }
                }
            }
        }
    }

    /// <summary>UI更新を共有</summary>
    [PunRPC]
    void SynchroAreaCountText(int masterCount, int otherCount)
    {
        if (_thisIsMaster)
        {
            _myCountText.text = masterCount.ToString("D2") + "%";
            _otherCountText.text = otherCount.ToString("D2") + "%";
        }
        else
        {
            _otherCountText.text = masterCount.ToString("D2") + "%";
            _myCountText.text = otherCount.ToString("D2") + "%";
        }
    }

    void ChangeCountUIColor()
    {
        if ((_thisIsMaster && _masterArea.AreaOwner == AreaOwner.master) || (!_thisIsMaster && _masterArea.AreaOwner == AreaOwner.other))
        {
            _myCountText.color = Color.white;
            _myCoutnBackImage.color = _uiColors[0];
            _otherCountText.color = _uiColors[1];
            _otherCoutnBackImage.color = _uiColors[2];
        }
        else if ((_thisIsMaster && _masterArea.AreaOwner == AreaOwner.other) || (!_thisIsMaster && _masterArea.AreaOwner == AreaOwner.master))
        {
            _myCountText.color = _uiColors[1];
            _myCoutnBackImage.color = _uiColors[2];
            _otherCountText.color = Color.white;
            _otherCoutnBackImage.color = _uiColors[0];
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

        InGameManager.Instance.GameState = GameState.Result;
    }

    IEnumerator GameOverUI(bool isWin)
    {
        // ingameのUIを消す
        _minePlayer.UIInvisibleOnGameOver();

        _inGameCanvas.SetActive(false);
        _pointCanves.SetActive(false);
        _gameOverCanvas.SetActive(true);

        if (isWin)
        {
            _winText.SetActive(true);
        }
        else
        {
            _lossText.SetActive(true);
        }

        yield return new WaitForSeconds(2); // fade開始までのdelay

        _resultFadePanel.DOFade(1, 1);

        yield return new WaitForSeconds(1); // fade終了までのdelay

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _resultFadePanel.DOFade(0, 1);
        _gameOverCanvas.SetActive(false);
        _resultObj.SetActive(true);
        _resultManager.InitializeResult(isWin, _otherPlayer ? _otherPlayer.DeadCount : 0, _minePlayer.DeadCount, 1000, 500, 500);
    }

    private void Update()
    {
        AreaCountUpdate();

    }
}