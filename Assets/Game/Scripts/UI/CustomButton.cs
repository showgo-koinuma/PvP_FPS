using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>画像をボタンにする</summary>
public class CustomButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Tooltip("ボタンの実行内容")] public UnityEvent _buttonAction;
    [SerializeField] TMP_Text _buttonText;
    [SerializeField] Image _changeColorImage;
    [SerializeField] Color _inactiveColor;
    [SerializeField] AudioClip _clickSound;

    public Action ButtonAction; // scriptからも設定出来る

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
    
    // タップ クリックしたときの処理を実行
    public void OnPointerClick(PointerEventData eventData) 
    {
        _buttonAction?.Invoke(); // Actionが設定されてないときはDebugを出したい
        ButtonAction?.Invoke();

        if (_clickSound) _audioSource.PlayOneShot(_clickSound);
    }

    public void ChangeButtonState(bool isActive, string buttonText, Action newAction = null)
    {
        _changeColorImage.color = isActive? _activeColor : _inactiveColor; // activeに応じて色を変える
        _buttonText.text = buttonText;
        ButtonAction = newAction;
    }

    // カーソルが重なる
    public void OnPointerEnter(PointerEventData eventData) { }

    // カーソルが離れる
    public void OnPointerExit(PointerEventData eventData) { }

    // クリックDown
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOScale(_defaultScale * 0.95f, 0.24f).SetEase(Ease.OutCubic);
        _canvasGroup.DOFade(0.8f, 0.24f).SetEase(Ease.OutCubic);
    }

    // クリックUp
    public void OnPointerUp(PointerEventData eventData) 
    {
        transform.DOScale(_defaultScale, 0.24f).SetEase(Ease.OutCubic);
        _canvasGroup.DOFade(1f, 0.24f).SetEase(Ease.OutCubic);
    }
}