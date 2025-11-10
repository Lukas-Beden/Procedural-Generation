using Components.ProceduralGeneration;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MyMeshGenerator))]
public class MeshTesterEditor : Editor
{
    private Editor _meshGenEditor;

    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        // Update the serialized object
        serializedObject.Update();

        //// Display the _generationMethod field
        //SerializedProperty meshGenProp = serializedObject.FindProperty("_meshGenEditor");

        //// Display all fields inside the ScriptableObject
        //if (meshGenProp.objectReferenceValue != null)
        //{
        //    EditorGUILayout.Space(5);
        //    EditorGUILayout.LabelField("Generation Method Settings", EditorStyles.boldLabel);

        //    // Create a nested editor for the ScriptableObject
        //    CreateCachedEditor(meshGenProp.objectReferenceValue, null, ref _meshGenEditor);
        //    _meshGenEditor.OnInspectorGUI();
        //}

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();

        // Add some space
        EditorGUILayout.Space(10);

        // Get the target component
        MyMeshGenerator generator = (MyMeshGenerator)target;

        if (GUILayout.Button("Generate Grid", GUILayout.Height(30)))
        {
            if (Application.isPlaying)
            {
                generator.GenerateGrid();
            }
            else
            {
                EditorGUILayout.HelpBox("Grid generation can only be run in Play Mode!", MessageType.Warning);
            }
        }
    }
}
