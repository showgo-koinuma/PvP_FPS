using UnityEngine;
using UnityEngine.UI;

public class CrosshairCntlr : MonoBehaviour
{
    [SerializeField] RectTransform _crosshair;
    [SerializeField] MoveType _moveType;
    [SerializeField] float _sizeChangeRate = 2f;
    [Space(10)] // �ȉ�hit marker
    [SerializeField] GameObject _hitMarker;

    RectTransform _hitMarkerRT;
    Image _hitMarkerImage;

    float _initialSize;
    float _targetSizeDelta;
    float _timeItTake = 0.1f;

    float _hitMarkerCurrentAlpha = 0;
    Color _hitMarkerColor = new Color(1, 1, 1, 0); // ���� �����Ȕ�

    private void Awake()
    {
        _hitMarkerRT = _hitMarker.GetComponent<RectTransform>();
        _hitMarkerImage = _hitMarker.GetComponent<Image>();
        _hitMarkerImage.color = _hitMarkerColor;
        _initialSize = _crosshair.sizeDelta.x;
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

        ReflectHitMarkerFade();
    }

    /// <summary>�T�C�Y�𔽉f������</summary>
    void ReflectSizeDelta()
    {
        Vector2 currentSizeDelta = _crosshair.sizeDelta;
        Vector2 targetSizeDelta = new Vector2(_targetSizeDelta, _targetSizeDelta);

        // clamp���K�v�Ȃ�clamp����

        Vector2 smoothedSize = Vector2.Lerp(currentSizeDelta, targetSizeDelta, _timeItTake); // �J��
        _crosshair.sizeDelta = smoothedSize; // ���f
    }

    /// <summary>�T�C�Y�𔽉f������</summary>
    void ReflectScale()
    {
        Vector2 currentScale = _crosshair.localScale;
        Vector2 targetScale = new Vector2(_targetSizeDelta - 107, _targetSizeDelta - 107);
        Vector2 smoothedScale = Vector2.Lerp(currentScale, targetScale, _timeItTake); // �J��

        _crosshair.localScale = smoothedScale;
    }

    /// <summary>�q�b�g�}�[�J�[��fade���v�Z�����f</summary>
    void ReflectHitMarkerFade()
    {
        _hitMarkerColor.a = _hitMarkerCurrentAlpha;
        _hitMarkerCurrentAlpha -= Time.deltaTime; // a�̌���
        _hitMarkerImage.color = _hitMarkerColor;
    }

    /// <summary>�T�C�Y���Z�b�g����</summary>
    public void SetSize(float size)
    {
        _targetSizeDelta = _initialSize + size * _sizeChangeRate;
    }

    /// <summary>damageble�ɓ��Ă��Ƃ��̏���</summary>
    public void OnHit(bool isHead)
    {
        ChangeHitMarkerColor(isHead);
        _hitMarkerCurrentAlpha = 1;
    }

    /// <summary>�q�b�g�}�[�J�[�̐F��ς���</summary>
    void ChangeHitMarkerColor(bool isHead)
    {
        if (isHead)
        {
            _hitMarkerColor = Color.red;
        }
        else
        {
            _hitMarkerColor = Color.white;
        }
    }

    /// <summary>��ʕ\����؂�ւ���</summary>
    public void SwitchDisplay(bool isDisplay)
    {
        _crosshair.gameObject.SetActive(isDisplay);
    }
}

enum MoveType
{
    SizeDelta,
    Scale
}