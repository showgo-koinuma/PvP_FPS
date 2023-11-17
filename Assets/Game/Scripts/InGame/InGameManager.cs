using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    static InGameManager _instance;
    public static InGameManager Instance { get => _instance; }
    public event Action UpdateAction;
    /// <summary>オブジェクトの参照を共有するため</summary>
    public Dictionary<int, GameObject> ViewGameObjects = new Dictionary<int, GameObject>();

    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else _instance = this;
    }

    private void Update()
    {
        UpdateAction?.Invoke();
    }
}