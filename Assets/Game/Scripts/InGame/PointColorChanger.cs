using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PointColorChanger : MonoBehaviour
{
    [Header("[0]:blue, [1]:red, [2]:default")]
    [SerializeField] Color[] _teamColor;
    [SerializeField] Color[] _holoColor;
    [SerializeField] Color[] _objColor;
    [Header("Map Obj")]
    [SerializeField] Light[] _lights;
    [SerializeField] Material _holoMat;
    [SerializeField] Material _changeColorObjMat;
    [Header("UI")]
    [SerializeField] Image _takingPercentImage;
    [SerializeField] Image _areaOwnerImage;

    private void Awake()
    {
        _holoMat.SetColor("_Color", _holoColor[2]);
        _changeColorObjMat.SetColor("_Color", _objColor[2]);
    }

    public void UpdatePerUI(AreaState areaState, float value)
    {
        if (areaState == AreaState.masterTaking)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _takingPercentImage.color = _teamColor[0];
            }
            else
            {
                _takingPercentImage.color = _teamColor[1];
            }
        }
        else if (areaState == AreaState.otherTaking)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _takingPercentImage.color = _teamColor[1];
            }
            else
            {
                _takingPercentImage.color = _teamColor[0];
            }
        }

        _takingPercentImage.fillAmount = value;
    }

    public void ChangeColor(bool toBlue)
    {
        foreach (Light light in _lights)
        {
            light.color = toBlue? _teamColor[0] : _teamColor[1];
        }

        _holoMat.SetColor("_Color", toBlue? _holoColor[0] : _holoColor[1]);
        _changeColorObjMat.SetColor("_Color", toBlue? _objColor[0] : _objColor[1]);

        _areaOwnerImage.color = toBlue ? _teamColor[0] : _teamColor[1];
    }
}
