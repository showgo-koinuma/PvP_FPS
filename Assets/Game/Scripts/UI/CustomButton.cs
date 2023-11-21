using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Events;
using Unity.VisualScripting;

/// <summary>画像をボタンにする</summary>
public class CustomButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Tooltip("ボタンの実行内容")] public UnityEvent _buttonAction;
    public event Action ButtonAction;
    CanvasGroup _canvasGroup;

    private void Awake() => _canvasGroup = GetComponent<CanvasGroup>();
    
    // タップ クリックしたときの処理を実行
    public void OnPointerClick(PointerEventData eventData) 
    {
        _buttonAction?.Invoke(); // Actionが設定されてないときはDebugを出したい
        ButtonAction?.Invoke();
    }
    // カーソルが重なる
    public void OnPointerEnter(PointerEventData eventData) { }
    // カーソルが離れる
    public void OnPointerExit(PointerEventData eventData) { }

    // クリックDown
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOScale(0.95f, 0.24f).SetEase(Ease.OutCubic);
        _canvasGroup.DOFade(0.8f, 0.24f).SetEase(Ease.OutCubic);
    }
    // クリックUp
    public void OnPointerUp(PointerEventData eventData) 
    {
        transform.DOScale(1f, 0.24f).SetEase(Ease.OutCubic);
        _canvasGroup.DOFade(1f, 0.24f).SetEase(Ease.OutCubic);
    }
}