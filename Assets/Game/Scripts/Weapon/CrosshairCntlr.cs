using UnityEngine;

public class CrosshairCntlr : MonoBehaviour
{
    RectTransform _rectTF;

    float _initialSize;
    /// <summary>�T�C�Y�ύX�ɂ����鎞��</summary>
    float _targetSize;
    float _timeItTake = 0.1f;

    private void Awake()
    {
        _rectTF = GetComponent<RectTransform>();
        _initialSize = _rectTF.sizeDelta.x;
    }

    void Update()
    {
        ReflectSize();
    }

    /// <summary>�T�C�Y�𔽉f������</summary>
    void ReflectSize()
    {
        Vector2 currentSizeDelta = _rectTF.sizeDelta;
        Vector2 targetSizeDelta = new Vector2(_targetSize, _targetSize);

        // clamp���K�v�Ȃ�clamp����

        Vector2 smoothedSize = Vector2.Lerp(currentSizeDelta, targetSizeDelta, _timeItTake); // �J��
        _rectTF.sizeDelta = smoothedSize; // ���f
    }

    /// <summary>�T�C�Y���Z�b�g����</summary>
    public void SetSize(float size)
    {
        _targetSize = _initialSize + size * 6;
    }

    /// <summary>��ʕ\����؂�ւ���</summary>
    public void SwitchDisplay(bool isDisplay)
    {
        gameObject.SetActive(isDisplay);
    }
}
