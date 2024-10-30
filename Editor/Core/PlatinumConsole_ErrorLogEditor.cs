using UnityEditor;
using UnityEngine;
using CobaPlatinum.DebugTools.ErrorLog;
using CobaPlatinum.TextUtilities;

[CustomEditor(typeof(PlatinumConsole_ErrorLog))]
public class PlatinumConsole_ErrorLogEditor : Editor
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
        GUILayout.Box("COBA PLATINUM CONSOLE - ERROR LOG", headerStyle, GUILayout.Width(Screen.width));
        GUILayout.FlexibleSpace();
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        DrawDefaultInspector();
    }
}
