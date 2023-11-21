using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Events;
using Unity.VisualScripting;

/// <summary>�摜���{�^���ɂ���</summary>
public class CustomButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Tooltip("�{�^���̎��s���e")] public UnityEvent _buttonAction;
    public event Action ButtonAction;
    CanvasGroup _canvasGroup;

    private void Awake() => _canvasGroup = GetComponent<CanvasGroup>();
    
    // �^�b�v �N���b�N�����Ƃ��̏��������s
    public void OnPointerClick(PointerEventData eventData) 
    {
        _buttonAction?.Invoke(); // Action���ݒ肳��ĂȂ��Ƃ���Debug���o������
        ButtonAction?.Invoke();
    }
    // �J�[�\�����d�Ȃ�
    public void OnPointerEnter(PointerEventData eventData) { }
    // �J�[�\���������
    public void OnPointerExit(PointerEventData eventData) { }

    // �N���b�NDown
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOScale(0.95f, 0.24f).SetEase(Ease.OutCubic);
        _canvasGroup.DOFade(0.8f, 0.24f).SetEase(Ease.OutCubic);
    }
    // �N���b�NUp
    public void OnPointerUp(PointerEventData eventData) 
    {
        transform.DOScale(1f, 0.24f).SetEase(Ease.OutCubic);
        _canvasGroup.DOFade(1f, 0.24f).SetEase(Ease.OutCubic);
    }
}