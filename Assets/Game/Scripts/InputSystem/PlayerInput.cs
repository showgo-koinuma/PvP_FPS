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
    /// <summary>InputType�Ƃ���ɑΉ�����Action��Dictionary</summary>
    Dictionary<InputType, Action> _actionDic = new Dictionary<InputType, Action>();
    /// <summary>update�Ŏ��s������̂͑S�Ă����Ŏ��s����</summary>
    event Action _updateAction;

    // input�̏�Ԃ�ۑ����A�����Ŏ��s���邽�߂̂���
    Vector2 _lookRotation;
    public Vector2 LookRotation { get => _lookRotation; }
    Vector2 _inputMoveVector;
    public Vector2 InputMoveVector { get => _inputMoveVector; }
    bool _inputOnFire = false;
    public bool InputOnFire { get => _inputOnFire; }

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
        _gameInput.InGame.Jump.started += OnLookRotate;
        _gameInput.InGame.Look.performed += OnLookRotate;
        _gameInput.InGame.Look.canceled += OnLookRotate;
        _gameInput.InGame.Move.started += OnMove;
        _gameInput.InGame.Move.performed += OnMove;
        _gameInput.InGame.Move.canceled += OnMove;
        _gameInput.InGame.Jump.started += OnJump;
        _gameInput.InGame.Fire.started += OnFire;
        _gameInput.InGame.Fire.canceled += OnFire;
    }

    /// <summary>�R�[���o�b�N�ɓo�^����Action���Z�b�g�o����</summary>
    /// <param name="inputType"></param><param name="action"></param>
    public void SetInputAction(InputType inputType, Action action)
    {
        _actionDic[inputType] += action;
    }

    #region input�ɂ���Ē��ڃR�[���o�b�N�����
    void OnLookRotate(InputAction.CallbackContext context)
    {
        _lookRotation = context.ReadValue<Vector2>();
    }
    void OnMove(InputAction.CallbackContext context)
    {
        _inputMoveVector = context.ReadValue<Vector2>();
    }
    void OnJump(InputAction.CallbackContext context)
    {
        _actionDic[InputType.Jump]?.Invoke();
    }
    void OnFire(InputAction.CallbackContext context)
    {
        _inputOnFire = context.phase == InputActionPhase.Started;
    }
    #endregion

    /// <summary>update�ŌĂԂ��̂��Z�b�g����</summary>
    /// <param name="action"></param>
    public void SetUpdateAction(Action action)
    {
        _updateAction += action;
    }

    private void Update()
    {
        _updateAction?.Invoke();
    }
}

public enum InputType
{
    //Move, // dictionary���g��Ȃ��ꍇ�v��Ȃ��H
    Jump,
    //Fire,
}