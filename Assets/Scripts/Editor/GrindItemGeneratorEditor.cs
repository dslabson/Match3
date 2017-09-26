#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridItemsGenerator))]
public class GrindItemGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GridItemsGenerator script = (GridItemsGenerator)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            script.Awake();
            script.Start();
        }
    }
}
#endif