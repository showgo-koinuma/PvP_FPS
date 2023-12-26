using UnityEngine;
using UnityEngine.UI;

public class PointAreaManager : MonoBehaviour
{
    [Header("Area renge")]
    [SerializeField, Tooltip("obj位置からのdelta")] Vector3 _center = Vector3.zero;
    [SerializeField, Tooltip("centerを中心としたサイズ")] Vector3 _size = Vector3.one;

    [Header("Area system")]
    [SerializeField, Tooltip("areaを確保するスピード(per/s)")] float _takeAreaPerSpeed;
    [SerializeField] float _lostAreaPerSpeed;

    [Header("UI")]
    [SerializeField] Image _takingPerImage;
    [SerializeField] Image _areaOwnerImage;
    [SerializeField] Color[] _teamColor;
 
    [Header("Gizmos")]
    [SerializeField, Tooltip("Gizmosの表示切替")] bool _visible = true;
    [SerializeField] Color _color = Color.yellow;

    Transform _master, _other;

    AreaState _areaState = AreaState.nomal;
    public AreaState AreaState { get => _areaState; }
    AreaOwner _areaOwner = AreaOwner.anyone;
    public AreaOwner AreaOwner { get => _areaOwner; }
    WhoTaking _whoTaking = WhoTaking.master;

    // area value
    float _masterTakePer;
    float _otherTakePer;

    public void SetPlayerTransform(Transform player, bool isMaster)
    {
        if (isMaster) _master = player;
        else _other = player;
    }

    private void Update()
    {
        AreaStateUpdate();
        UIUpdate();
    }

    /// <summary>playerがエリア内にいるか判定し、perValueとstateを更新する</summary>
    void AreaStateUpdate()
    {
        bool masterTaking = CheckArea(_master);
        bool otherTaking = false;
        if (_other != null) otherTaking = CheckArea(_other); // debug用だけど

        if (masterTaking && !otherTaking) // masterだけ
        {
            if (_masterTakePer < 1)
            {
                _masterTakePer += _takeAreaPerSpeed * Time.deltaTime;
                _whoTaking = WhoTaking.master;
            }
            else
            {
                _masterTakePer = 1;
                _areaOwner = AreaOwner.master; // 所有者変更
            }

            
        }
        else if (!masterTaking && otherTaking) // otherだけ
        {
            if (_otherTakePer < 1)
            {
                _otherTakePer += _takeAreaPerSpeed * Time.deltaTime;
                _whoTaking = WhoTaking.other;
            }
            else
            {
                _otherTakePer = 1;
                _areaOwner = AreaOwner.other; // 所有者変更
            }
        }

        Debug.Log(_areaState);
    }

    /// <summary>内部データ更新をUIに反映させる</summary>
    void UIUpdate()
    {
        _takingPerImage.fillAmount = Mathf.Max(_masterTakePer, _otherTakePer);

        if (_whoTaking == WhoTaking.master) _takingPerImage.color = _teamColor[0];
        else _takingPerImage.color = _teamColor[1];

        //if (_areaOwner == AreaOwner.master) _takingPerImage.color = _teamColor[0];
        //else if (_areaOwner == AreaOwner.other) _takingPerImage.color = _teamColor[1];
        //else _takingPerImage.color = _teamColor[2];
    }

    bool CheckArea(Transform player)
    {
        Vector3 distance = transform.position - player.position;
        return Mathf.Abs(distance.x) < _size.x / 2 &&
            Mathf.Abs(distance.y) < _size.y / 2 &&
            Mathf.Abs(distance.z) < _size.z / 2;
    }

    /// <summary>areaの所有状況をリセットさせる</summary> // タイミングむずいか
    public void ResetArea()
    {
        _areaState = AreaState.nomal;
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

public enum AreaState
{
    nomal,
    masterTaking,
    otherTaking,
    masterGet,
    otherGet
}

public enum AreaOwner
{
    anyone,
    master,
    other
}

public enum WhoTaking
{
    master,
    other
}