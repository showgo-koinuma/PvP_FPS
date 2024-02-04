using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    static PlayerInput _instance = default;
    public static PlayerInput Instance
    {
        get
        {
            if (!_instance) //null�Ȃ�C���X�^���X������
            {
                var obj = new GameObject("PlayerInput");
                var input = obj.AddComponent<PlayerInput>();
                _instance = input;
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    /// <summary>�R�[���o�b�N�o�^�p</summary>
    GameInput _gameInput;
    public GameInput GameInput { get => _gameInput; }
    /// <summary>InputType�Ƃ���ɑΉ�����Action��Dictionary</summary>
    Dictionary<InputType, Action> _actionDic = new Dictionary<InputType, Action>();

    // input�̏�Ԃ�ۑ����A�����Ŏ��s���邽�߂̂���
    Vector2 _lookRotation;
    public Vector2 LookRotation { get => _lookRotation; }
    bool _onForward;
    bool _onBack;
    bool _onLeft;
    bool _onRight;
    public Vector3 InputMoveVector
    {
        get => new Vector3(_onRight ? 1 : 0 + (_onLeft ? -1 : 0), 0, _onForward? 1 : 0 + (_onBack? -1 : 0));
    }
    bool _onJump;
    public bool OnJumpButton { get => _onJump; }
    bool _isCrouching = false;
    public bool IsCrouching { get => _isCrouching; }
    bool _inputOnFire = false;
    public bool InputOnFire { get => _inputOnFire; }
    bool _isADS = false;
    public bool IsADS { get => _isADS; }

    private void Awake()
    {
        _gameInput = new GameInput();
        _gameInput.Enable();
        Initialization();
    }

    /// <summary>����������</summary>
    void Initialization()
    {
        for (int i = 0; i < Enum.GetValues(typeof(InputType)).Length; i++)
        {
            _actionDic.Add((InputType)i, null); // ������
        }

        // �R�[���o�b�N��o�^���Ă��� TODO:���삪�������ꍇ���������K�v������
        _gameInput.InGame.Look.started += OnLookRotate;
        _gameInput.InGame.Look.performed += OnLookRotate;
        _gameInput.InGame.Look.canceled += OnLookRotate;
        _gameInput.InGame.Forward.started += OnForward;
        _gameInput.InGame.Forward.canceled += OnForward;
        _gameInput.InGame.Back.started += OnBackwards;
        _gameInput.InGame.Back.canceled += OnBackwards;
        _gameInput.InGame.Left.started += OnLeft;
        _gameInput.InGame.Left.canceled += OnLeft;
        _gameInput.InGame.Right.started += OnRight;
        _gameInput.InGame.Right.canceled += OnRight;
        _gameInput.InGame.Jump.started += OnJump;
        _gameInput.InGame.Jump.canceled += OnJump;
        _gameInput.InGame.Crouch.started += OnCrouch;
        _gameInput.InGame.Crouch.canceled += OnCrouch;
        _gameInput.InGame.Fire.started += OnFire;
        _gameInput.InGame.Fire.canceled += OnFire;
        _gameInput.InGame.Reload.started += OnReload;
        _gameInput.InGame.ADS.started += OnADS;
        _gameInput.InGame.ADS.canceled += OnADS;
        _gameInput.InGame.SwitchWeapon.started += OnSwitchWeapon;
        _gameInput.InGame.SettingSwitch.started += OnSettingSwitch;
    }

    /// <summary>�R�[���o�b�N�ɓo�^����Action���Z�b�g�o����</summary>
    /// <param name="inputType"></param><param name="action"></param>
    public void SetInputAction(InputType inputType, Action action)
    {
        _actionDic[inputType] += action;
    }
    public void DelInputAction(InputType inputType, Action action)
    {
        _actionDic[inputType] -= action;
    }

    #region input�ɂ���Ē��ڃR�[���o�b�N�����
    void OnLookRotate(InputAction.CallbackContext context)
    {
        _lookRotation = context.ReadValue<Vector2>();
    }
    void OnForward(InputAction.CallbackContext context)
    {
        _onForward = context.phase == InputActionPhase.Started;
    }
    void OnBackwards(InputAction.CallbackContext context)
    {
        _onBack = context.phase == InputActionPhase.Started;
    }
    void OnLeft(InputAction.CallbackContext context)
    {
        _onLeft = context.phase == InputActionPhase.Started;
    }
    void OnRight(InputAction.CallbackContext context)
    {
        _onRight = context.phase == InputActionPhase.Started;
    }
    void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started) _actionDic[InputType.Jump]?.Invoke();
        _onJump = context.phase == InputActionPhase.Started;
    }
    void OnCrouch(InputAction.CallbackContext context)
    {
        _isCrouching = context.phase == InputActionPhase.Started;
        _actionDic[InputType.Crouch]?.Invoke();
    }
    void OnFire(InputAction.CallbackContext context)
    {
        _inputOnFire = context.phase == InputActionPhase.Started;
    }
    void OnReload(InputAction.CallbackContext context)
    {
        _actionDic[InputType.Reload]?.Invoke();
    }
    void OnADS(InputAction.CallbackContext context)
    {
        _isADS = context.phase == InputActionPhase.Started;
        _actionDic[InputType.ADS]?.Invoke();
    }
    void OnSwitchWeapon(InputAction.CallbackContext context)
    {
        _actionDic[InputType.SwitchWeapon]?.Invoke();
    }
    void OnSettingSwitch(InputAction.CallbackContext context)
    {
        _actionDic[InputType.SettingSwitch]?.Invoke();
    }
    #endregion
}

public enum InputType
{
    //Move, // dictionary���g��Ȃ��ꍇ�v��Ȃ��H
    Jump,
    Crouch,
    //Fire,
    Reload,
    ADS,
    SwitchWeapon,
    SettingSwitch
}