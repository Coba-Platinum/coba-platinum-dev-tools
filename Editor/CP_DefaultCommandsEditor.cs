using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using CobaPlatinum.DebugTools.Console.DefaultCommands;

[CustomEditor(typeof(CP_DefaultCommands))]
public class CP_DefaultCommandsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        GUILayout.Label("COBA PLATINUM CONSOLE - DEFAULT COMMANDS", EditorStyles.largeLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("This is a library of default commands that can be used within the " +
            "\nCoba Platinum cunsole in the Debug Window. " +
            "\n\nDO NOT remove this component if you want to have access to the \ndefault commands!");
        EditorGUILayout.EndVertical();
    }
}
