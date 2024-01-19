using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [SerializeField] GameObject _settingCanvas;
    [Tooltip("SensSetting")]
    [SerializeField] TMP_InputField _horiText;
    [SerializeField] Slider _horiSlider;


    static SettingManager _instance = default;
    public static SettingManager Instance { get => _instance; }

    bool _cursolrVisible; // �J�����Ƃ��̃J�[�\���ݒ��ۑ����A����Ƃ��ɍĐݒ肷��

    // sens


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
    }

    public void HoriSensValueChanged(bool isInputField)
    {
        if (isInputField)
        {
            if (float.TryParse(_horiText.text, out float value))
            {
                _horiSlider.value = value;
            }
            else 
            {

            }
        }
        else
        {

        }
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