using System;
using UnityEngine;

public class ShotGunAnimCntlr : MonoBehaviour
{
    public Action _finishCookingAction;
    public Action _insertShellAction;

    void InsertShell()
    {
        _insertShellAction?.Invoke();
    }

    void FinishCooking()
    {
        _finishCookingAction?.Invoke();
    }
}
