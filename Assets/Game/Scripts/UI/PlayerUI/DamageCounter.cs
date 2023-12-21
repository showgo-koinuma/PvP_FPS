using TMPro;
using UnityEngine;

/// <summary>damega count ui controller</summary>
public class DamageCounter : MonoBehaviour
{
    [SerializeField, Tooltip("UIが追尾するターゲット")] Transform _target;
    [SerializeField] float _worldOffsetY;
    [SerializeField] Vector3 _screenOffset = Vector3.zero;

    TextMeshProUGUI _textMeshPeo;

    private void Awake()
    {
        _textMeshPeo = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        SetPosition();
    }

    private void SetPosition()
    {
        //Vector3 worldOffset = Camera.main.transform.TransformDirection(_worldOffset);
        //worldOffset.y = _worldOffset.y;
        Vector3 thisPos = Camera.main.WorldToScreenPoint(_target.position + Vector3.up * _worldOffsetY) + _screenOffset;

        if (thisPos.z > 0)
        {
            Debug.Log(thisPos);
            transform.position = thisPos;
        }
    }
}