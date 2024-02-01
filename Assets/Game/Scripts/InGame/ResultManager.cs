using System.Collections;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    [Header("System")]
    [SerializeField] GameObject _systems;
    [SerializeField] Image _checkImage;

    [Header("Obj")]
    [SerializeField] RectTransform _KDObj;
    [SerializeField] RectTransform _damageObj;
    [SerializeField] RectTransform _accuracyObj;
    [SerializeField] RectTransform _HSPerObj;

    [Header("Text")]
    [SerializeField] TMP_Text _winLoseText;
    [SerializeField] TMP_Text _KDText;
    [SerializeField] TMP_Text _damageText;
    [SerializeField] TMP_Text _accuracyText;
    [SerializeField] TMP_Text _HSPerText;

    [Space(10)]
    [SerializeField] Color[] _winLoseColor;

    float[] _textPosX = new float[4];
    const float _initialPosX = 624.93f;

    public void InitializeResult(bool isWin, int kill, int death, int damage, int accuracy, int hs)
    {
        // set text pos
        _textPosX[0] = _KDObj.anchoredPosition.x;
        _textPosX[1] = _damageObj.anchoredPosition.x;
        _textPosX[2] = _accuracyObj.anchoredPosition.x;
        _textPosX[3] = _HSPerObj.anchoredPosition.x;
        _KDObj.anchoredPosition = new Vector3(_initialPosX, _KDObj.anchoredPosition.y, 0);
        _damageObj.anchoredPosition = new Vector3(_initialPosX, _damageObj.anchoredPosition.y, 0);
        _accuracyObj.anchoredPosition = new Vector3(_initialPosX, _accuracyObj.anchoredPosition.y, 0);
        _HSPerObj.anchoredPosition = new Vector3(_initialPosX, _HSPerObj.anchoredPosition.y, 0);

        // set text
        _winLoseText.text = isWin? "Victory" : "Defeat";
        _winLoseText.color = isWin? _winLoseColor[0] : _winLoseColor[1];
        _KDText.text = $"{kill} / {death}";
        _damageText.text = damage.ToString("#,0");
        _accuracyText.text = ((float)accuracy / 10).ToString();
        _HSPerText.text = ((float)hs / 10).ToString();

        _systems.SetActive(false);
        _checkImage.DOFade(0, 0);

        StartCoroutine(nameof(ResDataMove));
    }

    IEnumerator ResDataMove()
    {
        yield return new WaitForSeconds(1);
        _KDObj.DOAnchorPos(new Vector2(_textPosX[0], _KDObj.anchoredPosition.y), 1).SetEase(Ease.OutSine);
        yield return new WaitForSeconds(0.1f);
        _damageObj.DOAnchorPos(new Vector2(_textPosX[1], _damageObj.anchoredPosition.y), 1).SetEase(Ease.OutSine);
        yield return new WaitForSeconds(0.1f);
        _accuracyObj.DOAnchorPos(new Vector2(_textPosX[2], _accuracyObj.anchoredPosition.y), 1).SetEase(Ease.OutSine);
        yield return new WaitForSeconds(0.1f);
        _HSPerObj.DOAnchorPos(new Vector2(_textPosX[3], _HSPerObj.anchoredPosition.y), 1).SetEase(Ease.OutSine);

        yield return new WaitForSeconds(1);
        _systems.SetActive(true);
    }

    public void OtherIsContinue()
    {
        _checkImage.DOFade(1, 0);
    }
}
