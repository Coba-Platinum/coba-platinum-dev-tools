using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using CobaPlatinum.DebugTools.PlatinumConsole.DefaultExposedFields;
using CobaPlatinum.TextUtilities;

[CustomEditor(typeof(PlatinumConsole_DefaultExposedFields))]
public class CP_DefaultExposedFieldsEditor : Editor
{
    GUIStyle headerStyle;
    Color headerColor = TextUtils.UnnormalizedColor(0, 168, 255);
    public override void OnInspectorGUI()
    {
        headerStyle = new GUIStyle(GUI.skin.box);
        headerStyle.normal.background = Texture2D.whiteTexture; // must be white to tint properly
        headerStyle.normal.textColor = Color.white; // whatever you want
        headerStyle.fontSize = 14;


        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = headerColor;
        GUILayout.FlexibleSpace();
        GUILayout.Box("COBA PLATINUM CONSOLE - DEFAULT EXPOSED FIELDS", headerStyle, GUILayout.Width(Screen.width));
        GUILayout.FlexibleSpace();
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("This is a library of default exposed fields that can be used within the " +
            "\nCoba Platinum Debug Window. " +
            "\n\nDO NOT remove this component if you want to have access to the \ndefault exposed fields!");
        EditorGUILayout.EndVertical();
    }
}
