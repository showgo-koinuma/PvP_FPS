using UnityEngine;

public class SettingManager : MonoBehaviour
{
    [SerializeField] GameObject _settingCanvas;
    [Space(10)]


    static SettingManager _instance = default;
    public static SettingManager Instance { get => _instance; }

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

    void SwitchCanvas()
    {
        _settingCanvas.SetActive(!_settingCanvas.activeSelf);
    }
}