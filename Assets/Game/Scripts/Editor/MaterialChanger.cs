#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

public class MaterialChanger : EditorWindow
{
    static Material _material;
    static Vector2 _mousePos;


    [MenuItem("Tools/MaterialChanger/MaterialChanger Window")]
    static void Open()
    {
        GetWindow<MaterialChanger>();
    }

    private void OnGUI()
    {
        _material = EditorGUILayout.ObjectField("material", _material, typeof(Material), false) as Material;
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

    [MenuItem ("Tools/MaterialChanger/Change the Material of the Selected Object #g")]
    static void ChangingTheSelectedMaterial()
    {
        var picked = HandleUtility.PickGameObject(_mousePos, false);

        if (picked != null)
        {
            Debug.Log(picked.name);

            if (picked.TryGetComponent(out MeshRenderer meshR))
            {
                meshR.sharedMaterial = _material;
            }
        }
    }
}

#endif