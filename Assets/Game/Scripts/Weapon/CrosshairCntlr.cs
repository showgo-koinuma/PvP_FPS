using UnityEngine;

public class CrosshairCntlr : MonoBehaviour
{
    RectTransform _rectTF;

    float _initialSize;
    /// <summary>サイズ変更にかかる時間</summary>
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

    /// <summary>サイズを反映させる</summary>
    void ReflectSize()
    {
        Vector2 currentSizeDelta = _rectTF.sizeDelta;
        Vector2 targetSizeDelta = new Vector2(_targetSize, _targetSize);

        // clampが必要ならclampする

        Vector2 smoothedSize = Vector2.Lerp(currentSizeDelta, targetSizeDelta, _timeItTake); // 遷移
        _rectTF.sizeDelta = smoothedSize; // 反映
    }

    /// <summary>サイズをセットする</summary>
    public void SetSize(float size)
    {
        _targetSize = _initialSize + size * 6;
    }

    /// <summary>画面表示を切り替える</summary>
    public void SwitchDisplay(bool isDisplay)
    {
        gameObject.SetActive(isDisplay);
    }
}
