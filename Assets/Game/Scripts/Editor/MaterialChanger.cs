#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class MaterialChanger : EditorWindow
{
    static Material _materialQ;
    static Material _materialE;
    static Material _materialR;
    static Material _materialF;
    static Vector2 _mousePos;


    [MenuItem("Tools/MaterialChanger/MaterialChanger Window")]
    static void Open()
    {
        GetWindow<MaterialChanger>();
    }

    private void OnGUI()
    {
        _materialQ = EditorGUILayout.ObjectField("Material Q", _materialQ, typeof(Material), false) as Material;
        _materialE = EditorGUILayout.ObjectField("Material E", _materialE, typeof(Material), false) as Material;
        _materialR = EditorGUILayout.ObjectField("Material R", _materialR, typeof(Material), false) as Material;
        _materialF = EditorGUILayout.ObjectField("Material F", _materialF, typeof(Material), false) as Material;
        wantsMouseMove = true;
    }

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        SceneView.duringSceneGui -= SceneViewOnGUI;
        SceneView.duringSceneGui += SceneViewOnGUI;
    }

    static void SceneViewOnGUI(SceneView sceneView)
    {
        _mousePos = Event.current.mousePosition;
    }

    [MenuItem ("Tools/MaterialChanger/Change Material on Q #q")]
    static void ChangingMaterialQ()
    {
        var picked = HandleUtility.PickGameObject(_mousePos, false);

        if (picked != null)
        {
            Debug.Log(picked.name);

            if (picked.TryGetComponent(out MeshRenderer meshR))
            {
                meshR.sharedMaterial = _materialQ;
            }
        }
    }
    [MenuItem("Tools/MaterialChanger/Change Material on E #e")]
    static void ChangingMaterialE()
    {
        var picked = HandleUtility.PickGameObject(_mousePos, false);

        if (picked != null)
        {
            Debug.Log(picked.name);

            if (picked.TryGetComponent(out MeshRenderer meshR))
            {
                meshR.sharedMaterial = _materialE;
            }
        }
    }
    [MenuItem("Tools/MaterialChanger/Change Material on R #r")]
    static void ChangingMaterialR()
    {
        var picked = HandleUtility.PickGameObject(_mousePos, false);

        if (picked != null)
        {
            Debug.Log(picked.name);

            if (picked.TryGetComponent(out MeshRenderer meshR))
            {
                meshR.sharedMaterial = _materialR;
            }
        }
    }
    [MenuItem("Tools/MaterialChanger/Change Material on F #c")]
    static void ChangingMaterialF()
    {
        var picked = HandleUtility.PickGameObject(_mousePos, false);

        if (picked != null)
        {
            Debug.Log(picked.name);

            if (picked.TryGetComponent(out MeshRenderer meshR))
            {
                meshR.sharedMaterial = _materialF;
            }
        }
    }
}
#endif