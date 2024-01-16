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

    /// <summary>サイズを反映させる</summary>
    void ReflectSizeDelta()
    {
        Vector2 currentSizeDelta = _rectTF.sizeDelta;
        Vector2 targetSizeDelta = new Vector2(_targetSizeDelta, _targetSizeDelta);

        // clampが必要ならclampする

        Vector2 smoothedSize = Vector2.Lerp(currentSizeDelta, targetSizeDelta, _timeItTake); // 遷移
        _rectTF.sizeDelta = smoothedSize; // 反映
    }

    void ReflectScale()
    {
        Vector2 currentScale = _rectTF.localScale;
        Vector2 targetScale = new Vector2(_targetSizeDelta - 7f, _targetSizeDelta - 7f);
        Vector2 smoothedScale = Vector2.Lerp(currentScale, targetScale, _timeItTake); // 遷移
        Debug.Log(_targetSizeDelta);

        _rectTF.localScale = smoothedScale;
    }

    /// <summary>サイズをセットする</summary>
    public void SetSize(float size)
    {
        _targetSizeDelta = _initialSize + size * _sizeChangeRate;
    }

    /// <summary>画面表示を切り替える</summary>
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