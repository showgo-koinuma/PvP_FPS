using UnityEngine;

public class InputTest : MonoBehaviour
{
    private void OnEnable()
    {
        //PlayerInput.Instance.SetUpdateAction(OnFire);
        PlayerInput.Instance.SetInputAction(InputType.Jump, OnJump);
        //PlayerInput.Instance.SetUpdateAction(OnMove);
    }

    void OnFire()
    {
        if (PlayerInput.Instance.InputOnFire)
        {
            Debug.Log("fire");
        }
    }

    void OnJump()
    {
        Debug.Log("jump");
    }

    void OnMove()
    {
        if (PlayerInput.Instance.InputMoveVector.magnitude != 0) Debug.Log(PlayerInput.Instance.InputMoveVector);
    }
}
