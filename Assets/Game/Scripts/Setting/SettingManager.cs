using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    [SerializeField] GameObject _settingCanvas;

    [Header("SensSettings")]
    [SerializeField] TMP_InputField _horiSensText;
    [SerializeField] Slider _horiSensSlider;
    [Space(5)]
    [SerializeField] TMP_InputField _verSensText;
    [SerializeField] Slider _verSensSlider;
    [Space(5)]
    [SerializeField] TMP_InputField _zoomSensText;
    [SerializeField] Slider _zoomSensSlider;

    [Header("BackButton")]
    [SerializeField] CustomButton _backButton;
    [SerializeField] CustomButton _quitButton;

    [Header("Sound")]
    [SerializeField] AudioClip _openSetting;
    [SerializeField] AudioClip _closeSetting;

    public CustomButton BackButton { get => _backButton; }
    public CustomButton QuitButton { get => _quitButton; }


    static SettingManager _instance = default;
    public static SettingManager Instance { get => _instance; }

    AudioSource _audioSource;

    bool _cursolrVisible; // 開いたときのカーソル設定を保存し、閉じるときに再設定する

    // sensの値がそれぞれ変更されたときに呼ばれる
    public Action<float> OnHoriSensChanged;
    public Action<float> OnVerSensChanged;
    public Action<float> OnZoomSensChanged;

    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        PlayerInput.Instance.SetInputAction(InputType.SettingSwitch, SwitchCanvas); // 切替アクションを登録
        _backButton.ButtonAction = SwitchCanvas;
        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary>Horizontal Sensの値を変更する</summary>
    public void HoriSensValueChange(bool isInputField)
    {
        if (isInputField) // input fieldで値を変更した場合
        {
            if (float.TryParse(_horiSensText.text, out float value)) // floatに出来る文字列か判定
            {
                _horiSensSlider.value = value; // sliderの値も連動する
                _horiSensText.text = _horiSensSlider.value.ToString("0.00");
            }
            else
            {
                // floatに出来ない場合、最も小さい値とする
                _horiSensSlider.value = _horiSensSlider.minValue;
                _horiSensText.text = _horiSensSlider.minValue.ToString();
            }
        }
        else // sliderで変更した場合
        {
            _horiSensText.text = _horiSensSlider.value.ToString("0.00"); // 小数第2位まで表示
        }

        OnHoriSensChanged?.Invoke(_horiSensSlider.value);
    }

    /// <summary>Verticl Sensの値を変更する</summary>
    public void VerSensValueChange(bool isInputField)
    {
        if (isInputField) // input fieldで値を変更した場合
        {
            if (float.TryParse(_verSensText.text, out float value)) // floatに出来る文字列か判定
            {
                _verSensSlider.value = value; // sliderの値も連動する
                _verSensText.text = _verSensSlider.value.ToString("0.00");
            }
            else
            {
                // floatに出来ない場合、最も小さい値とする
                _verSensSlider.value = _verSensSlider.minValue;
                _verSensText.text = _verSensSlider.minValue.ToString();
            }
        }
        else // sliderで変更した場合
        {
            _verSensText.text = _verSensSlider.value.ToString("0.00"); // 小数第2位まで表示
        }

        OnVerSensChanged?.Invoke(_verSensSlider.value);
    }

    /// <summary>Zoom Sensの値を変更する</summary>
    public void ZoomSensChange(bool isInputField)
    {
        if (isInputField) // input fieldで値を変更した場合
        {
            if (float.TryParse(_zoomSensText.text, out float value)) // floatに出来る文字列か判定
            {
                _zoomSensSlider.value = value; // sliderの値も連動する
                _zoomSensText.text = _zoomSensSlider.value.ToString("0.00");
            }
            else
            {
                // floatに出来ない場合、最も小さい値とする
                _zoomSensSlider.value = _zoomSensSlider.minValue;
                _zoomSensText.text = _zoomSensSlider.minValue.ToString();
            }
        }
        else // sliderで変更した場合
        {
            _zoomSensText.text = _zoomSensSlider.value.ToString("0.00"); // 小数第2位まで表示
        }

        OnZoomSensChanged?.Invoke(_zoomSensSlider.value);
    }

    public void SwitchCanvas()
    {
        if (_settingCanvas.activeSelf)
        {
            _settingCanvas.SetActive(false);
            _audioSource.PlayOneShot(_closeSetting);

            if (!_cursolrVisible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            _settingCanvas.SetActive(true);
            _audioSource.PlayOneShot(_openSetting);
            _cursolrVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _settingCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}