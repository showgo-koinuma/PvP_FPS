using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [SerializeField] GameObject _settingCanvas;
    [SerializeField] CustomButton _settingButton;
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


    static SettingManager _instance = default;
    public static SettingManager Instance { get => _instance; }

    bool _cursolrVisible; // �J�����Ƃ��̃J�[�\���ݒ��ۑ����A����Ƃ��ɍĐݒ肷��

    // sens�̒l�����ꂼ��ύX���ꂽ�Ƃ��ɌĂ΂��
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

        _settingCanvas.SetActive(false); // �ݒ��ʂ͍ŏ��͕\�����Ȃ�
        PlayerInput.Instance.SetInputAction(InputType.SettingSwitch, SwitchCanvas); // �ؑփA�N�V������o�^
        _settingButton.ButtonAction = SwitchCanvas;
        _backButton.ButtonAction = SwitchCanvas;
    }

    /// <summary>Horizontal Sens�̒l��ύX����</summary>
    public void HoriSensValueChange(bool isInputField)
    {
        if (isInputField) // input field�Œl��ύX�����ꍇ
        {
            if (float.TryParse(_horiSensText.text, out float value)) // float�ɏo���镶���񂩔���
            {
                _horiSensSlider.value = value; // slider�̒l���A������
                _horiSensText.text = _horiSensSlider.value.ToString("0.00");
            }
            else
            {
                // float�ɏo���Ȃ��ꍇ�A�ł��������l�Ƃ���
                _horiSensSlider.value = _horiSensSlider.minValue;
                _horiSensText.text = _horiSensSlider.minValue.ToString();
            }
        }
        else // slider�ŕύX�����ꍇ
        {
            _horiSensText.text = _horiSensSlider.value.ToString("0.00"); // ������2�ʂ܂ŕ\��
        }

        OnHoriSensChanged?.Invoke(_horiSensSlider.value);
    }

    /// <summary>Verticl Sens�̒l��ύX����</summary>
    public void VerSensValueChange(bool isInputField)
    {
        if (isInputField) // input field�Œl��ύX�����ꍇ
        {
            if (float.TryParse(_verSensText.text, out float value)) // float�ɏo���镶���񂩔���
            {
                _verSensSlider.value = value; // slider�̒l���A������
                _verSensText.text = _verSensSlider.value.ToString("0.00");
            }
            else
            {
                // float�ɏo���Ȃ��ꍇ�A�ł��������l�Ƃ���
                _verSensSlider.value = _verSensSlider.minValue;
                _verSensText.text = _verSensSlider.minValue.ToString();
            }
        }
        else // slider�ŕύX�����ꍇ
        {
            _verSensText.text = _verSensSlider.value.ToString("0.00"); // ������2�ʂ܂ŕ\��
        }

        OnVerSensChanged?.Invoke(_verSensSlider.value);
    }

    /// <summary>Zoom Sens�̒l��ύX����</summary>
    public void ZoomSensChange(bool isInputField)
    {
        if (isInputField) // input field�Œl��ύX�����ꍇ
        {
            if (float.TryParse(_zoomSensText.text, out float value)) // float�ɏo���镶���񂩔���
            {
                _zoomSensSlider.value = value; // slider�̒l���A������
                _zoomSensText.text = _zoomSensSlider.value.ToString("0.00");
            }
            else
            {
                // float�ɏo���Ȃ��ꍇ�A�ł��������l�Ƃ���
                _zoomSensSlider.value = _zoomSensSlider.minValue;
                _zoomSensText.text = _zoomSensSlider.minValue.ToString();
            }
        }
        else // slider�ŕύX�����ꍇ
        {
            _zoomSensText.text = _zoomSensSlider.value.ToString("0.00"); // ������2�ʂ܂ŕ\��
        }

        OnZoomSensChanged?.Invoke(_zoomSensSlider.value);
    }

    void SwitchCanvas()
    {
        if (_settingCanvas.activeSelf)
        {
            _settingCanvas.SetActive(false);

            if (!_cursolrVisible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            _settingCanvas.SetActive(true);
            _cursolrVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}