using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindCntler : MonoBehaviour
{
    [SerializeField, Tooltip("リバインド対象のAction名")] string _actionName;
    [SerializeField, Tooltip("変更するBindIndex")] int _bindIndex;
    [SerializeField, Tooltip("現在のBindingのパスを表示するテキスト")] TMP_Text _pathText;

    InputAction _action;
    InputAction _escapeAction;
    InputActionRebindingExtensions.RebindingOperation _rebindOperation;
    string _waitingString = "Waiting for key...";

    public static string FindKeyName(string actionName) =>
        PlayerInput.Instance.GameInput.FindAction(actionName).GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
    public static int GetKeyBindIndex(string actionName) =>
        PlayerInput.Instance.GameInput.FindAction(actionName).GetBindingIndex();

    // 初期化
    private void Awake()
    {
        // InputActionインスタンスを保持しておく
        _action = PlayerInput.Instance.GameInput.FindAction(_actionName);
        _escapeAction = PlayerInput.Instance.GameInput.InGame.SettingSwitch;
        // キーバインドの表示を反映する
        RefreshDisplay();
    }

    // リバインドを開始する
    public void StartRebinding()
    {
        if (_action == null) return;

        // もしリバインド中なら、強制的にキャンセル
        // Cancelメソッドを実行すると、OnCancelイベントが発火する
        _rebindOperation?.Cancel();

        _action.Disable(); // リバインド前にActionを無効化
        _escapeAction.Disable();
        _pathText.text = _waitingString; // waitingの文字に変更

        // リバインドのオペレーションを作成、コールバック設定、開始
        _rebindOperation = _action
            .PerformInteractiveRebinding(_bindIndex)
            .OnComplete(_ =>
            { // リバインドが完了した時の処理
                RefreshDisplay();
                OnFinished();
            })
            .OnCancel(_ =>
            {　// リバインドがキャンセルされた時の処理
                RefreshDisplay();
                OnFinished();
            })
            .WithCancelingThrough("<Keyboard>/escape")
            .Start(); // ここでリバインドを開始する

        void OnFinished()
        {
            CleanUpOperation(); // オペレーションの後処理
            _action.Enable(); // 一時的に無効化したActionを有効化する
            _escapeAction.Enable();
        }
    }

    /// <summary>上書きされた情報をリセットする</summary>
    public void ResetOverrides()
    {
        // Bindingの上書きを全て解除する
        _action?.RemoveAllBindingOverrides();
        RefreshDisplay();
    }

    /// <summary>現在のキーバインド表示を更新</summary>
    public void RefreshDisplay()
    {
        if (_action == null || _pathText == null) return;

        _pathText.text = _action.GetBindingDisplayString(_bindIndex);
    }

    /// <summary>リバインドオペレーションを破棄する</summary>
    private void CleanUpOperation()
    {
        // オペレーションを作成したら、Disposeしないとメモリリークする
        _rebindOperation?.Dispose();
        _rebindOperation = null;
    }

    private void OnDestroy()
    {
        // オペレーションは必ず破棄する必要がある
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