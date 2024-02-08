using UnityEngine;
using UnityEngine.UI;

public class CrosshairCntlr : MonoBehaviour
{
    [SerializeField] RectTransform _crosshair;
    [SerializeField] MoveType _moveType;
    [SerializeField] float _sizeChangeRate = 2f;
    [Space(10)] // 以下hit marker
    [SerializeField] GameObject _hitMarker;

    RectTransform _hitMarkerRT;
    Image _hitMarkerImage;

    float _initialSize;
    float _targetSizeDelta;
    float _timeItTake = 0.1f;

    float _hitMarkerCurrentAlpha = 0;
    Color _hitMarkerColor = new Color(1, 1, 1, 0); // 初期 透明な白

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

    /// <summary>サイズを反映させる</summary>
    void ReflectSizeDelta()
    {
        Vector2 currentSizeDelta = _crosshair.sizeDelta;
        Vector2 targetSizeDelta = new Vector2(_targetSizeDelta, _targetSizeDelta);

        // clampが必要ならclampする

        Vector2 smoothedSize = Vector2.Lerp(currentSizeDelta, targetSizeDelta, _timeItTake); // 遷移
        _crosshair.sizeDelta = smoothedSize; // 反映
    }

    /// <summary>サイズを反映させる</summary>
    void ReflectScale()
    {
        Vector2 currentScale = _crosshair.localScale;
        Vector2 targetScale = new Vector2(_targetSizeDelta - 107, _targetSizeDelta - 107);
        Vector2 smoothedScale = Vector2.Lerp(currentScale, targetScale, _timeItTake); // 遷移

        _crosshair.localScale = smoothedScale;
    }

    /// <summary>ヒットマーカーのfadeを計算し反映</summary>
    void ReflectHitMarkerFade()
    {
        _hitMarkerColor.a = _hitMarkerCurrentAlpha;
        _hitMarkerCurrentAlpha -= Time.deltaTime; // aの減少
        _hitMarkerImage.color = _hitMarkerColor;
    }

    /// <summary>サイズをセットする</summary>
    public void SetSize(float size)
    {
        _targetSizeDelta = _initialSize + size * _sizeChangeRate;
    }

    /// <summary>damagebleに当てたときの処理</summary>
    public void OnHit(bool isHead)
    {
        ChangeHitMarkerColor(isHead);
        _hitMarkerCurrentAlpha = 1;
    }

    /// <summary>ヒットマーカーの色を変える</summary>
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

    /// <summary>画面表示を切り替える</summary>
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