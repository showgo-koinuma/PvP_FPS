using System;
using UnityEngine;

/// <summary>�Q�[���S�̂�Manager</summary>
public class GameManager : MonoBehaviour
{
    static GameManager _instance;
    public GameManager Instance
    {
        get
        {
            if (_instance) return _instance;
            else
            {
                DontDestroyOnLoad(gameObject);
                return _instance = this;
            }
        }
    }
}
