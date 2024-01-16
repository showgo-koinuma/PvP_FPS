using UnityEngine;

public class CrosshairCntlr : MonoBehaviour
{
    [SerializeField] MoveType _moveType;
    [SerializeField] float _sizeChangeRate = 2f;

    RectTransform _rectTF;

    float _initialSize;
    float _targetSizeDelta;
    float _timeItTake = 0.1f;

    private void Awake()
    {
        _rectTF = GetComponent<RectTransform>();
        _initialSize = _rectTF.sizeDelta.x;
    }

    void Update()
    {
        if (_moveType == MoveType.SizeDelta)
        {
            ReflectSizeDelta();
        }
        else
        {
            ReflectScale();
        }
    }

    /// <summary>�T�C�Y�𔽉f������</summary>
    void ReflectSizeDelta()
    {
        Vector2 currentSizeDelta = _rectTF.sizeDelta;
        Vector2 targetSizeDelta = new Vector2(_targetSizeDelta, _targetSizeDelta);

        // clamp���K�v�Ȃ�clamp����

        Vector2 smoothedSize = Vector2.Lerp(currentSizeDelta, targetSizeDelta, _timeItTake); // �J��
        _rectTF.sizeDelta = smoothedSize; // ���f
    }

    void ReflectScale()
    {
        Vector2 currentScale = _rectTF.localScale;
        Vector2 targetScale = new Vector2(_targetSizeDelta - 7f, _targetSizeDelta - 7f);
        Vector2 smoothedScale = Vector2.Lerp(currentScale, targetScale, _timeItTake); // �J��
        Debug.Log(_targetSizeDelta);

        _rectTF.localScale = smoothedScale;
    }

    /// <summary>�T�C�Y���Z�b�g����</summary>
    public void SetSize(float size)
    {
        _targetSizeDelta = _initialSize + size * _sizeChangeRate;
    }

    /// <summary>��ʕ\����؂�ւ���</summary>
    public void SwitchDisplay(bool isDisplay)
    {
        gameObject.SetActive(isDisplay);
    }
}

enum MoveType
{
    SizeDelta,
    Scale
}