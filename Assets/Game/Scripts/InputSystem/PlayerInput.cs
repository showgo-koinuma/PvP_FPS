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
            if (!_instance) //nullならインスタンス化する
            {
                var obj = new GameObject("PlayerInput");
                var input = obj.AddComponent<PlayerInput>();
                _instance = input;
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }
    /// <summary>コールバック登録用</summary>
    GameInput _gameInput;
    /// <summary>InputTypeとそれに対応するActionのDictionary</summary>
    Dictionary<InputType, Action> _actionDic = new Dictionary<InputType, Action>();
    /// <summary>updateで実行するものは全てここで実行する</summary>
    event Action _updateAction;

    // inputの状態を保存し、条件で実行するためのもの
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

    /// <summary>初期化処理</summary>
    void Initialization()
    {
        for (int i = 0; i < Enum.GetValues(typeof(InputType)).Length; i++)
        {
            _actionDic.Add((InputType)i, null); // 初期化
        }

        // コールバックを登録していく TODO:操作が増えた場合書き足す必要がある
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

    /// <summary>コールバックに登録するActionをセット出来る</summary>
    /// <param name="inputType"></param><param name="action"></param>
    public void SetInputAction(InputType inputType, Action action)
    {
        _actionDic[inputType] += action;
    }

    #region inputによって直接コールバックされる
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

    /// <summary>updateで呼ぶものをセットする</summary>
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
    //Move, // dictionaryを使わない場合要らない？
    Jump,
    //Fire,
}