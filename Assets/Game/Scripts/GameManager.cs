using System;
using UnityEngine;

/// <summary>ƒQ[ƒ€‘S‘Ì‚ÌManager</summary>
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
