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

    bool _cursolrVisible; // 開いたときのカーソル設定を保存し、閉じるときに再設定する

    // sens


    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        _settingCanvas.SetActive(false); // 設定画面は最初は表示しない
        PlayerInput.Instance.SetInputAction(InputType.SettingSwitch, SwitchCanvas); // 切替アクションを登録
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