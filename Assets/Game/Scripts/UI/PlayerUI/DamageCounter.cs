using TMPro;
using UnityEngine;
using DG.Tweening;

/// <summary>damega count ui controller</summary>
public class DamageCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] _textObjs;
    [SerializeField, Range(0f, 1f), Tooltip("ダメージテキストが別のobjになるまでの時間")] float _changeTextTime;
    [SerializeField] float _fadeTime;
    [Header("position")]
    [SerializeField, Tooltip("UIが追尾するターゲット")] Transform _target;
    [SerializeField] float _worldOffsetY;
    [SerializeField] Vector3 _screenOffset = Vector3.zero;

    int _currentTextObjIndex = 0;
    Vector3 _targetPosOnHit;
    Vector3 _notCurrentPos;
    int _totalDmg;
    float _timer = 1f;
    bool _faded = false;

    private void Update()
    {
        SetPosition();

        if (_timer <= _changeTextTime) _timer += Time.deltaTime;
        else if (!_faded) FadeOutText();

        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    DamageUpdate(14);
        //}
    }

    void SetPosition()
    {
        for (int i = 0; i < _textObjs.Length; i++)
        {
            if (i == _currentTextObjIndex)
            {
                Vector3 thisPos = Camera.main.WorldToScreenPoint(_targetPosOnHit + Vector3.up * _worldOffsetY) + _screenOffset;

                if (thisPos.z > 0)
                {
                    _textObjs[_currentTextObjIndex].transform.position = thisPos;
                }
            }
            else
            {
                Vector3 thisPos = Camera.main.WorldToScreenPoint(_notCurrentPos + Vector3.up * _worldOffsetY) + _screenOffset;

                if (thisPos.z > 0)
                {
                    _textObjs[i].transform.position = thisPos;
                }
            }
        }
    }

    void TargetPosUpdate()
    {
        _targetPosOnHit = _target.position;
    }

    public void DamageUpdate(int dmg)
    {
        if (_timer > _changeTextTime)
        {
            _faded = false;
            _textObjs[_currentTextObjIndex].DOFade(1, 0);
            _totalDmg = dmg;
        }
        else
        {
            _totalDmg += dmg;
        }

        _textObjs[_currentTextObjIndex].text = _totalDmg.ToString(); // テキスト更新
        TargetPosUpdate(); // ポジション更新
        _timer = 0f; // reset timer
    }

    void FadeOutText()
    {
        _faded = true;
        _notCurrentPos = _targetPosOnHit;
        _textObjs[_currentTextObjIndex].DOFade(0, _fadeTime);
        _textObjs[_currentTextObjIndex].rectTransform.DOMoveY(_textObjs[_currentTextObjIndex].rectTransform.position.y + 20, _fadeTime);
        _currentTextObjIndex++;
        _currentTextObjIndex %= _textObjs.Length;
    }
}