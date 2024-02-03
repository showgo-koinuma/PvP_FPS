using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>�摜���{�^���ɂ���</summary>
public class CustomButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Tooltip("�{�^���̎��s���e")] public UnityEvent _buttonAction;
    [SerializeField] TMP_Text _buttonText;
    [SerializeField] Image _changeColorImage;
    [SerializeField] Color _inactiveColor;
    [SerializeField] AudioClip _clickSound;

    public Action ButtonAction; // script������ݒ�o����

    CanvasGroup _canvasGroup;
    AudioSource _audioSource;
    Vector3 _defaultScale;
    Color _activeColor;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _audioSource = GetComponent<AudioSource>();
        _defaultScale = transform.localScale;
        _activeColor = _changeColorImage.color;
    }
    
    // �^�b�v �N���b�N�����Ƃ��̏��������s
    public void OnPointerClick(PointerEventData eventData) 
    {
        _buttonAction?.Invoke(); // Action���ݒ肳��ĂȂ��Ƃ���Debug���o������
        ButtonAction?.Invoke();

        if (_clickSound) _audioSource.PlayOneShot(_clickSound);
    }

    public void ChangeButtonState(bool isActive, string buttonText, Action newAction = null)
    {
        _changeColorImage.color = isActive? _activeColor : _inactiveColor; // active�ɉ����ĐF��ς���
        _buttonText.text = buttonText;
        ButtonAction = newAction;
    }

    // �J�[�\�����d�Ȃ�
    public void OnPointerEnter(PointerEventData eventData) { }

    // �J�[�\���������
    public void OnPointerExit(PointerEventData eventData) { }

    // �N���b�NDown
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOScale(_defaultScale * 0.95f, 0.24f).SetEase(Ease.OutCubic);
        _canvasGroup.DOFade(0.8f, 0.24f).SetEase(Ease.OutCubic);
    }

    // �N���b�NUp
    public void OnPointerUp(PointerEventData eventData) 
    {
        transform.DOScale(_defaultScale, 0.24f).SetEase(Ease.OutCubic);
        _canvasGroup.DOFade(1f, 0.24f).SetEase(Ease.OutCubic);
    }
}