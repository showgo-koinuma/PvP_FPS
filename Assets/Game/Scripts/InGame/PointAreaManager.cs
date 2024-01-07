using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PointAreaManager : MonoBehaviourPun
{
    [Header("Area renge")]
    [SerializeField, Tooltip("obj�ʒu�����delta")] Vector3 _center = Vector3.zero;
    [SerializeField, Tooltip("center�𒆐S�Ƃ����T�C�Y")] Vector3 _size = Vector3.one;

    [Header("Area system")]
    [SerializeField, Tooltip("area���m�ۂ���X�s�[�h(per/s)")] float _takeAreaPerSpeed;
    [SerializeField] float _lostAreaPerSpeed;

    [Header("UI")]
    [SerializeField] Image _takingPerImage;
    [SerializeField] Image _areaOwnerImage;
    [SerializeField] Color[] _teamColor;
 
    [Header("Gizmos")]
    [SerializeField, Tooltip("Gizmos�̕\���ؑ�")] bool _visible = true;
    [SerializeField] Color _color = Color.yellow;

    Transform _myPlayer;

    // Area�̏��
    bool _isMaster;
    bool _masterInArea;
    bool _otherInArea;
    AreaOwner _areaOwner = AreaOwner.anyone;
    public AreaOwner AreaOwner { get => _areaOwner; }
    AreaState _areaState = AreaState.anyone;
    public AreaState AreaState { get => _areaState; }
    AnyoneInArea _anyoneInArea = 0;

    // area value
    float _masterTakePer = 0;
    float _otherTakePer = 0;

    private void Awake()
    {
        _isMaster = PhotonNetwork.IsMasterClient;

        if (!_isMaster)
        {
            Color masterColor = _teamColor[0];
            _teamColor[0] = _teamColor[1];
            _teamColor[1] = masterColor;
        }
    }

    public void SetPlayerTransform(Transform player)
    {
        _myPlayer = player;
    }

    private void Update()
    {
        AreaStateUpdate();
        UIUpdate();
    }

    /// <summary>player���G���A���ɂ��邩���肵�AperValue��state���X�V����</summary>
    void AreaStateUpdate()
    {
        if (_isMaster) // master���̏���
        {
            if (_masterInArea != CheckArea(_myPlayer))
            {
                _masterInArea = !_masterInArea;

                if (_masterInArea)
                {
                    _anyoneInArea |= AnyoneInArea.master;
                }
                else
                {
                    _anyoneInArea &= ~AnyoneInArea.master;
                }
            }

            AreaState lastAreaState = _areaState;
            AreaOwner lastAreaOwner = _areaOwner;
            _areaState = AreaState.anyone;

            // AnyoneInArea���珊�L�󋵂��v�Z
            if (_anyoneInArea == 0) // �N�����Ȃ�
            {
                if (_masterTakePer > 0)
                {
                    _masterTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                }
                else
                {
                    _masterTakePer = 0;
                }
                if (_otherTakePer > 0)
                {
                    _otherTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                }
                else
                {
                    _otherTakePer = 0;
                }
            }
            else if (_anyoneInArea == AnyoneInArea.master) // master����
            {
                if (_areaOwner != AreaOwner.master)
                {
                    _masterTakePer += _takeAreaPerSpeed * Time.deltaTime;
                    _otherTakePer = 0;
                    _areaState = AreaState.masterTaking;

                    if (_masterTakePer >= 1)
                    {
                        _masterTakePer = 0;
                        _areaOwner = AreaOwner.master;
                    }
                }
                else if (_otherTakePer > 0)
                {
                    _otherTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                }
                else
                {
                    _otherTakePer = 0;
                }
            }
            else if (_anyoneInArea == AnyoneInArea.other) // other����
            {
                if (_areaOwner != AreaOwner.other)
                {
                    _otherTakePer += _takeAreaPerSpeed * Time.deltaTime;
                    _masterTakePer = 0;
                    _areaState = AreaState.otherTaking;

                    if (_otherTakePer >= 1)
                    {
                        _otherTakePer = 0;
                        _areaOwner = AreaOwner.other;
                    }
                }
                else if (_masterTakePer > 0)
                {
                    _masterTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                }
                else
                {
                    _masterTakePer = 0;
                }
            }
            else // ��������
            {
                _areaState = AreaState.both;
            }

            if (lastAreaState != _areaState || lastAreaOwner != _areaOwner) // �󋵂��ς�����狤�L
            {
                photonView.RPC(nameof(SynchroAreaSituation), RpcTarget.Others, (int)_areaState, (int)_areaOwner); // other�ɋ��L
            }
        }
        else // master�łȂ��ꍇ�͏󋵂����L���邾���@�ŏI�I�Ȕ����master���s��
        {
            if (_otherInArea != CheckArea(_myPlayer)) // �������L
            {
                _otherInArea = !_otherInArea;
                photonView.RPC(nameof(SynchroInAreaOther), RpcTarget.Others, _otherInArea);
            }

            // �G���A�����肩�珊�L�󋵂��v�Z
            if (_areaState == AreaState.both) return;

            if (_areaOwner == AreaOwner.master)
            {
                _masterTakePer = 0;

                if (_areaState != AreaState.otherTaking)
                {
                    if (_otherTakePer > 0) _otherTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                    else _otherTakePer = 0;
                }
            }
            else if (_areaOwner == AreaOwner.other)
            {
                _otherTakePer = 0;

                if (_areaState != AreaState.masterTaking)
                {
                    if (_masterTakePer > 0) _masterTakePer -= _lostAreaPerSpeed * Time.deltaTime;
                    else _masterTakePer = 0;
                }
            }

            if (_areaState == AreaState.masterTaking && _areaOwner != AreaOwner.master)
            {
                _masterTakePer += _takeAreaPerSpeed * Time.deltaTime;
                _otherTakePer = 0;
            }
            else if (_areaState == AreaState.otherTaking && _areaOwner != AreaOwner.other)
            {
                _otherTakePer += _takeAreaPerSpeed * Time.deltaTime;
                _masterTakePer = 0;
            }

        }
    }

    /// <summary>other���G���A��player�������L���邽�߂̂���</summary>
    [PunRPC]
    void SynchroInAreaOther(bool isIn)
    {
        if (isIn)
        {
            _anyoneInArea |= AnyoneInArea.other;
        }
        else
        {
            _anyoneInArea &= ~AnyoneInArea.other;
        }
    }
    /// <summary>other�ɃG���A�󋵂����L����</summary>
    [PunRPC]
    void SynchroAreaSituation(int areaState, int areaOwner)
    {
        if (!_isMaster)
        {
            _areaState = (AreaState)Enum.ToObject(typeof(AreaState), areaState);
            _areaOwner = (AreaOwner)Enum.ToObject(typeof(AreaOwner), areaOwner);
        }
    }

    /// <summary>�����f�[�^�X�V��UI�ɔ��f������</summary>
    void UIUpdate()
    {
        _takingPerImage.fillAmount = Mathf.Max(_masterTakePer, _otherTakePer);

        if (_areaState == AreaState.masterTaking) _takingPerImage.color = _teamColor[0];
        else if (_areaState == AreaState.otherTaking) _takingPerImage.color = _teamColor[1];

        if (_areaOwner == AreaOwner.master) _areaOwnerImage.color = _teamColor[0];
        else if (_areaOwner == AreaOwner.other) _areaOwnerImage.color= _teamColor[1];
        else _areaOwnerImage.color = _teamColor[2];
    }

    bool CheckArea(Transform player)
    {
        Vector3 distance = transform.position - player.position;
        return Mathf.Abs(distance.x) < _size.x / 2 &&
            Mathf.Abs(distance.y) < _size.y / 2 &&
            Mathf.Abs(distance.z) < _size.z / 2;
    }

    /// <summary>area�̏��L�󋵂����Z�b�g������</summary> // �^�C�~���O�ނ�����
    public void ResetArea()
    {
        _masterTakePer = 0;
        _otherTakePer = 0;
    }

    void OnDrawGizmos()
    {
        if (!_visible)
        {
            return;
        }

        Gizmos.color = _color;

        Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
        Gizmos.DrawWireCube(_center, _size);
    }
}

public enum AreaOwner
{
    anyone,
    master,
    other
}

public enum AreaState
{
    anyone,       // �N�����Ȃ� 
    both,         // �ǂ��������
    masterTaking,
    otherTaking,
}

[Flags]
enum AnyoneInArea
{
    master = 1,
    other = 2
}