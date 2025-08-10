#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AlienFormationPlacer))]
public class AlienFormationPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AlienFormationPlacer placer = (AlienFormationPlacer)target;

        GUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Formation"))
        {
            // Record undo for the placer object itself (changes to children are undone in GenerateFormation)
            Undo.RecordObject(placer, "Generate Formation");
            placer.GenerateFormation();
        }
        if (GUILayout.Button("Clear Generated"))
        {
            Undo.RecordObject(placer, "Clear Generated");
            placer.ClearGenerated();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(4);
        EditorGUILayout.HelpBox("Editor notes:\n• Auto-update regenerates when you change inspector values.\n• If you spawn many prefabs, disable Auto Update and use Generate button.\n• ClearGenerated only removes objects created by this tool if 'Preserve Manual Children' is enabled.", MessageType.Info);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(placer);
        }
    }
}
#endif