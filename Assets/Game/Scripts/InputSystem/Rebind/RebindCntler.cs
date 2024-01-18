using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindCntler : MonoBehaviour
{
    [SerializeField, Tooltip("���o�C���h�Ώۂ�Action��")] string _actionName;
    [SerializeField, Tooltip("�ύX����BindIndex")] int _bindIndex;
    [SerializeField, Tooltip("���݂�Binding�̃p�X��\������e�L�X�g")] TMP_Text _pathText;

    InputAction _action;
    InputAction _escapeAction;
    InputActionRebindingExtensions.RebindingOperation _rebindOperation;
    string _waitingString = "Waiting for key...";

    public static string FindKeyName(string actionName) =>
        PlayerInput.Instance.GameInput.FindAction(actionName).GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
    public static int GetKeyBindIndex(string actionName) =>
        PlayerInput.Instance.GameInput.FindAction(actionName).GetBindingIndex();

    // ������
    private void Awake()
    {
        // InputAction�C���X�^���X��ێ����Ă���
        _action = PlayerInput.Instance.GameInput.FindAction(_actionName);
        _escapeAction = PlayerInput.Instance.GameInput.InGame.SettingSwitch;
        // �L�[�o�C���h�̕\���𔽉f����
        RefreshDisplay();
    }

    // ���o�C���h���J�n����
    public void StartRebinding()
    {
        if (_action == null) return;

        // �������o�C���h���Ȃ�A�����I�ɃL�����Z��
        // Cancel���\�b�h�����s����ƁAOnCancel�C�x���g�����΂���
        _rebindOperation?.Cancel();

        _action.Disable(); // ���o�C���h�O��Action�𖳌���
        _escapeAction.Disable();
        _pathText.text = _waitingString; // waiting�̕����ɕύX

        // ���o�C���h�̃I�y���[�V�������쐬�A�R�[���o�b�N�ݒ�A�J�n
        _rebindOperation = _action
            .PerformInteractiveRebinding(_bindIndex)
            .OnComplete(_ =>
            { // ���o�C���h�������������̏���
                RefreshDisplay();
                OnFinished();
            })
            .OnCancel(_ =>
            {�@// ���o�C���h���L�����Z�����ꂽ���̏���
                RefreshDisplay();
                OnFinished();
            })
            .WithCancelingThrough("<Keyboard>/escape")
            .Start(); // �����Ń��o�C���h���J�n����

        void OnFinished()
        {
            CleanUpOperation(); // �I�y���[�V�����̌㏈��
            _action.Enable(); // �ꎞ�I�ɖ���������Action��L��������
            _escapeAction.Enable();
        }
    }

    /// <summary>�㏑�����ꂽ�������Z�b�g����</summary>
    public void ResetOverrides()
    {
        // Binding�̏㏑����S�ĉ�������
        _action?.RemoveAllBindingOverrides();
        RefreshDisplay();
    }

    /// <summary>���݂̃L�[�o�C���h�\�����X�V</summary>
    public void RefreshDisplay()
    {
        if (_action == null || _pathText == null) return;

        _pathText.text = _action.GetBindingDisplayString(_bindIndex);
    }

    /// <summary>���o�C���h�I�y���[�V������j������</summary>
    private void CleanUpOperation()
    {
        // �I�y���[�V�������쐬������ADispose���Ȃ��ƃ��������[�N����
        _rebindOperation?.Dispose();
        _rebindOperation = null;
    }

    private void OnDestroy()
    {
        // �I�y���[�V�����͕K���j������K�v������
        CleanUpOperation();
    }
}

#if false
public static class InputRebinder
{
    static InputActionRebindingExtensions.RebindingOperation _rebindOperation = null;
    public static string FindKeyName(string actionName) =>
        GA.Input.Controller.FindAction(actionName).GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
    public static int GetKeyBindIndex(string actionName) =>
        GA.Input.Controller.FindAction(actionName).GetBindingIndex();

    public static void ReBind(string actionName, Action<string> displayAction)
    {
        GA.Input.Controller.Disable();
        _rebindOperation = GA.Input.Controller.FindAction(actionName).
            PerformInteractiveRebinding()
            .WithTargetBinding(GetKeyBindIndex(actionName))
            .OnComplete(_ => {
                displayAction(FindKeyName(actionName));
                FinishBinding();
            })
            .OnCancel(_ => FinishBinding())
            .Start();
    }
    static void FinishBinding()
    {
        _rebindOperation.Dispose();
        _rebindOperation = null;
        GA.Input.Controller.Enable();
    }
}
#endif