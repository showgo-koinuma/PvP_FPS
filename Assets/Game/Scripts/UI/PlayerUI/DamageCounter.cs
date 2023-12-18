using TMPro;
using UnityEngine;

/// <summary>damega count ui controller</summary>
public class DamageCounter : MonoBehaviour
{
    [SerializeField, Tooltip("UIが追尾するターゲット")] Transform _target;
    [SerializeField] Vector3 _offset = Vector3.zero;

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
        transform.position = Camera.main.WorldToScreenPoint(_target.position + _offset);
    }
}
